using Serilog.Events;

namespace Serilog.Sinks.WinUi3.LogViewModels;

public interface ILogViewModel
{
    public uint Id { get; set; }
    public LogEvent? LogEvent { get; set; }
}
