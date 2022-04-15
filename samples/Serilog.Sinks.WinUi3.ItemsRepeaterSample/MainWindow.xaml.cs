using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.WinUi3.LogViewModels;
using Serilog.Templates;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Sinks.WinUi3.ItemsRepeaterSample;

public sealed partial class MainWindow : Window
{
    private CancellationTokenSource? _cancellationTokenSource;
    private LoggingLevelSwitch _levelSwitch;
    private ItemsRepeaterLogBroker _logBroker;

    public MainWindow()
    {
        this.InitializeComponent();

        this.Title = "Serilog WinUI 3 Sink Demo";

        _cancellationTokenSource = new CancellationTokenSource();

        _levelSwitch = new LoggingLevelSwitch();
        _levelSwitch.MinimumLevel = LogEventLevel.Verbose;

        string[] levels = Enum.GetNames(typeof(LogEventLevel));

        foreach (string level in levels)
        {
            LevelSwitcher.Items.Add(level.ToString());
        }

        LevelSwitcher.SelectedIndex = 0;
        LevelSwitcher.SelectionChanged += ((sender, e) =>
        {
            if (sender is ComboBox comboBox &&
                Enum.TryParse<LogEventLevel>(comboBox.SelectedItem.ToString(), out LogEventLevel level) is true)
                _levelSwitch.MinimumLevel = level;
        });

        App.Current.Resources.TryGetValue("DefaultTextForegroundThemeBrush", out object defaultTextForegroundBrush);

        _logBroker = new ItemsRepeaterLogBroker(
            LogViewer,
            LogScrollViewer,
            new EmojiLogViewModelBuilder((defaultTextForegroundBrush as SolidColorBrush)?.Color)

                .SetTimestampFormat(new ExpressionTemplate("[{@t:yyyy-MM-dd HH:mm:ss.fff}]"))

                .SetLevelsFormat(new ExpressionTemplate("{@l:u3}"))
                .SetLevelForeground(LogEventLevel.Verbose, Colors.Gray)
                .SetLevelForeground(LogEventLevel.Debug, Colors.Gray)
                .SetLevelForeground(LogEventLevel.Warning, Colors.Yellow)
                .SetLevelForeground(LogEventLevel.Error, Colors.Red)
                .SetLevelForeground(LogEventLevel.Fatal, Colors.HotPink)

                .SetSourceContextFormat(new ExpressionTemplate("{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}"))

                .SetMessageFormat(new ExpressionTemplate("{@m}"))
                .SetMessageForeground(LogEventLevel.Verbose, Colors.Gray)
                .SetMessageForeground(LogEventLevel.Debug, Colors.Gray)
                .SetMessageForeground(LogEventLevel.Warning, Colors.Yellow)
                .SetMessageForeground(LogEventLevel.Error, Colors.Red)
                .SetMessageForeground(LogEventLevel.Fatal, Colors.HotPink)

                .SetExceptionFormat(new ExpressionTemplate("{@x}"))
                .SetExceptionForeground(Colors.HotPink)
                );

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(_levelSwitch)
            .WriteTo.WinUi3Control(_logBroker)
            .CreateLogger();

        Logger = Log.Logger.ForContext<MainWindow>();

        _logBroker.IsAutoScrollOn = true;
    }

    private ILogger Logger { get; set; }

    private void AutoScrollToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch && _logBroker is not null)
            _logBroker.IsAutoScrollOn = toggleSwitch.IsOn;
    }

    private async void LoggingToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            if (toggleSwitch.IsOn is true)
                await StartLogging();
            else
                StopLogging();
        }
    }

    private async Task StartLogging()
    {
        try
        {
            _cancellationTokenSource ??= new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            int index = 0;
            List<Task> logSourceTasks = new();

            for (int i = 0; i < 10; i++)
            {
                int sourceId = i;
                logSourceTasks.Add(Task.Run(async () =>
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        Random random = new Random();
                        int value = random.Next(20);
                        switch (value)
                        {
                            case < 7:
                                Logger.Verbose("Source:{SourceId:00} Value:{Value:00} Index:{Index:00000}", sourceId, value, index);
                                break;
                            case < 12:
                                Logger.Debug("Source:{SourceId:00} Value:{Value:00} Index:{Index:00000}", sourceId, value, index);
                                break;
                            case < 16:
                                Logger.Information("Source:{SourceId:00} Value:{Value:00} Index:{Index:00000}", sourceId, value, index);
                                break;
                            case < 18:
                                Logger.Warning("Source:{SourceId:00} Value:{Value:00} Index:{Index:00000}", sourceId, value, index);
                                break;
                            case < 19:
                                Logger.Error("Source:{SourceId:00} Value:{Value:00} Index:{Index:00000}", sourceId, value, index);
                                break;
                            default:
                                Logger.Fatal(new Exception("FATAL EXCEPTION!"), "Something wrong happened! Source:{SourceId:00} Value:{Value:0} Index:{Index:00000}", sourceId, value, index);
                                break;
                        }
                        index++;

                        await Task.Delay(random.Next(1000));
                    }
                }));
            }
            await Task.WhenAll(logSourceTasks);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
        }
    }

    private void StopLogging()
    {
        Log.Logger.Information("Cancel requested.");
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new();
    }

    private void UpdateToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch && _logBroker is not null)
            LogViewer.Visibility = toggleSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed;
    }
    private void Window_Closed(object sender, WindowEventArgs args)
    {
        StopLogging();
        Log.CloseAndFlush();
    }
}
