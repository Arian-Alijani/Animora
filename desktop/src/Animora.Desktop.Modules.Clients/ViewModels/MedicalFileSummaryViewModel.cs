using System.Globalization;
using Animora.Desktop.Modules.Clients.Handlers;
using Animora.Desktop.UI.Localization;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Clients.ViewModels;

/// <summary>
/// The medical-file summary screen (playbook step 1): renders item 22's
/// <see cref="GetMedicalFileSummaryQuery"/> result as a read-only header, with the visit-history
/// and lab/attachment links left as <c>TODO(P1-07)</c>/<c>TODO(P1-08)</c> markers rather than a
/// cross-module call (the phase 05 TODO header's "the medical file's links out stay markers"
/// decision, DT-01, CM-06) — see <see cref="HasVisitHistoryLink"/>/<see cref="HasAttachmentsLink"/>
/// below and <c>Views/MedicalFileSummaryView.axaml</c>'s own comment for where each marker sits.
/// Not rail-visible: reached only from <see cref="PatientListViewModel.OpenMedicalFileCommand"/>
/// (item 27's own addition to that already-landed screen, mirroring how item 23 added
/// <c>OwnerListViewModel.OpenPatientsCommand</c> ahead of <see cref="PatientListViewModel"/>
/// existing) and from its own <see cref="EditPatientCommand"/> round trip.
/// </summary>
public sealed class MedicalFileSummaryViewModel : ViewModelBase, INavigationAware
{
    /// <summary>Navigation key this screen registers under (item 28/29 wire it).</summary>
    public const string RouteKey = "medical-file-summary";

    // Calendar-average days per year, the same constant PatientFormViewModel.ComputeAgeYears uses
    // for the same "staff-entered approximate age is never more precise than this" reasoning.
    private const double DaysPerYear = 365.25;

    private readonly IMediator _mediator;
    private readonly INavigationService _navigation;
    private readonly TimeProvider _timeProvider;

    private Guid _patientId;
    private bool _isLoading;
    private bool _isNotFound;

    private string _patientName = string.Empty;
    private Guid _ownerId;
    private string _ownerDisplayName = string.Empty;
    private string _species = string.Empty;
    private string _sex = string.Empty;
    private string? _breed;
    private DateTime? _birthDateUtc;
    private bool _isBirthDateEstimated;
    private decimal? _weightKg;
    private bool _isSterilized;
    private string? _microchipId;
    private DateTime? _microchipImplantedAtUtc;
    private string? _color;
    private string? _temperament;
    private string? _housingType;
    private string? _diet;
    private string? _barcodeValue;
    private string? _surgicalHistory;

    public MedicalFileSummaryViewModel(IMediator mediator, INavigationService navigation, TimeProvider timeProvider)
    {
        _mediator = mediator;
        _navigation = navigation;
        _timeProvider = timeProvider;

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as the module's other screens).
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        EditPatientCommand = new RelayCommand(OpenPatientForEdit);
        BackCommand = new RelayCommand(() => _navigation.NavigateTo(PatientListViewModel.RouteKey));
    }

    public IAsyncRelayCommand LoadCommand { get; }

    /// <summary>Round-trips to <see cref="PatientFormViewModel"/> for this same patient (item 26's
    /// edit mode), returning here would require a second navigation — left to the user, since a
    /// save already returns to <see cref="PatientListViewModel"/>, not back to this screen.</summary>
    public IRelayCommand EditPatientCommand { get; }

    public IRelayCommand BackCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>True when <see cref="GetMedicalFileSummaryQuery"/> found no patient carrying the
    /// navigated id (e.g. it was reached with a stale/removed id) — shown instead of the header
    /// fields below rather than left blank.</summary>
    public bool IsNotFound
    {
        get => _isNotFound;
        private set => SetProperty(ref _isNotFound, value);
    }

    public string PatientName
    {
        get => _patientName;
        private set => SetProperty(ref _patientName, value);
    }

    public string OwnerDisplayName
    {
        get => _ownerDisplayName;
        private set => SetProperty(ref _ownerDisplayName, value);
    }

    public string Species
    {
        get => _species;
        private set => SetProperty(ref _species, value);
    }

    public string Sex
    {
        get => _sex;
        private set => SetProperty(ref _sex, value);
    }

    public string? Breed
    {
        get => _breed;
        private set => SetProperty(ref _breed, value);
    }

    /// <summary>
    /// The raw UTC birth date, bound through <c>JalaliDateConverter</c> at the View's own binding
    /// edge (DESK-ARCH-14) — this ViewModel never formats a date string itself.
    /// </summary>
    public DateTime? BirthDateUtc
    {
        get => _birthDateUtc;
        private set => SetProperty(ref _birthDateUtc, value);
    }

    public bool IsBirthDateEstimated
    {
        get => _isBirthDateEstimated;
        private set => SetProperty(ref _isBirthDateEstimated, value);
    }

    /// <summary>
    /// The age <see cref="BirthDateUtc"/> derives to, formatted for display — never stored, always
    /// recomputed from the read model's birth date (item 10's "age is derived ... at the UI binding
    /// edge and never stored" decision). <see langword="null"/> when <see cref="BirthDateUtc"/>
    /// itself is unknown, the same case <c>PatientFormViewModel</c>'s age input leaves blank.
    /// </summary>
    public string? AgeDisplay { get; private set; }

