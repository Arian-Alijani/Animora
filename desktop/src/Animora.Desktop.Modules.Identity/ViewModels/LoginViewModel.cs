using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Identity.Handlers;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Services;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Identity.ViewModels;

// TODO(P2): once a real sign-in endpoint exists, this screen still dispatches SignInQuery and still
// resolves the same IdentityErrors codes to Persian text below — only SignInHandler's body changes
// from a local lookup to the network call (DT-12, SEC-01, SEC-03).
/// <summary>
/// The login screen's credential form (playbook steps 1/3/4): dispatches <see cref="SignInQuery"/>
/// (item 19) and resolves its failure code to Persian text at this binding edge (CONV-12).
/// </summary>
public sealed class LoginViewModel : ViewModelBase
{
    /// <summary>Rail-visible navigation key this screen registers under (item 31 wires it). Rail-visible
    /// per the phase TODO's six-decision note: no pre-shell auth window exists in Stage A.</summary>
    public const string RouteKey = "login";

    private readonly IMediator _mediator;
    private readonly IToastService _toast;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string? _errorMessage;
    private bool _isSigningIn;

    public LoginViewModel(IMediator mediator, IToastService toast)
    {
        _mediator = mediator;
        _toast = toast;

        // Hand-built rather than [RelayCommand]: CommunityToolkit.Mvvm's generator is an analyzer
        // asset of a package this project only sees transitively (same reason as HomeViewModel and
        // the shell's own view model).
        SignInCommand = new AsyncRelayCommand(SignInAsync);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
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

    public bool IsSigningIn
    {
        get => _isSigningIn;
        private set
        {
            if (SetProperty(ref _isSigningIn, value))
            {
                OnPropertyChanged(nameof(CanSignIn));
            }
        }
    }

    public bool CanSignIn => !_isSigningIn;

    public IAsyncRelayCommand SignInCommand { get; }

    private async Task SignInAsync(CancellationToken cancellationToken)
    {
        IsSigningIn = true;
        ErrorMessage = null;

        try
        {
            // No DbContext/HttpClient here — the query is this ViewModel's only way to reach data (DT-02).
            var result = await _mediator
                .Send(new SignInQuery(Username, Password), cancellationToken)
                .ConfigureAwait(true);

            if (result.IsFailure)
            {
                ErrorMessage = ResolveErrorMessage(result.Error.Code);
                return;
            }

            // TODO(P1-33): once the sign-in projection notification exists, publish it here so
            // ICurrentUserState reflects result.Value instead of phase-02's fixed placeholder, and
            // navigate the shell to its landing route. Until then this screen only proves the
            // credential round trip; a non-blocking toast is the only visible success feedback
            // (DESK-ARCH-07 — never a modal spinner or blocking confirmation).
            _toast.ShowSuccess($"خوش‌آمدید، {result.Value.FullName}", "ورود موفق");
            Password = string.Empty;
        }
        finally
        {
            IsSigningIn = false;
        }
    }

    private static string ResolveErrorMessage(string code) => code switch
    {
        IdentityErrors.InvalidCredentials => "نام کاربری یا رمز عبور اشتباه است.",
        IdentityErrors.AccountInactive => "این حساب کاربری غیرفعال شده است؛ با مدیر کلینیک تماس بگیرید.",
        IdentityErrors.ValidationFailed => "نام کاربری و رمز عبور را به‌درستی وارد کنید.",
        _ => "ورود ناموفق بود؛ دوباره تلاش کنید.",
    };
}
