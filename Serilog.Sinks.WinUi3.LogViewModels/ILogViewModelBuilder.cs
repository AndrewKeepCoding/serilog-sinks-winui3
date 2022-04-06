using Serilog.Events;

namespace Serilog.Sinks.WinUi3.LogViewModels;

public interface ILogViewModelBuilder
{
    ILogViewModel Build(LogEvent logEvent);
}
