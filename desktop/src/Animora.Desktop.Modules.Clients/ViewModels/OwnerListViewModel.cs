using System.Collections.ObjectModel;
using Animora.Desktop.Modules.Clients.Handlers;
using Animora.Desktop.Modules.Clients.Models;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Clients.ViewModels;

/// <summary>
/// The owner list screen (DT-08): a virtualized, keyset-paged <c>DataGrid</c> over
/// <see cref="GetOwnerListQuery"/> (item 16), with the commands that open
/// <see cref="OwnerFormViewModel"/>'s route for either a create or an edit, plus the command that
/// opens <see cref="PatientListViewModel"/>'s route scoped to one owner — the phase 05 TODO's "one
/// patient-list route serves both modes" decision (AG-14, DESK-ARCH-05). Mirrors
/// <c>StaffListViewModel</c>'s shape.
/// </summary>
public sealed class OwnerListViewModel : ViewModelBase, INavigationAware
{
    /// <summary>Rail-visible navigation key this screen registers under (item 28/29 wire it).</summary>
    public const string RouteKey = "owner-list";

    // Comfortably under DT-08's 200-row virtualization threshold, so a single round trip fills a
    // typical window while keeping each page light — the same value StaffListViewModel uses.
    private const int PageSize = 50;

    private readonly IMediator _mediator;
    private readonly INavigationService _navigation;

    private string _searchTerm = string.Empty;
    private string? _afterId;
    private bool _isLoading;
    private bool _hasMore;

    public OwnerListViewModel(IMediator mediator, INavigationService navigation)
    {
        _mediator = mediator;
        _navigation = navigation;

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as Modules.Identity's screens).
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        CreateCommand = new RelayCommand(() => _navigation.NavigateTo(OwnerFormViewModel.RouteKey));
        EditCommand = new RelayCommand<Owner>(OpenForEdit);
        OpenPatientsCommand = new RelayCommand<Owner>(OpenPatients);
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
    public ObservableCollection<Owner> Items { get; } = [];

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

    public IAsyncRelayCommand SearchCommand { get; }

    public IAsyncRelayCommand LoadMoreCommand { get; }

    public IRelayCommand CreateCommand { get; }

    public IRelayCommand<Owner> EditCommand { get; }

    /// <summary>Opens <see cref="PatientListViewModel"/> scoped to the row's owner (item 25's
    /// owner-scoped list mode) — this screen's own way in, since the rail's patient-list entry (if
    /// any) always opens the global, unscoped mode instead.</summary>
    public IRelayCommand<Owner> OpenPatientsCommand { get; }

    /// <inheritdoc />
    public void OnNavigatedTo(object? parameter)
    {
        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10). A return visit
        // re-runs the search from scratch rather than showing a stale page from a previous visit.
        SearchCommand.Execute(null);
    }

    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            _afterId = null;

            var page = await _mediator
                .Send(new GetOwnerListQuery(SearchTerm, _afterId, PageSize), cancellationToken)
                .ConfigureAwait(true);

            Items.Clear();
            foreach (var owner in page.Items)
            {
                Items.Add(owner);
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
                .Send(new GetOwnerListQuery(SearchTerm, _afterId, PageSize), cancellationToken)
                .ConfigureAwait(true);

            foreach (var owner in page.Items)
            {
                Items.Add(owner);
            }

            _afterId = page.NextCursor;
            HasMore = page.NextCursor is not null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenForEdit(Owner? owner)
    {
        if (owner is not null)
        {
            _navigation.NavigateTo(OwnerFormViewModel.RouteKey, owner.Id);
        }
    }

    private void OpenPatients(Owner? owner)
    {
        if (owner is not null)
        {
            _navigation.NavigateTo(PatientListViewModel.RouteKey, owner.Id);
        }
    }
}
