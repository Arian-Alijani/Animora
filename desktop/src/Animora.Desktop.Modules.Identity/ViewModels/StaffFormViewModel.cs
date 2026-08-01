using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Identity.Handlers;
using Animora.Desktop.Modules.Identity.Models;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Identity.ViewModels;

/// <summary>
/// The staff create/edit form (playbook steps 1/3/4): loads item 21's single-staff read (for an
/// edit), item 23's role list and the SEC-17 anchor lookup, then dispatches
/// <see cref="SaveStaffMemberCommand"/> (item 22). The navigation parameter selects the mode —
/// <see langword="null"/> for a create, an existing <see cref="StaffMember.Id"/> for an edit
/// (DESK-ARCH-05).
/// </summary>
public sealed class StaffFormViewModel : ViewModelBase, INavigationAware
{
    /// <summary>Navigation key this screen registers under (item 31 wires it). Not rail-visible:
    /// reached only from <see cref="StaffListViewModel"/>.</summary>
    public const string RouteKey = "staff-form";

    private readonly IMediator _mediator;
    private readonly INavigationService _navigation;

    private Guid? _staffId;
    private bool _isCreate = true;
    private string _fullName = string.Empty;
    private string _username = string.Empty;
    private string _usernameSuffix = string.Empty;
    private string _mobileNumber = string.Empty;
    private string? _email;
    private bool _isActive = true;
    private IReadOnlyList<Role> _roles = [];
    private Role? _selectedRole;
    private string? _ownerAdminUsername;
    private string? _errorMessage;
    private bool _isSaving;

    public StaffFormViewModel(IMediator mediator, INavigationService navigation)
    {
        _mediator = mediator;
        _navigation = navigation;

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as the other screens in this module).
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(() => _navigation.NavigateTo(StaffListViewModel.RouteKey));
    }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public string HeaderTitle => _isCreate ? "افزودن کارمند جدید" : "ویرایش اطلاعات کارمند";

    public string SaveButtonText => _isCreate ? "ایجاد کارمند" : "ذخیره تغییرات";

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    /// <summary>Freeform username field, bound when <see cref="ShowFreeformUsername"/> is true.</summary>
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    /// <summary>
    /// Suffix-only field beside the SEC-17 anchor prefix, bound when
    /// <see cref="ShowUsernamePrefix"/> is true.
    /// </summary>
    public string UsernameSuffix
    {
        get => _usernameSuffix;
        set => SetProperty(ref _usernameSuffix, value);
    }

    public string MobileNumber
    {
        get => _mobileNumber;
        set => SetProperty(ref _mobileNumber, value);
    }

    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public IReadOnlyList<Role> Roles
    {
        get => _roles;
        private set => SetProperty(ref _roles, value);
    }

    public Role? SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (SetProperty(ref _selectedRole, value))
            {
                OnPropertyChanged(nameof(ShowUsernamePrefix));
                OnPropertyChanged(nameof(ShowFreeformUsername));
            }
        }
    }

    /// <summary>
    /// The SEC-17 anchor: the tenant's current owner-admin username, or <see langword="null"/> when
    /// no staff member holds that role yet (the same bootstrap edge case
    /// <c>SaveStaffMemberHandler</c> leaves unchecked).
    /// </summary>
    public string? OwnerAdminUsername
    {
        get => _ownerAdminUsername;
        private set
        {
            if (SetProperty(ref _ownerAdminUsername, value))
            {
                OnPropertyChanged(nameof(ShowUsernamePrefix));
                OnPropertyChanged(nameof(ShowFreeformUsername));
            }
        }
    }

    /// <summary>
    /// Whether to render the SEC-17 anchor as a fixed prefix beside a suffix-only field, instead of
    /// one freeform <see cref="Username"/> field (item 28's UI rule): only for a create, and only
    /// when the selected role is not the system-seeded owner-admin role.
    /// </summary>
    public bool ShowUsernamePrefix =>
        _isCreate && _selectedRole is { IsSystemRole: false } && !string.IsNullOrWhiteSpace(_ownerAdminUsername);

    public bool ShowFreeformUsername => !ShowUsernamePrefix;

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
        _staffId = parameter as Guid?;
        _isCreate = _staffId is null;
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(SaveButtonText));

        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10).
        LoadCommand.Execute(null);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        // No DbContext/HttpClient here — Mediator queries are this ViewModel's only way to reach data (DT-02).
        Roles = await _mediator.Send(new GetRolesQuery(), cancellationToken).ConfigureAwait(true);
        OwnerAdminUsername = await _mediator.Send(new GetOwnerAdminUsernameQuery(), cancellationToken).ConfigureAwait(true);

        if (_staffId is { } staffId)
        {
            var staff = await _mediator.Send(new GetStaffMemberQuery(staffId), cancellationToken).ConfigureAwait(true);
            if (staff is not null)
            {
                FullName = staff.FullName;
                Username = staff.Username;
                MobileNumber = staff.MobileNumber;
                Email = staff.Email;
                IsActive = staff.IsActive;
                SelectedRole = Roles.FirstOrDefault(role => role.Id == staff.RoleId);
            }
        }
        else
        {
            IsActive = true;
        }
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var username = ShowUsernamePrefix ? $"{OwnerAdminUsername}-{UsernameSuffix}" : Username;

            var command = new SaveStaffMemberCommand(
                _staffId,
                FullName,
                username,
                MobileNumber,
                string.IsNullOrWhiteSpace(Email) ? null : Email,
                SelectedRole?.Id ?? Guid.Empty,
                IsActive);

            var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(true);

            if (result.IsFailure)
            {
                ErrorMessage = ResolveErrorMessage(result.Error.Code);
                return;
            }

            _navigation.NavigateTo(StaffListViewModel.RouteKey);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string ResolveErrorMessage(string code) => code switch
    {
        IdentityErrors.ValidationFailed => "اطلاعات وارد شده معتبر نیست؛ فیلدها را بررسی کنید.",
        IdentityErrors.UsernameAlreadyTaken => "این نام کاربری قبلاً برای حساب دیگری ثبت شده است.",
        IdentityErrors.RoleNotFound => "نقش انتخاب‌شده معتبر نیست.",
        IdentityErrors.SubordinateUsernamePrefixRequired => "نام کاربری باید با پیشوند نام کاربری مدیر کلینیک شروع شود.",
        _ => "ذخیره اطلاعات ناموفق بود؛ دوباره تلاش کنید.",
    };
}
