namespace Animora.Desktop.Modules.Identity.Data;

// TODO(P2): delete this type once SignInHandler calls the server's sign-in endpoint instead
// (DT-12, SEC-01, SEC-03) — see IStaffCredentialReadStore's own TODO(P2).
/// <summary>Satisfies <see cref="IStaffCredentialReadStore"/> over <see cref="IdentitySampleData"/>.</summary>
internal sealed class InMemoryStaffCredentialReadStore : IStaffCredentialReadStore
{
    private readonly IdentitySampleData _sampleData;

    public InMemoryStaffCredentialReadStore(IdentitySampleData sampleData)
    {
        _sampleData = sampleData;
    }

    public Task<StaffCredential?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            var account = _sampleData.Staff.FirstOrDefault(
                candidate => string.Equals(candidate.Username, username, StringComparison.Ordinal));

            if (account is null || !_sampleData.PasswordsByStaffId.TryGetValue(account.Id, out var password))
            {
                return Task.FromResult<StaffCredential?>(null);
            }

            return Task.FromResult<StaffCredential?>(new StaffCredential(account.Id, password));
        }
    }
}
