using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Identity.Data;
using Animora.Desktop.Modules.Identity.Handlers;
using Animora.Desktop.Modules.Identity.Models;
using Animora.SharedKernel.Primitives;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Animora.Desktop.UnitTests.Modules.Identity;

/// <summary>
/// SignInHandler's three seams substituted the way GetHomeSummaryHandlerTests substitutes
/// IHomeSummaryReadStore (DT-02/DT-03): the handler takes only the interfaces the module itself
/// declared, so the local Stage A lookup — and CredentialValidator's structural gate ahead of it —
/// are exercised with no DbContext, connection or network call. The TODO(P2) sign-in endpoint this
/// seam stands in for is out of scope here (DT-12).
/// </summary>
public class SignInHandlerTests
{
    private static readonly Guid StaffId = Guid.CreateVersion7();
    private static readonly Guid RoleId = Guid.CreateVersion7();
    private const string Username = "petshop-drahmadi";
    private const string Password = "Petshop@123";

    private readonly IStaffCredentialReadStore _credentialReadStore = Substitute.For<IStaffCredentialReadStore>();
    private readonly IStaffReadStore _staffReadStore = Substitute.For<IStaffReadStore>();
    private readonly IRoleReadStore _roleReadStore = Substitute.For<IRoleReadStore>();
    private readonly SignInHandler _handler;

    public SignInHandlerTests()
    {
        _handler = new SignInHandler(_credentialReadStore, _staffReadStore, _roleReadStore);
    }

    [Fact]
    public async Task A_matching_username_and_password_succeeds_and_projects_the_role_claims()
    {
        StubKnownAccount(isActive: true);

        Result<SignedInStaff> result = await _handler.Handle(new SignInQuery(Username, Password), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.StaffId.Should().Be(StaffId);
        result.Value.FullName.Should().Be("دکتر سارا احمدی");
        result.Value.RoleId.Should().Be(RoleId);
        result.Value.RoleDisplayName.Should().Be("دامپزشک");
        result.Value.PermissionClaimKeys.Should().BeEquivalentTo(["patients.write"]);
    }

    [Fact]
    public async Task An_unknown_username_returns_the_generic_invalid_credentials_code()
    {
        _credentialReadStore.FindByUsernameAsync("no-such-user", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StaffCredential?>(null));

        Result<SignedInStaff> result = await _handler.Handle(new SignInQuery("no-such-user", Password), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.InvalidCredentials);
    }

    [Fact]
    public async Task A_wrong_password_for_a_real_account_returns_the_same_generic_code()
    {
        // Same code as an unknown username by design (SignInHandler's own remark): a distinct code
        // here would let the login form be used to enumerate valid usernames.
        StubKnownAccount(isActive: true);

        Result<SignedInStaff> result = await _handler.Handle(new SignInQuery(Username, "wrong-password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.InvalidCredentials);
    }

    [Fact]
    public async Task A_deactivated_account_returns_account_inactive()
    {
        StubKnownAccount(isActive: false);

        Result<SignedInStaff> result = await _handler.Handle(new SignInQuery(Username, Password), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.AccountInactive);
    }

    [Theory]
    [InlineData("", "Petshop@123")]
    [InlineData("petshop-drahmadi", "")]
    public async Task Malformed_input_fails_validation_before_any_seam_is_reached(string username, string password)
    {
        Result<SignedInStaff> result = await _handler.Handle(new SignInQuery(username, password), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(IdentityErrors.ValidationFailed);
        await _credentialReadStore.DidNotReceive().FindByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void StubKnownAccount(bool isActive)
    {
        _credentialReadStore.FindByUsernameAsync(Username, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StaffCredential?>(new StaffCredential(StaffId, Password)));

        _staffReadStore.GetByIdAsync(StaffId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StaffMember?>(new StaffMember(
                StaffId,
                "دکتر سارا احمدی",
                Username,
                "09123456789",
                null,
                RoleId,
                "دامپزشک",
                isActive)));

        _roleReadStore.GetByIdAsync(RoleId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Role?>(new Role(RoleId, "دامپزشک", ["patients.write"], MemberCount: 1, IsSystemRole: false)));
    }
}
