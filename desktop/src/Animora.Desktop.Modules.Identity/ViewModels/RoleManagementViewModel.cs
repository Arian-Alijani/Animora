using System.Collections.ObjectModel;
using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Identity.Handlers;
using Animora.Desktop.Modules.Identity.Models;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Identity.ViewModels;

/// <summary>
/// One <see cref="PermissionCatalog"/> claim's checkbox state inside the role-management screen's
/// claim panel (SEC-09) — a thin, mutable projection over the catalog's static row plus this form's
/// current assignment, never a second source of the claim's identity or label (INV-18).
/// </summary>
public sealed class PermissionClaimOptionViewModel : ObservableObject
{
    private bool _isAssigned;

    public PermissionClaimOptionViewModel(PermissionClaim claim, bool isAssigned, bool isLocked)
    {
        Claim = claim;
        _isAssigned = isAssigned;
        IsLocked = isLocked;
    }

    public PermissionClaim Claim { get; }

    public string DisplayName => Claim.DisplayName;

    public bool IsAssigned
    {
        get => _isAssigned;
        set => SetProperty(ref _isAssigned, value);
    }

    /// <summary>
    /// Whether this checkbox is disabled because it is <see cref="PermissionCatalog.OwnerAdminProtectedClaimKey"/>
    /// on the tenant's system-seeded owner-admin role (SEC-11) — always assigned here, never
    /// clearable from this screen. <see cref="SaveRoleHandler"/> re-checks the same rule server-side
    /// of this fake, so this flag is a UX courtesy, not the enforcement point (INV-09).
    /// </summary>
    public bool IsLocked { get; }
}

/// <summary>
/// One <see cref="PermissionCatalog"/> module group in the claim-assignment panel (SEC-09), in the
/// catalog's own module order — never the role's own (arbitrary) claim-key order.
/// </summary>
public sealed record PermissionClaimGroupViewModel(string ModuleName, IReadOnlyList<PermissionClaimOptionViewModel> Claims);

/// <summary>
/// The role-management screen (playbook steps 1/3/4): a role list beside a claim-assignment form for
/// whichever role is selected, dispatching <see cref="GetRolesQuery"/> (item 23) and
/// <see cref="SaveRoleCommand"/> (item 24). Combined into one screen, unlike the staff list/form
/// pair, because a tenant's role count never approaches DT-08's virtualization threshold and the
/// claim panel is what an owner-admin actually spends time on (item 29's TODO note).
/// </summary>
public sealed class RoleManagementViewModel : ViewModelBase, INavigationAware
{
    /// <summary>Rail-visible navigation key this screen registers under (item 31 wires it).</summary>
    public const string RouteKey = "role-management";

    private readonly IMediator _mediator;

    private Guid? _roleId;
    private bool _isCreate = true;
    private string _displayName = string.Empty;
    private Role? _selectedRole;
    private IReadOnlyList<PermissionClaimGroupViewModel> _claimGroups;
    private string? _errorMessage;
    private bool _isLoading;
    private bool _isSaving;

