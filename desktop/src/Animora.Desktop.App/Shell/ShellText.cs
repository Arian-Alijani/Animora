namespace Animora.Desktop.App.Shell;

/// <summary>
/// The Persian chrome labels the shell itself owns — brand block, rail group headings and the status
/// indicator's states. They are collected here rather than spread over the view models and
/// <c>ShellWindow.axaml</c> so the shell has exactly one text surface to swap when a localization
/// resource exists (AG-11); a module screen's labels stay with that module.
/// <para>
/// Static properties, not instance members: a stateless instance member fails this build
/// (CA1822 + <c>TreatWarningsAsErrors</c>), and XAML reaches them through <c>{x:Static}</c>.
/// </para>
/// </summary>
public static class ShellText
{
    public static string ProductName => "آنیمورا";

    public static string CommandCenterGroup => "مرکز کنترل";

    public static string ClinicManagementGroup => "مدیریت کلینیک";

    public static string FinancialOperationsGroup => "امور مالی";

    public static string StatusOnline => "متصل";

    public static string StatusOffline => "آفلاین";

    public static string StatusSyncing => "در حال همگام‌سازی";

    public static string StatusReadOnly => "فقط خواندنی";
}
