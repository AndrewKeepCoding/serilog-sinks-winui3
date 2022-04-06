using Microsoft.UI.Xaml.Media;

namespace Serilog.Sinks.WinUi3.LogViewModels;

public record LogViewModelElement() : ILogViewModelElement
{
    public string? Text { get; set; }
    public SolidColorBrush? Foreground { get; set; }
}
