namespace Animora.Desktop.UI.Navigation;

/// <summary>
/// The rail's nav groups (design-reference.md §6, the three `Overline` group labels observed on the
/// sidebar). Declaration order is the rail's top-to-bottom group order, so the registry can sort by
/// enum value and needs no separate group-order field. A route that is not rail-visible (detail or
/// dialog screens reached from another screen) carries no group at all.
/// </summary>
public enum RailGroup
{
    CommandCenter,
    ClinicManagement,
    FinancialOperations,
}
