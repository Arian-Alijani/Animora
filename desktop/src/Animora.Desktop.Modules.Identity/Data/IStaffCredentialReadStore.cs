namespace Animora.Desktop.Modules.Identity.Data;

/// <summary>
/// One account's local sign-in secret, resolved by username (SEC-03).
/// </summary>
/// <param name="StaffId">The <see cref="Models.StaffMember.Id"/> the credential belongs to.</param>
/// <param name="Password">
/// The account's password in the clear, exactly as Stage A's seeded demo dataset holds it
/// (<c>IdentitySampleData</c>). Not a hash and not a shape real credential storage would ever take:
/// SEC-03 fixes <c>Argon2id</c> hashing as the server's job, applied over the network call this whole
/// seam stands in for until it exists.
/// </param>
public sealed record StaffCredential(Guid StaffId, string Password);

/// <summary>
/// The local credential-lookup seam behind <c>SignInHandler</c>, declared by the module that
/// consumes it (DIR-03 applied to the desktop).
/// </summary>
/// <remarks>
/// Unlike <see cref="IStaffReadStore"/>/<see cref="IRoleReadStore"/>, this seam is not something
/// Stage C rebinds to a local Dapper query: SEC-01 and SEC-03 both put credential verification on
/// the server, so the real implementation is a network call to the sign-in endpoint, not a local
/// store at all. Stage A's in-memory fake exists only so the login screen has something to click
/// through against before that endpoint exists.
/// </remarks>
// TODO(P2): replace every binding of this interface with a call to the server's sign-in endpoint;
// delete the interface and its Stage A fake once that call lands (DT-12, SEC-01, SEC-03).
public interface IStaffCredentialReadStore
{
    /// <summary>
    /// Resolves the credential entry for <paramref name="username"/>, or <see langword="null"/> when
    /// no account holds it — the case <c>SignInHandler</c> maps to
    /// <c>IdentityErrors.InvalidCredentials</c>.
    /// </summary>
    Task<StaffCredential?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
}
