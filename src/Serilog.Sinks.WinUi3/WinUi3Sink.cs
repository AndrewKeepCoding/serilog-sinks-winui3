using Microsoft.UI.Dispatching;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;

namespace Serilog.Sinks.WinUi3;

internal class WinUi3Sink : ILogEventSink, IDisposable
{
    private readonly Action<LogEvent> _addLogEvent;
    private readonly Channel<LogEvent> _channel = Channel.CreateUnbounded<LogEvent>();
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly DispatcherQueuePriority _dispatcherQueuePriority;
    private readonly TimeSpan _loggingInterval = TimeSpan.FromMilliseconds(500);
    private CancellationTokenSource? _cancellationTokenSource = new();

    public WinUi3Sink(
        Action<LogEvent> addLogEvent,
        DispatcherQueue dispatcherQueue,
        DispatcherQueuePriority dispatcherQueuePriority)
    {
        _dispatcherQueue = dispatcherQueue;
        _dispatcherQueuePriority = dispatcherQueuePriority;
        _addLogEvent = addLogEvent;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    public void Emit(LogEvent logEvent)
    {
        _ = _channel.Writer.TryWrite(logEvent);
    }

    public bool StartUpdate()
    {
        _cancellationTokenSource ??= new();
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        return _dispatcherQueue.TryEnqueue(
            _dispatcherQueuePriority,
            async () =>
            {
                List<LogEvent> cache = new();
                Stopwatch stopwatch = new();
                using PeriodicTimer timer = new(_loggingInterval);
                try
                {
                    while (await timer.WaitForNextTickAsync(cancellationToken) is true)
                    {
                        stopwatch.Restart();
                        while (_channel.Reader.TryRead(out LogEvent? logEvent) is true)
                        {
                            if (logEvent is not null)
                                cache.Add(logEvent);

                            if (stopwatch.ElapsedMilliseconds > _loggingInterval.Milliseconds)
                                break;
                        }

                        foreach (LogEvent logEvent in cache)
                        {
                            _addLogEvent(logEvent);
                        }

                        cache.Clear();
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine(ex.ToString());
                    throw;
                }
            });
    }

    public void StopUpdate()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new();
    }
}
