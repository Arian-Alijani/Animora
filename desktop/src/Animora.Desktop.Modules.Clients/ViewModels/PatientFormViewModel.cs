using System.Collections.ObjectModel;
using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Clients.Handlers;
using Animora.Desktop.Modules.Clients.Models;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using Animora.SharedKernel.Validation.Clients;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Clients.ViewModels;

/// <summary>
/// The navigation argument <see cref="PatientListViewModel"/> hands to this screen (DESK-ARCH-05):
/// which patient to load for an edit, and which owner (if any) to pre-fill for a create — the two
/// independent axes item 26 names, never conflated into one nullable id the way
/// <c>StaffFormViewModel</c>'s single id parameter could, since a create can carry an owner and an
/// edit never needs one supplied.
/// </summary>
/// <param name="PatientId"><see langword="null"/> for a create; the row being edited otherwise.</param>
/// <param name="OwnerId">
/// The pre-filled owner for a create reached from <see cref="OwnerListViewModel.OpenPatientsCommand"/>
/// or <see cref="PatientListViewModel"/>'s own owner-scoped mode; <see langword="null"/> for a
/// global-mode create, in which case the form's owner picker starts empty (item 26's "or picked when
/// absent" rule). Ignored for an edit — <see cref="GetPatientQuery"/>'s result already carries the
/// patient's actual owner.
/// </param>
public sealed record PatientFormNavigationParameter(Guid? PatientId, Guid? OwnerId);

/// <summary>
/// The patient create/edit form (playbook steps 1/3/4): loads item 20's single-patient read (for an
/// edit), then dispatches <see cref="SavePatientCommand"/> (item 21). The navigation parameter is a
/// <see cref="PatientFormNavigationParameter"/> selecting the mode and, for a create, the pre-filled
/// owner (DESK-ARCH-05). Mirrors <c>StaffFormViewModel</c>'s shape, with an owner picker and the
/// birth-date-or-age intake flow <c>IPatientInput.BirthDateUtc</c>'s own doc comment describes added
/// on top.
/// </summary>
public sealed class PatientFormViewModel : ViewModelBase, INavigationAware
{
    /// <summary>Navigation key this screen registers under (item 28/29 wire it). Not rail-visible:
    /// reached only from <see cref="PatientListViewModel"/>.</summary>
    public const string RouteKey = "patient-form";

    // Bounded well under DT-08's 200-row virtualization threshold: this picker is a quick-find aid
    // for a create/edit form, not the owner list screen itself, so a short result set is enough.
    private const int OwnerSearchLimit = 20;

    // Calendar-average days per Jalali/Gregorian year, the same constant both calendars converge on
    // over any few-year span — good enough for a staff-entered approximate age, which was never
    // more precise than "about N years" to begin with.
    private const double DaysPerYear = 365.25;

    private readonly IMediator _mediator;
    private readonly INavigationService _navigation;
    private readonly TimeProvider _timeProvider;

    private Guid? _patientId;
    private Guid? _preFilledOwnerId;
    private bool _isCreate = true;
    private string _name = string.Empty;
    private string _species = PatientValidator.AllowedSpecies.First();
    private string _sex = PatientValidator.AllowedSexes.First();
    private string? _breed;
    private bool _isBirthDateKnown = true;
    private DateTimeOffset? _birthDate;
    private decimal? _estimatedAgeYears;
    private decimal? _weightKg;
    private bool _isSterilized;
    private string? _microchipId;
    private DateTimeOffset? _microchipImplantedAt;
    private string? _color;
    private string? _temperament;
    private string? _housingType;
    private string? _diet;
    private string? _barcodeValue;
    private string? _surgicalHistory;

    private string _ownerSearchTerm = string.Empty;
    private Owner? _selectedOwner;
    private bool _isOwnerLocked;

    private string? _errorMessage;
    private bool _isSaving;

    public PatientFormViewModel(IMediator mediator, INavigationService navigation, TimeProvider timeProvider)
    {
        _mediator = mediator;
        _navigation = navigation;
        _timeProvider = timeProvider;

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as the module's other screens).
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(() => _navigation.NavigateTo(PatientListViewModel.RouteKey));
        SearchOwnersCommand = new AsyncRelayCommand(SearchOwnersAsync);
        UnlockOwnerCommand = new RelayCommand(() => IsOwnerLocked = false);
    }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public IAsyncRelayCommand SearchOwnersCommand { get; }