    /// <summary>
    /// The weight-over-time trend is Visits' own <c>BiometricReading</c> series
    /// (05-domain-model.md); this screen shows only the one current value and marks the chart's
    /// absence explicitly rather than silently omitting it — <c>TODO(P1-07)</c>
    /// (<c>IPatientInput.WeightKg</c>'s own doc comment, phase 05 TODO item 3's answer).
    /// </summary>
    public decimal? WeightKg
    {
        get => _weightKg;
        private set => SetProperty(ref _weightKg, value);
    }

    public bool IsSterilized
    {
        get => _isSterilized;
        private set => SetProperty(ref _isSterilized, value);
    }

    public string? MicrochipId
    {
        get => _microchipId;
        private set => SetProperty(ref _microchipId, value);
    }

    public DateTime? MicrochipImplantedAtUtc
    {
        get => _microchipImplantedAtUtc;
        private set => SetProperty(ref _microchipImplantedAtUtc, value);
    }

    public string? Color
    {
        get => _color;
        private set => SetProperty(ref _color, value);
    }

    public string? Temperament
    {
        get => _temperament;
        private set => SetProperty(ref _temperament, value);
    }

    public string? HousingType
    {
        get => _housingType;
        private set => SetProperty(ref _housingType, value);
    }

    public string? Diet
    {
        get => _diet;
        private set => SetProperty(ref _diet, value);
    }

    public string? BarcodeValue
    {
        get => _barcodeValue;
        private set => SetProperty(ref _barcodeValue, value);
    }

    public string? SurgicalHistory
    {
        get => _surgicalHistory;
        private set => SetProperty(ref _surgicalHistory, value);
    }

    // TODO(P1-07): visit history is Visits' own screen; once it exists, this becomes a real
    // NavigateTo(VisitHistoryViewModel.RouteKey, _patientId) command instead of a disabled marker
    // (the phase 05 TODO header's "links out stay markers" decision, DT-01, CM-06).
    /// <summary>Always <see langword="false"/> in this phase — the View shows the visit-history
    /// link disabled, never wired to a command, until phase 07 lands. Static, not an instance
    /// facade: it never reads instance state (CA1822 + <c>TreatWarningsAsErrors</c>), and the View
    /// reaches it through <c>{x:Static}</c> the same way <c>ShellText</c>'s members are reached.
    /// Becomes an instance property again once phase 07 gives it real per-patient state to
    /// read.</summary>
    public static bool HasVisitHistoryLink => false;

    // TODO(P1-08): attachments are Files' own screen; once it exists, this becomes a real
    // NavigateTo(...) command the same way as the visit-history marker above.
    /// <summary>Always <see langword="false"/> in this phase — the View shows the attachments link
    /// disabled, never wired to a command, until phase 08 lands. Static for the same CA1822 reason
    /// as <see cref="HasVisitHistoryLink"/>.</summary>
    public static bool HasAttachmentsLink => false;

    /// <inheritdoc />
    public void OnNavigatedTo(object? parameter)
    {
        _patientId = parameter as Guid? ?? Guid.Empty;

        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10).
        LoadCommand.Execute(null);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            // No DbContext/HttpClient here — Mediator queries are this ViewModel's only way to
            // reach data (DT-02), through the patient seam rather than a medical-file seam of its
            // own (item 22, AG-14, INV-18).
            var summary = await _mediator
                .Send(new GetMedicalFileSummaryQuery(_patientId), cancellationToken)
                .ConfigureAwait(true);

            IsNotFound = summary is null;
            if (summary is null)
            {
                return;
            }

            PatientName = summary.PatientName;
            _ownerId = summary.OwnerId;
            OwnerDisplayName = summary.OwnerDisplayName;
            Species = summary.Species;
            Sex = summary.Sex;
            Breed = summary.Breed;
            BirthDateUtc = summary.BirthDateUtc;
            IsBirthDateEstimated = summary.IsBirthDateEstimated;
            WeightKg = summary.WeightKg;
            IsSterilized = summary.IsSterilized;
            MicrochipId = summary.MicrochipId;
            MicrochipImplantedAtUtc = summary.MicrochipImplantedAtUtc;
            Color = summary.Color;
            Temperament = summary.Temperament;
            HousingType = summary.HousingType;
            Diet = summary.Diet;
            BarcodeValue = summary.BarcodeValue;
            SurgicalHistory = summary.SurgicalHistory;

            AgeDisplay = ComputeAgeDisplay(summary.BirthDateUtc, summary.IsBirthDateEstimated);
            OnPropertyChanged(nameof(AgeDisplay));
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string? ComputeAgeDisplay(DateTime? birthDateUtc, bool isEstimated)
    {
        if (birthDateUtc is not { } value)
        {
            return null;
        }

        var today = _timeProvider.GetUtcNow().UtcDateTime.Date;
        var years = Math.Round((today - value.Date).Days / DaysPerYear, 1);
        var formatted = PersianNumberFormatter.ToPersianDigits(
            years.ToString("0.#", CultureInfo.InvariantCulture));

        return isEstimated ? $"{formatted} سال (تخمینی)" : $"{formatted} سال";
    }

    private void OpenPatientForEdit()
    {
        _navigation.NavigateTo(
            PatientFormViewModel.RouteKey,
            new PatientFormNavigationParameter(_patientId, _ownerId));
    }
}
