using Animora.Desktop.Modules.Identity.Models;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Published by <see cref="ViewModels.LoginViewModel"/> once <see cref="SignInQuery"/> (item 19)
/// succeeds (item 33), carrying the same <see cref="SignedInStaff"/> projection the query returns.
/// </summary>
/// <remarks>
/// Declared in this module rather than in <c>App/AppState</c>: a module may never reference the
/// composition root (DT-01, AT-09), so the notification has to live on the side that direction does
/// allow — <c>App/AppState/CurrentUserState</c>'s handler references this module instead, the same
/// way the composition root already does to register its routes.
/// </remarks>
public sealed record StaffSignedInNotification(SignedInStaff Staff) : INotification;