    public IRelayCommand UnlockOwnerCommand { get; }

    public string HeaderTitle => _isCreate ? "افزودن بیمار جدید" : "ویرایش اطلاعات بیمار";

    public string SaveButtonText => _isCreate ? "ایجاد بیمار" : "ذخیره تغییرات";

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>Registry the species <c>ComboBox</c> binds its <c>ItemsSource</c> to — the same
    /// list <see cref="PatientValidator"/> checks against, so the form can never offer a value the
    /// handler would reject (SH-05, INV-02). Static, not an instance facade: it never reads
    /// instance state (CA1822 + <c>TreatWarningsAsErrors</c>), and the View reaches it through
    /// <c>{x:Static}</c> the same way <c>ShellText</c>'s members are reached.</summary>
    public static IReadOnlyCollection<string> AllowedSpecies => PatientValidator.AllowedSpecies;

    public static IReadOnlyCollection<string> AllowedSexes => PatientValidator.AllowedSexes;

    public static IReadOnlyCollection<string> AllowedHousingTypes => PatientValidator.AllowedHousingTypes;

    public string Species
    {
        get => _species;
        set => SetProperty(ref _species, value);
    }

    public string Sex
    {
        get => _sex;
        set => SetProperty(ref _sex, value);
    }

    public string? Breed
    {
        get => _breed;
        set => SetProperty(ref _breed, value);
    }

    /// <summary>
    /// Selects which of the two mutually exclusive birth-date inputs below is authoritative — the
    /// "birth date or age, whichever is known" intake flow <c>IPatientInput.BirthDateUtc</c>'s own
    /// doc comment describes. <see langword="true"/> shows <see cref="BirthDate"/>'s
    /// <c>DatePicker</c>; <see langword="false"/> shows <see cref="EstimatedAgeYears"/>'s
    /// <c>NumericUpDown</c> instead.
    /// </summary>
    public bool IsBirthDateKnown
    {
        get => _isBirthDateKnown;
        set
        {
            if (SetProperty(ref _isBirthDateKnown, value))
            {
                OnPropertyChanged(nameof(IsBirthDateEstimatedInput));
            }
        }
    }

    /// <summary>
    /// Two-way facade over the negation of <see cref="IsBirthDateKnown"/> so the age-mode
    /// <c>RadioButton</c> can bind directly instead of needing a converter: setting it flips
    /// <see cref="IsBirthDateKnown"/>, and the reverse setter there keeps both buttons in one
    /// mutually exclusive pair in sync via the paired <c>OnPropertyChanged</c>.
    /// </summary>
    public bool IsBirthDateEstimatedInput
    {
        get => !_isBirthDateKnown;
        set => IsBirthDateKnown = !value;
    }

    /// <summary>
    /// <c>IPatientInput.BirthDateUtc</c> at the binding edge (DESK-ARCH-14) when
    /// <see cref="IsBirthDateKnown"/> is true. A <c>DatePicker</c> works in
    /// <see cref="DateTimeOffset"/>, not the domain's UTC <see cref="DateTime"/>, and
    /// <c>JalaliDateConverter</c> is deliberately one-way (CONV-05) — the same reasoning
    /// <c>OwnerFormViewModel.IntakeDate</c> already documents.
    /// </summary>
    public DateTimeOffset? BirthDate
    {
        get => _birthDate;
        set => SetProperty(ref _birthDate, value);
    }

    /// <summary>
    /// Staff-entered approximate age in years, used to derive an estimated
    /// <c>IPatientInput.BirthDateUtc</c> when <see cref="IsBirthDateKnown"/> is false — never stored
    /// as its own field, since age is always derived from a birth date, estimated or exact (phase 05
    /// TODO item 3's documented answer).
    /// </summary>
    public decimal? EstimatedAgeYears
    {
        get => _estimatedAgeYears;
        set => SetProperty(ref _estimatedAgeYears, value);
    }

    public decimal? WeightKg
    {
        get => _weightKg;
        set => SetProperty(ref _weightKg, value);
    }

    public bool IsSterilized
    {
        get => _isSterilized;
        set => SetProperty(ref _isSterilized, value);
    }

    public string? MicrochipId
    {
        get => _microchipId;
        set => SetProperty(ref _microchipId, value);
    }

