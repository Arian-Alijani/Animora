using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace Animora.Desktop.UI.Services;

/// <summary>
/// Single registration point for the Material Design icon set (DESK-ARCH-13). Phase 02's
/// composition root calls <see cref="Register"/> once (via <c>AddDesktopUi()</c>, item 32); no
/// module or view registers a provider itself, so `{ia:Icon mdi-...}` resolves consistently
/// everywhere. Icon sizing uses Theme/Tokens/Layout.axaml's IconSizeSmall/IconSize/IconSizeLarge —
/// no new size tokens are needed here.
/// </summary>
public static class IconProviderRegistrar
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        IconProvider.Current.Register<MaterialDesignIconProvider>();
        _registered = true;
    }
}
