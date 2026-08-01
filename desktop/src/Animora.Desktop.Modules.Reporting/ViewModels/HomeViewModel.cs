using Animora.Desktop.Modules.Reporting.Handlers;
using Animora.Desktop.Modules.Reporting.Models;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Reporting.ViewModels;

// TODO(P1-10): grow this into the real dashboard (hero, charts, activity lists) against the same
// query; the landing route, its data seam and this file's location do not move with it.
public sealed class HomeViewModel : ViewModelBase, INavigationAware
{
    private readonly IMediator _mediator;

    private HomeSummary? _summary;

    public HomeViewModel(IMediator mediator)
    {
        _mediator = mediator;

        // Hand-built command rather than [RelayCommand]: CommunityToolkit.Mvvm's generator is an
        // analyzer asset of a package this project only sees transitively, so the attribute is not
        // available here (same reason as the shell's own view model).
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    /// <summary>
    /// The screen's single read. Exposed rather than kept private so a later phase's refresh
    /// affordance binds to it instead of adding a second load path.
    /// </summary>
    public IAsyncRelayCommand LoadCommand { get; }

    /// <summary>
    /// The read model as the handler returned it — storage units, UTC instants. Persian digits, Toman
    /// conversion and Jalali dates are applied by the converters <c>HomeView.axaml</c> binds through,
    /// which keeps them at the binding edge and out of this type (DESK-ARCH-14).
    /// </summary>
    public HomeSummary? Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    /// <inheritdoc />
    public void OnNavigatedTo(object? parameter)
    {
        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10), and the command
        // owns the resulting task. The Home route takes no parameter, so nothing is read from it.
        LoadCommand.Execute(null);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        // No DbContext/HttpClient here — the query is the ViewModel's only way to reach data (DT-02).
        HomeSummary summary = await _mediator
            .Send(new GetHomeSummaryQuery(), cancellationToken)
            // Continue on the UI thread: the assignment below raises PropertyChanged for bound views.
            .ConfigureAwait(true);

        Summary = summary;
    }
}
