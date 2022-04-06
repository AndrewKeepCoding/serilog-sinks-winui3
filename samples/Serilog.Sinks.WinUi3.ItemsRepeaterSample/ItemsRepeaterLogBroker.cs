using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Serilog.Events;
using Serilog.Sinks.WinUi3.LogViewModels;
using System;
using System.Collections.ObjectModel;

namespace Serilog.Sinks.WinUi3.ItemsRepeaterSample;

public class ItemsRepeaterLogBroker : IWinUi3LogBroker
{
    private readonly ILogViewModelBuilder _logViewModelBuilder;

    public ItemsRepeaterLogBroker(
        ItemsRepeater itemsRepeater,
        ScrollViewer scrollViewer,
        ILogViewModelBuilder logViewModelBuilder)
    {
        itemsRepeater.SetBinding(ItemsRepeater.ItemsSourceProperty, new Binding() { Source = Logs });

        _logViewModelBuilder = logViewModelBuilder;

        DispatcherQueue = itemsRepeater.DispatcherQueue;
        AddLogEvent = logEvent => Logs.Add(_logViewModelBuilder.Build(logEvent));
        Logs.CollectionChanged += ((sender, e) =>
        {
            if (IsAutoScrollOn is true && sender is ObservableCollection<ILogViewModel> collection)
            {
                scrollViewer.ChangeView(
                    horizontalOffset: 0,
                    verticalOffset: scrollViewer.ScrollableHeight,
                    zoomFactor: 1,
                    disableAnimation: true);
            }
        });
    }

    public Action<LogEvent> AddLogEvent { get; }
    public DispatcherQueue DispatcherQueue { get; }
    public bool IsAutoScrollOn { get; set; }

    private ObservableCollection<ILogViewModel> Logs { get; init; } = new();
}
