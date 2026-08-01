using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Clients.Handlers;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Clients.ViewModels;

/// <summary>
/// The owner create/edit form (playbook steps 1/3/4): loads item 17's single-owner read (for an
/// edit), then dispatches <see cref="SaveOwnerCommand"/> (item 18). The navigation parameter selects
/// the mode — <see langword="null"/> for a create, an existing owner id for an edit (DESK-ARCH-05).
/// Mirrors <c>StaffFormViewModel</c>'s shape.
/// </summary>
public sealed class OwnerFormViewModel : ViewModelBase, INavigationAware
{
    /// <summary>Navigation key this screen registers under (item 28/29 wire it). Not rail-visible:
    /// reached only from <see cref="OwnerListViewModel"/>.</summary>
    public const string RouteKey = "owner-form";

    private readonly IMediator _mediator;
    private readonly INavigationService _navigation;
    private readonly TimeProvider _timeProvider;

    private Guid? _ownerId;
    private bool _isCreate = true;
    private string _fullName = string.Empty;
    private string _mobileNumber = string.Empty;
    private string? _landlineNumber;
    private string? _nationalId;
    private string? _address;
    private string? _city;
    private string? _notes;
    private DateTimeOffset _intakeDate;
    private string? _errorMessage;
    private bool _isSaving;

    public OwnerFormViewModel(IMediator mediator, INavigationService navigation, TimeProvider timeProvider)
    {
        _mediator = mediator;
        _navigation = navigation;
        _timeProvider = timeProvider;

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as the module's other screens).
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(() => _navigation.NavigateTo(OwnerListViewModel.RouteKey));
    }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public string HeaderTitle => _isCreate ? "افزودن صاحب حیوان جدید" : "ویرایش اطلاعات صاحب حیوان";

    public string SaveButtonText => _isCreate ? "ایجاد پرونده" : "ذخیره تغییرات";

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string MobileNumber
    {
        get => _mobileNumber;
        set => SetProperty(ref _mobileNumber, value);
    }

    public string? LandlineNumber
    {
        get => _landlineNumber;
        set => SetProperty(ref _landlineNumber, value);
    }

    public string? NationalId
    {
        get => _nationalId;
        set => SetProperty(ref _nationalId, value);
    }

    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string? City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    /// <summary>Clinic-internal only (see <c>IOwnerInput.Notes</c>'s own doc comment); nothing on
    /// this form marks it as owner-facing.</summary>
    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    /// <summary>
    /// <c>IOwnerInput.IntakeDateUtc</c> at the binding edge (DESK-ARCH-14): a <c>DatePicker</c>
    /// works in <see cref="DateTimeOffset"/>, not the domain's UTC <see cref="DateTime"/>, and
    /// <c>JalaliDateConverter</c> is deliberately one-way (display-only, CONV-05) — so this wrapper,
    /// not that converter, is where the two-way edit happens. Only the date part is kept; the domain
    /// field is a date stored at UTC midnight (CONV-04).
    /// </summary>
    public DateTimeOffset IntakeDate
    {
        get => _intakeDate;
        set => SetProperty(ref _intakeDate, value);
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
        _ownerId = parameter as Guid?;
        _isCreate = _ownerId is null;
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(SaveButtonText));

        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10).
        LoadCommand.Execute(null);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (_ownerId is { } ownerId)
        {
            // No DbContext/HttpClient here — Mediator queries are this ViewModel's only way to reach data (DT-02).
            var owner = await _mediator.Send(new GetOwnerQuery(ownerId), cancellationToken).ConfigureAwait(true);
            if (owner is not null)
            {
                FullName = owner.FullName;
                MobileNumber = owner.MobileNumber;
                LandlineNumber = owner.LandlineNumber;
                NationalId = owner.NationalId;
                Address = owner.Address;
                City = owner.City;
                Notes = owner.Notes;
                IntakeDate = new DateTimeOffset(owner.IntakeDateUtc, TimeSpan.Zero);
            }
        }
        else
        {
            // "the form pre-fills today's date" — IOwnerInput.IntakeDateUtc's own doc comment
            // (phase 05 TODO item 2's answer). TimeProvider rather than DateTime.UtcNow/Now (CONV-06).
            IntakeDate = new DateTimeOffset(_timeProvider.GetUtcNow().UtcDateTime.Date, TimeSpan.Zero);
        }
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var command = new SaveOwnerCommand(
                _ownerId,
                FullName,
                MobileNumber,
                string.IsNullOrWhiteSpace(LandlineNumber) ? null : LandlineNumber,
                string.IsNullOrWhiteSpace(NationalId) ? null : NationalId,
                string.IsNullOrWhiteSpace(Address) ? null : Address,
                string.IsNullOrWhiteSpace(City) ? null : City,
                string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                DateTime.SpecifyKind(IntakeDate.UtcDateTime.Date, DateTimeKind.Utc));

            var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(true);

            if (result.IsFailure)
            {
                ErrorMessage = ResolveErrorMessage(result.Error.Code);
                return;
            }

            _navigation.NavigateTo(OwnerListViewModel.RouteKey);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string ResolveErrorMessage(string code) => code switch
    {
        ClientsErrors.ValidationFailed => "اطلاعات وارد شده معتبر نیست؛ فیلدها را بررسی کنید.",
        _ => "ذخیره اطلاعات ناموفق بود؛ دوباره تلاش کنید.",
    };
}
