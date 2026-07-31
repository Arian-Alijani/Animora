using Avalonia.Controls;

namespace Animora.Desktop.App.Gallery;

// TODO(P1-01): throwaway phase-01 host check window (TODO.md item 35) — delete alongside
// TokenGalleryWindow.axaml and App.axaml/App.axaml.cs's wiring to it once item 37 confirms
// Animora.Desktop.UI's tokens/controls render correctly on Windows.
public partial class TokenGalleryWindow : Window
{
    public TokenGalleryWindow()
    {
        InitializeComponent();

        // Set in code-behind rather than bound through a ViewModel: this window previews
        // Theme/Styles/DataGrid.axaml's row/header tokens (DT-08), it is not a real screen.
        SampleDataGrid.ItemsSource = new[]
        {
            new { Name = "رها احمدی", Status = "تأیید شده", Amount = "۸۶۴,۵۰۰,۰۰۰ ریال" },
            new { Name = "کیان محمدی", Status = "در انتظار", Amount = "۱۲۰,۰۰۰,۰۰۰ ریال" },
            new { Name = "سارا کریمی", Status = "لغو شده", Amount = "۰ ریال" },
        };
    }
}