    public RoleManagementViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _claimGroups = BuildClaimGroups([], isSystemRole: false);

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as the module's other screens).
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CreateCommand = new RelayCommand(BeginCreate);
    }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand CreateCommand { get; }

    /// <summary>The tenant's roles (SEC-09), unpaged per <see cref="Data.IRoleReadStore.GetAllAsync"/>.</summary>
    public ObservableCollection<Role> Roles { get; } = [];

    public Role? SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (SetProperty(ref _selectedRole, value) && value is not null)
            {
                LoadRoleIntoForm(value);
            }
        }
    }

    public string HeaderTitle => _isCreate ? "افزودن نقش جدید" : "ویرایش نقش";

    public string SaveButtonText => _isCreate ? "ایجاد نقش" : "ذخیره تغییرات";

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    /// <summary>
    /// The catalog's claims grouped by module (SEC-09), rebuilt whenever the form switches to a
    /// different role or starts a create — never mutated claim-by-claim from outside
    /// <see cref="PermissionClaimOptionViewModel.IsAssigned"/>.
    /// </summary>
    public IReadOnlyList<PermissionClaimGroupViewModel> ClaimGroups
    {
        get => _claimGroups;
        private set => SetProperty(ref _claimGroups, value);
    }

    /// <summary>Persian text for the last failed save, or <see langword="null"/> once cleared by a new attempt.</summary>
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

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

    /// <inheritdoc />
    public void OnNavigatedTo(object? parameter)
    {
        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10). A return visit
        // re-runs the list from scratch, same as StaffListViewModel.
        LoadCommand.Execute(null);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            var roles = await _mediator.Send(new GetRolesQuery(), cancellationToken).ConfigureAwait(true);

            Roles.Clear();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }

            // Keeps whichever role was selected before this reload (a save's own reload included)
            // in view, rather than resetting to the first row and surprising the owner-admin who is
            // still looking at the row they just edited.
            var roleToSelect = _roleId is { } roleId
                ? Roles.FirstOrDefault(role => role.Id == roleId)
                : Roles.FirstOrDefault();

            if (roleToSelect is not null)
            {
                SelectedRole = roleToSelect;
            }
            else
            {
                BeginCreate();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BeginCreate()
    {
        _roleId = null;
        _isCreate = true;

        // Direct field assignment, not the SelectedRole setter: deselecting the list must not
        // re-trigger LoadRoleIntoForm with the row being abandoned.
        _selectedRole = null;
        OnPropertyChanged(nameof(SelectedRole));

        DisplayName = string.Empty;
        ErrorMessage = null;
        ClaimGroups = BuildClaimGroups([], isSystemRole: false);
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    private void LoadRoleIntoForm(Role role)
    {
        _roleId = role.Id;
        _isCreate = false;
        DisplayName = role.DisplayName;
        ErrorMessage = null;
        ClaimGroups = BuildClaimGroups(role.PermissionClaimKeys, role.IsSystemRole);
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var claimKeys = ClaimGroups
                .SelectMany(group => group.Claims)
                .Where(claim => claim.IsAssigned)
                .Select(claim => claim.Claim.Key)
                .ToArray();

            var command = new SaveRoleCommand(_roleId, DisplayName, claimKeys);
            var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(true);

            if (result.IsFailure)
            {
                ErrorMessage = ResolveErrorMessage(result.Error.Code);
                return;
            }

            // The saved role's id becomes this form's anchor for the reload below, whether it was a
            // create (a fresh UUIDv7, INV-03) or an edit of the same row.
            _roleId = result.Value;
            await LoadAsync(cancellationToken).ConfigureAwait(true);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static IReadOnlyList<PermissionClaimGroupViewModel> BuildClaimGroups(
        IReadOnlyCollection<string> assignedKeys,
        bool isSystemRole)
    {
        var assigned = new HashSet<string>(assignedKeys, StringComparer.Ordinal);

        return PermissionCatalog.All
            .GroupBy(claim => claim.ModuleName)
            .Select(group => new PermissionClaimGroupViewModel(
                group.Key,
                group
                    .Select(claim =>
                    {
                        var isProtectedClaim = isSystemRole &&
                            string.Equals(claim.Key, PermissionCatalog.OwnerAdminProtectedClaimKey, StringComparison.Ordinal);

                        return new PermissionClaimOptionViewModel(
                            claim,
                            isAssigned: isProtectedClaim || assigned.Contains(claim.Key),
                            isLocked: isProtectedClaim);
                    })
                    .ToList()))
            .ToList();
    }

    private static string ResolveErrorMessage(string code) => code switch
    {
        IdentityErrors.ValidationFailed => "نام نقش و حداقل یک مجوز را به‌درستی وارد کنید.",
        IdentityErrors.UnknownPermissionClaimKey => "یکی از مجوزهای انتخاب‌شده در فهرست دسترسی‌ها موجود نیست.",
        IdentityErrors.SystemRoleClaimProtected => "دسترسی «مدیریت کارکنان» برای نقش مدیر کلینیک قابل حذف نیست.",
        _ => "ذخیره نقش ناموفق بود؛ دوباره تلاش کنید.",
    };
}
