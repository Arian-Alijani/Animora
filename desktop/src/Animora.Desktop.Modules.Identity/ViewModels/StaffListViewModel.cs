using System.Collections.ObjectModel;
using Animora.Desktop.Modules.Identity.Handlers;
using Animora.Desktop.Modules.Identity.Models;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Identity.ViewModels;

/// <summary>
/// The staff list screen (DT-08): a virtualized, keyset-paged <c>DataGrid</c> over
/// <see cref="GetStaffListQuery"/> (item 20), with the commands that open
/// <see cref="StaffFormViewModel"/>'s route for either a create or an edit.
/// </summary>
public sealed class StaffListViewModel : ViewModelBase, INavigationAware
{
    /// <summary>Rail-visible navigation key this screen registers under (item 31 wires it).</summary>
    public const string RouteKey = "staff-list";

    // Comfortably under DT-08's 200-row virtualization threshold, so a single round trip fills a
    // typical window while keeping each page light.
    private const int PageSize = 50;

    private readonly IMediator _mediator;
    private readonly INavigationService _navigation;

    private string _searchTerm = string.Empty;
    private string? _afterUsername;
    private bool _isLoading;
    private bool _hasMore;

    public StaffListViewModel(IMediator mediator, INavigationService navigation)
    {
        _mediator = mediator;
        _navigation = navigation;

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as HomeViewModel and LoginViewModel).
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        CreateCommand = new RelayCommand(() => _navigation.NavigateTo(StaffFormViewModel.RouteKey));
        EditCommand = new RelayCommand<StaffMember>(OpenForEdit);
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
    public ObservableCollection<StaffMember> Items { get; } = [];

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

    public IRelayCommand<StaffMember> EditCommand { get; }

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
            _afterUsername = null;

            var page = await _mediator
                .Send(new GetStaffListQuery(SearchTerm, _afterUsername, PageSize), cancellationToken)
                .ConfigureAwait(true);

            Items.Clear();
            foreach (var staff in page.Items)
            {
                Items.Add(staff);
            }

            _afterUsername = page.NextCursor;
            HasMore = page.NextCursor is not null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMoreAsync(CancellationToken cancellationToken)
    {
        if (_afterUsername is null)
        {
            return;
        }

        IsLoading = true;
        try
        {
            var page = await _mediator
                .Send(new GetStaffListQuery(SearchTerm, _afterUsername, PageSize), cancellationToken)
                .ConfigureAwait(true);

            foreach (var staff in page.Items)
            {
                Items.Add(staff);
            }

            _afterUsername = page.NextCursor;
            HasMore = page.NextCursor is not null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenForEdit(StaffMember? staff)
    {
        if (staff is not null)
        {
            _navigation.NavigateTo(StaffFormViewModel.RouteKey, staff.Id);
        }
    }
}
