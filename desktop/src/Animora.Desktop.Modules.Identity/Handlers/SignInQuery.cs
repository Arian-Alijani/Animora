using Animora.Desktop.Modules.Identity.Models;
using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Identity;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

// TODO(P2): once a real sign-in endpoint exists, this query still carries the submitted credential
// and SignInHandler still returns Result<SignedInStaff> with the same IdentityErrors codes — only
// the handler's body changes from a local lookup to the network call (DT-12, SEC-01, SEC-03).
/// <summary>
/// The login screen's one dispatch target (playbook step 3). Implements <see cref="ICredentialInput"/>
/// directly (CONV-18, INV-02) so <see cref="CredentialValidator"/> runs against the query itself.
/// </summary>
public sealed record SignInQuery(string Username, string Password)
    : ICredentialInput, IQuery<Result<SignedInStaff>>;
