using Animora.Desktop.UI.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Animora.Desktop.UI.Services;

/// <summary>
/// Composition entry point for this project (DIR-07): phase 02's composition root calls
/// <see cref="AddDesktopUi"/> exactly once and gets every design-system service this project owns,
/// without wiring each one individually. <see cref="IToastService"/> resolves once the caller also
/// registers the host-bound <c>INotificationManager</c> it depends on (constructed after the shell
/// window exists) — this method only adds the mapping, not that instance.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopUi(this IServiceCollection services)
    {
        IconProviderRegistrar.Register();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<PersianNumberFormatter>();
        services.TryAddSingleton<JalaliDateFormatter>();
        services.TryAddSingleton<MoneyFormatter>();

        services.TryAddSingleton<IDialogService, DialogService>();
        services.TryAddSingleton<IToastService, ToastService>();

        return services;
    }
}
