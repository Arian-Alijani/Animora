using System.Collections.ObjectModel;
using Animora.Desktop.Modules.Clients.Handlers;
using Animora.Desktop.Modules.Clients.Models;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Clients.ViewModels;

/// <summary>
/// The patient list screen (DT-08): one virtualized, keyset-paged <c>DataGrid</c> over
/// <see cref="GetPatientListQuery"/> (item 19) serving both the global list and any one owner's
/// scoped list through the navigation parameter — the phase 05 TODO header's "one patient-list
/// route serves both modes" decision (AG-14, DESK-ARCH-05, CONV-17) — with the owner scope shown as
/// a clearable header rather than a second screen. Mirrors <c>StaffListViewModel</c>'s shape.
/// </summary>
public sealed class PatientListViewModel : ViewModelBase, INavigationAware
{
    /// <summary>
    /// Rail-visible navigation key this screen registers under (item 28/29 wire it). Reached with no
    /// parameter from the rail (global mode) or with an <see cref="Owner.Id"/> from
    /// <see cref="OwnerListViewModel.OpenPatientsCommand"/> (owner-scoped mode).
    /// </summary>
    public const string RouteKey = "patient-list";

    // Comfortably under DT-08's 200-row virtualization threshold, so a single round trip fills a
    // typical window while keeping each page light — the same value StaffListViewModel uses.
    private const int PageSize = 50;

    private readonly IMediator _mediator;
    private readonly INavigationService _navigation;

    private Guid? _ownerId;
    private string _searchTerm = string.Empty;
    private string? _afterId;
    private string? _scopeOwnerName;
    private bool _isLoading;
    private bool _hasMore;

    public PatientListViewModel(IMediator mediator, INavigationService navigation)
    {
        _mediator = mediator;
        _navigation = navigation;

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as the module's other screens).
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        CreateCommand = new RelayCommand(OpenForCreate);
        EditCommand = new RelayCommand<Patient>(OpenForEdit);
        OpenMedicalFileCommand = new RelayCommand<Patient>(OpenMedicalFile);
        ClearScopeCommand = new AsyncRelayCommand(ClearScopeAsync);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    /// <summary>
    /// The grid's rows, appended to by <see cref="LoadMoreCommand"/> and replaced wholesale by
    /// <see cref="SearchCommand"/> — a mutable collection rather than a rebuilt list per page, since
    /// <c>DataGrid</c>'s virtualization observes <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>.
    /// </summary>
    public ObservableCollection<Patient> Items { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool HasMore
    {
        get => _hasMore;
        private set => SetProperty(ref _hasMore, value);
    }

    /// <summary>Whether this visit is the owner-scoped mode (<see cref="_ownerId"/> set), driving the
    /// clearable scope header's visibility.</summary>
    public bool IsScoped => _ownerId is not null;

    /// <summary>The scoped owner's <c>FullName</c>, loaded once per navigation via
    /// <see cref="GetOwnerQuery"/> so the header has a name even before the first row of an empty
    /// owner-scoped page loads.</summary>
    public string? ScopeOwnerName
    {
        get => _scopeOwnerName;
        private set => SetProperty(ref _scopeOwnerName, value);
    }

    public IAsyncRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand SearchCommand { get; }

    public IAsyncRelayCommand LoadMoreCommand { get; }

    public IRelayCommand CreateCommand { get; }

    public IRelayCommand<Patient> EditCommand { get; }

    /// <summary>
    /// Opens item 27's <see cref="MedicalFileSummaryViewModel"/> for the row's patient id — this
    /// screen's own entry point into that route, added by item 27 the same way item 23 added
    /// <see cref="OwnerListViewModel.OpenPatientsCommand"/> ahead of this screen existing.
    /// </summary>
    public IRelayCommand<Patient> OpenMedicalFileCommand { get; }

    /// <summary>Drops the owner scope and reloads as the global list, without a second navigation
    /// (the phase 05 TODO's "clearable scope header" wording for this item).</summary>
    public IAsyncRelayCommand ClearScopeCommand { get; }

    /// <inheritdoc />
    public void OnNavigatedTo(object? parameter)
    {
        _ownerId = parameter as Guid?;
        OnPropertyChanged(nameof(IsScoped));

        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10). A return visit
        // re-runs the search from scratch rather than showing a stale page from a previous visit.
        LoadCommand.Execute(null);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        await LoadScopeOwnerNameAsync(cancellationToken).ConfigureAwait(true);
        await SearchAsync(cancellationToken).ConfigureAwait(true);
    }

    private async Task LoadScopeOwnerNameAsync(CancellationToken cancellationToken)
    {
        if (_ownerId is not { } ownerId)
        {
            ScopeOwnerName = null;
            return;
        }

        var owner = await _mediator.Send(new GetOwnerQuery(ownerId), cancellationToken).ConfigureAwait(true);
        ScopeOwnerName = owner?.FullName;
    }

    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            _afterId = null;

            var page = await _mediator
                .Send(new GetPatientListQuery(_ownerId, SearchTerm, _afterId, PageSize), cancellationToken)
                .ConfigureAwait(true);

            Items.Clear();
            foreach (var patient in page.Items)
            {
                Items.Add(patient);
            }

            _afterId = page.NextCursor;
            HasMore = page.NextCursor is not null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMoreAsync(CancellationToken cancellationToken)
    {
        if (_afterId is null)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var page = await _mediator
                .Send(new GetPatientListQuery(_ownerId, SearchTerm, _afterId, PageSize), cancellationToken)
                .ConfigureAwait(true);

            foreach (var patient in page.Items)
            {
                Items.Add(patient);
            }

            _afterId = page.NextCursor;
            HasMore = page.NextCursor is not null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ClearScopeAsync(CancellationToken cancellationToken)
    {
        _ownerId = null;
        OnPropertyChanged(nameof(IsScoped));
        ScopeOwnerName = null;
        await SearchAsync(cancellationToken).ConfigureAwait(true);
    }

    private void OpenForCreate()
    {
        // The current scope, if any, pre-fills PatientFormViewModel's owner (item 26's "owner
        // pre-filled from the navigation parameter" rule); the global list passes null, and the form
        // picks an owner instead.
        _navigation.NavigateTo(PatientFormViewModel.RouteKey, new PatientFormNavigationParameter(null, _ownerId));
    }

    private void OpenForEdit(Patient? patient)
    {
        if (patient is not null)
        {
            _navigation.NavigateTo(
                PatientFormViewModel.RouteKey,
                new PatientFormNavigationParameter(patient.Id, null));
        }
    }

    private void OpenMedicalFile(Patient? patient)
    {
        if (patient is not null)
        {
            _navigation.NavigateTo(MedicalFileSummaryViewModel.RouteKey, patient.Id);
        }
    }
}
