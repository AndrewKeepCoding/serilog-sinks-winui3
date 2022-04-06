using Microsoft.UI.Dispatching;
using Serilog.Configuration;

namespace Serilog.Sinks.WinUi3;

public static class WinUi3SinkLoggerConfigurationExtensions
{
    public static LoggerConfiguration WinUi3Control(
        this LoggerSinkConfiguration sinkConfiguration,
        IWinUi3LogBroker logBroker,
        DispatcherQueuePriority dispatcherQueuePriority = DispatcherQueuePriority.Low)
    {
        WinUi3Sink sink = new(
            logBroker.AddLogEvent,
            logBroker.DispatcherQueue,
            dispatcherQueuePriority);
        sink.StartUpdate();

        return sinkConfiguration.Sink(sink);
    }
}
