using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.ArchTests;

// DT-06 / PHASE.md criterion 5: every literal color, font size, font family and corner radius in
// Animora.Desktop.UI lives in Theme/Tokens/ as a named resource; every other .axaml file only ever
// references those resources via {StaticResource ...}/{DynamicResource ...}. NetArchTest's rules
// above operate on compiled types and cannot see markup literals, so this test reads the project's
// own .axaml source directly (mirroring DesktopProjects' source-tree walk) instead.
public class TokenDisciplineRules
{
    // Avalonia's "Transparent" keyword states the absence of a fill, not a design decision, and a
    // literal 0 corner radius states "no rounding" — neither encodes a reusable design *value*, so
    // neither needs a token (DT-06 targets the values themselves, not every use of these properties).
    private static readonly HashSet<string> AllowedLiterals = new(StringComparer.OrdinalIgnoreCase)
    {
        "Transparent",
        "0",
    };

    private const string GuardedProperties = "CornerRadius|FontSize|FontFamily|Background|Foreground|BorderBrush|Fill|Stroke|Color";

    // Matches a plain XAML attribute, e.g. `Background="#FFFFFF"`; `[^{"]` on the value's first
    // character rejects both `{StaticResource ...}`/`{DynamicResource ...}` markup extensions and an
    // empty string, which is never a literal color/size/family/radius.
    private static readonly Regex AttributeLiteral = new(
        $@"\b(?<property>{GuardedProperties})=""(?<value>[^{{""][^""]*)""",
        RegexOptions.Compiled);

    // Matches the `<Setter Property="..." Value="...">` form Theme/Styles/ uses instead of a plain
    // attribute (Avalonia Style setters are not XAML attributes on the styled element itself).
    private static readonly Regex SetterLiteral = new(
        $@"<Setter\s+Property=""(?<property>{GuardedProperties})""\s+Value=""(?<value>[^{{""][^""]*)""",
        RegexOptions.Compiled);

    public static TheoryData<string> XamlFilesOutsideTokens => new(DiscoverXamlFiles());

    [Theory]
    [MemberData(nameof(XamlFilesOutsideTokens))]
    public void XamlOutsideTokensHasNoLiteralColorFontOrRadiusValues(string relativePath)
    {
        string content = File.ReadAllText(Path.Combine(UiProjectRoot(), relativePath));

        List<string> violations = [.. FindViolations(AttributeLiteral, content), .. FindViolations(SetterLiteral, content)];

        violations.Should().BeEmpty(
            "DT-06 forbids literal colors/font sizes/font families/corner radii outside Theme/Tokens/ ({0}): {1}",
            relativePath,
            string.Join(", ", violations));
    }

    private static IEnumerable<string> FindViolations(Regex pattern, string content)
    {
        foreach (Match match in pattern.Matches(content))
        {
            string value = match.Groups["value"].Value;
            if (!AllowedLiterals.Contains(value))
            {
                yield return $"{match.Groups["property"].Value}=\"{value}\"";
            }
        }
    }

    private static List<string> DiscoverXamlFiles()
    {
        string root = UiProjectRoot();
        string tokensRoot = Path.Combine(root, "Theme", "Tokens") + Path.DirectorySeparatorChar;

        return [.. Directory.EnumerateFiles(root, "*.axaml", SearchOption.AllDirectories)
            .Where(path => !path.StartsWith(tokensRoot, StringComparison.OrdinalIgnoreCase))
            .Where(path => !IsUnderBuildOutput(path))
            .Select(path => Path.GetRelativePath(root, path))
            .OrderBy(path => path, StringComparer.Ordinal)];
    }

    // obj/bin hold copies of every .axaml alongside generated designer artifacts; only the
    // hand-authored source under the project root is a compliance surface.
    private static bool IsUnderBuildOutput(string path) =>
        path.Split(Path.DirectorySeparatorChar).Any(segment => segment is "obj" or "bin");

    private static string UiProjectRoot() => Path.Combine(DesktopProjects.SourceRoot(), DesktopAssemblies.Ui);
}