    public DateTimeOffset? MicrochipImplantedAt
    {
        get => _microchipImplantedAt;
        set => SetProperty(ref _microchipImplantedAt, value);
    }

    public string? Color
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }

    public string? Temperament
    {
        get => _temperament;
        set => SetProperty(ref _temperament, value);
    }

    public string? HousingType
    {
        get => _housingType;
        set => SetProperty(ref _housingType, value);
    }

    public string? Diet
    {
        get => _diet;
        set => SetProperty(ref _diet, value);
    }

    public string? BarcodeValue
    {
        get => _barcodeValue;
        set => SetProperty(ref _barcodeValue, value);
    }

    public string? SurgicalHistory
    {
        get => _surgicalHistory;
        set => SetProperty(ref _surgicalHistory, value);
    }

    public string OwnerSearchTerm
    {
        get => _ownerSearchTerm;
        set => SetProperty(ref _ownerSearchTerm, value);
    }

    /// <summary>The owner quick-find picker's current result set (item 26's "or picked when
    /// absent" rule) — never the full owner list, per <see cref="OwnerSearchLimit"/>.</summary>
    public ObservableCollection<Owner> OwnerSearchResults { get; } = [];

    public Owner? SelectedOwner
    {
        get => _selectedOwner;
        set
        {
            if (SetProperty(ref _selectedOwner, value))
            {
                OnPropertyChanged(nameof(HasSelectedOwner));
            }
        }
    }

    public bool HasSelectedOwner => _selectedOwner is not null;

    /// <summary>
    /// True while the owner is the one <see cref="PatientFormNavigationParameter.OwnerId"/>
    /// pre-filled — the picker chrome stays hidden behind a single "تغییر" (change) button instead
    /// of exposing search UI the staff member has no reason to touch, until they ask for it.
    /// </summary>
    public bool IsOwnerLocked
    {
        get => _isOwnerLocked;
        private set => SetProperty(ref _isOwnerLocked, value);
    }

    /// <summary>Persian text for the last failed attempt, or <see langword="null"/> once cleared by a new one.</summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

    /// <inheritdoc />
    public void OnNavigatedTo(object? parameter)
    {
        var navigationParameter = parameter as PatientFormNavigationParameter ?? new PatientFormNavigationParameter(null, null);
        _patientId = navigationParameter.PatientId;
        _preFilledOwnerId = navigationParameter.OwnerId;
        _isCreate = _patientId is null;
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(SaveButtonText));

        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10). The
        // navigation parameter itself is not passed to the command: AsyncRelayCommand's parameterless
        // overload ignores whatever ICommand.Execute(object?) is called with, so both fields above
        // are set first and LoadAsync reads them back instead.
        LoadCommand.Execute(null);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (_patientId is { } patientId)
        {
            // No DbContext/HttpClient here — Mediator queries are this ViewModel's only way to reach data (DT-02).
            var patient = await _mediator.Send(new GetPatientQuery(patientId), cancellationToken).ConfigureAwait(true);
            if (patient is not null)
            {
                Name = patient.Name;
                Species = patient.Species;
                Sex = patient.Sex;
                Breed = patient.Breed;
                WeightKg = patient.WeightKg;
                IsSterilized = patient.IsSterilized;
                MicrochipId = patient.MicrochipId;
                MicrochipImplantedAt = patient.MicrochipImplantedAtUtc is { } implantedAt
                    ? new DateTimeOffset(implantedAt, TimeSpan.Zero)
                    : null;
                Color = patient.Color;
                Temperament = patient.Temperament;
                HousingType = patient.HousingType;
                Diet = patient.Diet;
                BarcodeValue = patient.BarcodeValue;
                SurgicalHistory = patient.SurgicalHistory;

                LoadBirthDate(patient.BirthDateUtc, patient.IsBirthDateEstimated);

                var owner = await _mediator.Send(new GetOwnerQuery(patient.OwnerId), cancellationToken).ConfigureAwait(true);
                SelectedOwner = owner;
                IsOwnerLocked = false;
            }
        }
        else if (_preFilledOwnerId is { } preFilledOwnerId)
        {
            var owner = await _mediator.Send(new GetOwnerQuery(preFilledOwnerId), cancellationToken).ConfigureAwait(true);
            SelectedOwner = owner;
            IsOwnerLocked = owner is not null;
        }
    }

    private void LoadBirthDate(DateTime? birthDateUtc, bool isEstimated)
    {
        if (birthDateUtc is not { } value)
        {
            IsBirthDateKnown = true;
            BirthDate = null;
            EstimatedAgeYears = null;
            return;
        }

        if (isEstimated)
        {
            IsBirthDateKnown = false;
            EstimatedAgeYears = ComputeAgeYears(value);
            BirthDate = null;
        }
        else
        {
            IsBirthDateKnown = true;
            BirthDate = new DateTimeOffset(value, TimeSpan.Zero);
            EstimatedAgeYears = null;
        }
    }

    private decimal ComputeAgeYears(DateTime birthDateUtc)
    {
        var today = _timeProvider.GetUtcNow().UtcDateTime.Date;
        var days = (today - birthDateUtc.Date).Days;
        return Math.Round((decimal)(days / DaysPerYear), 1);
    }

    private async Task SearchOwnersAsync(CancellationToken cancellationToken)
    {
        var page = await _mediator
            .Send(new GetOwnerListQuery(OwnerSearchTerm, null, OwnerSearchLimit), cancellationToken)
            .ConfigureAwait(true);

        OwnerSearchResults.Clear();
        foreach (var owner in page.Items)
        {
            OwnerSearchResults.Add(owner);
        }
    }

    /// <summary>
    /// Resolves <see cref="BirthDate"/>/<see cref="EstimatedAgeYears"/> into the single
    /// <c>(BirthDateUtc, IsBirthDateEstimated)</c> pair <see cref="SavePatientCommand"/> carries —
    /// the point where the "birth date or age, whichever is known" intake flow collapses back into
    /// one number so the two can never drift apart (<c>IPatientInput.BirthDateUtc</c>'s own doc
    /// comment).
    /// </summary>
    private (DateTime? BirthDateUtc, bool IsEstimated) ResolveBirthDate()
    {
        if (IsBirthDateKnown)
        {
            return BirthDate is { } known
                ? (DateTime.SpecifyKind(known.UtcDateTime.Date, DateTimeKind.Utc), false)
                : (null, false);
        }

        if (EstimatedAgeYears is not { } ageYears)
        {
            return (null, false);
        }

        var today = _timeProvider.GetUtcNow().UtcDateTime.Date;
        var estimatedBirthDate = today.AddDays(-(double)(ageYears * (decimal)DaysPerYear));
        return (DateTime.SpecifyKind(estimatedBirthDate, DateTimeKind.Utc), true);
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var (birthDateUtc, isBirthDateEstimated) = ResolveBirthDate();

            var command = new SavePatientCommand(
                _patientId,
                SelectedOwner?.Id ?? Guid.Empty,
                Name,
                Species,
                Sex,
                string.IsNullOrWhiteSpace(Breed) ? null : Breed,
                birthDateUtc,
                WeightKg,
                string.IsNullOrWhiteSpace(MicrochipId) ? null : MicrochipId,
                MicrochipImplantedAt is { } implantedAt
                    ? DateTime.SpecifyKind(implantedAt.UtcDateTime.Date, DateTimeKind.Utc)
                    : null,
                string.IsNullOrWhiteSpace(Color) ? null : Color,
                string.IsNullOrWhiteSpace(Temperament) ? null : Temperament,
                string.IsNullOrWhiteSpace(HousingType) ? null : HousingType,
                string.IsNullOrWhiteSpace(Diet) ? null : Diet,
                string.IsNullOrWhiteSpace(BarcodeValue) ? null : BarcodeValue,
                string.IsNullOrWhiteSpace(SurgicalHistory) ? null : SurgicalHistory,
                isBirthDateEstimated,
                IsSterilized);

            var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(true);

            if (result.IsFailure)
            {
                ErrorMessage = ResolveErrorMessage(result.Error.Code);
                return;
            }

            _navigation.NavigateTo(PatientListViewModel.RouteKey);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string ResolveErrorMessage(string code) => code switch
    {
        ClientsErrors.ValidationFailed => "اطلاعات وارد شده معتبر نیست؛ فیلدها را بررسی کنید.",
        ClientsErrors.OwnerNotFound => "صاحب حیوان انتخاب‌شده معتبر نیست؛ یک صاحب حیوان انتخاب کنید.",
        _ => "ذخیره اطلاعات ناموفق بود؛ دوباره تلاش کنید.",
    };
}
