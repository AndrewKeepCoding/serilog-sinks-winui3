using Microsoft.UI.Xaml;
using Serilog.Events;

namespace Serilog.Sinks.WinUi3.LogViewModels;

public record LogViewModel : ILogViewModel
{
    public uint Id { get; set; }
    public LogEvent? LogEvent { get; set; }
    public LogViewModelElement? Timestamp { get; set; }
    public LogViewModelElement? Level { get; set; }
    public LogViewModelElement? Message { get; set; }
    public LogViewModelElement? Exception { get; set; }
    public Visibility ExceptionVisibility { get; set; }
}
