namespace Animora.SharedKernel.Validation.Identity;

/// <summary>
/// The property surface a sign-in command implements, so <see cref="CredentialValidator"/> runs
/// directly against the command (CONV-18, INV-02).
/// </summary>
/// <remarks>
/// Credentials are carried as plain strings for exactly as long as the handler needs them: hashing
/// and verification are the server's (SEC-03), and no value from this surface is ever persisted by
/// the desktop (SEC-07).
/// </remarks>
public interface ICredentialInput
{
    /// <summary>The sign-in identifier, matching <see cref="IStaffInput.Username"/>.</summary>
    string Username { get; }

    /// <summary>The submitted password, unhashed and never stored.</summary>
    string Password { get; }
}
