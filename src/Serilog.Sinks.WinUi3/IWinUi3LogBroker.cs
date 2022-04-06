using Microsoft.UI.Dispatching;
using Serilog.Events;
using System;

namespace Serilog.Sinks.WinUi3;

public interface IWinUi3LogBroker
{
    Action<LogEvent> AddLogEvent { get; }
    DispatcherQueue DispatcherQueue { get; }
}
