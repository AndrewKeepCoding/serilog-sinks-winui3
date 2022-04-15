using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Serilog.Events;
using Serilog.Formatting;
using System.Collections.Generic;
using System.IO;
using Windows.UI;

namespace Serilog.Sinks.WinUi3.LogViewModels;

public class LogViewModelBuilder : ILogViewModelBuilder
{
    protected Dictionary<Color, SolidColorBrush> _colorCache = new();
    protected SolidColorBrush? _defaultForeground;

    protected SolidColorBrush? _exceptionForeground;
    protected ITextFormatter? _exceptionTextFormatter;

    protected Dictionary<LogEventLevel, SolidColorBrush> _levelForegrounds = new();
    protected ITextFormatter? _levelTextFormatter;

    protected Dictionary<LogEventLevel, SolidColorBrush> _messageForegrounds = new();
    protected ITextFormatter? _messageTextFormatter;
    protected SolidColorBrush? _sourceContextForeground;
    protected ITextFormatter? _sourceContextTextFormatter;
    protected SolidColorBrush? _timestampForeground;
    protected ITextFormatter? _timestampTextFormatter;

    public LogViewModelBuilder(Color? defaultForeground)
    {
        if (defaultForeground is Color color)
            _defaultForeground = new SolidColorBrush(color);
    }

    protected uint NextId { get; set; } = 1;

    public virtual ILogViewModel Build(LogEvent logEvent)
    {
        return new LogViewModel()
        {
            Id = NextId++,
            Timestamp = BuildTimestampElement(logEvent),
            Level = BuildLevelElement(logEvent),
            SourceContext = BuildSourceContextElement(logEvent),
            Message = BuildMessageElement(logEvent),
            Exception = BuildExceptionElement(logEvent),
            ExceptionVisibility = ((logEvent.Exception is not null) ? Visibility.Visible : Visibility.Collapsed)
        };
    }

    public virtual LogViewModelBuilder SetExceptionForeground(Color color)
    {
        _exceptionForeground = GetSolidColorBrush(color);
        return this;
    }

    public virtual LogViewModelBuilder SetExceptionFormat(ITextFormatter textFormatter)
    {
        _exceptionTextFormatter = textFormatter;
        return this;
    }

    public virtual LogViewModelBuilder SetLevelForeground(LogEventLevel level, Color color)
    {
        _levelForegrounds[level] = GetSolidColorBrush(color);
        return this;
    }

    public virtual LogViewModelBuilder SetLevelsFormat(ITextFormatter textFormatter)
    {
        _levelTextFormatter = textFormatter;
        return this;
    }

    public virtual LogViewModelBuilder SetMessageForeground(LogEventLevel level, Color color)
    {
        _messageForegrounds[level] = GetSolidColorBrush(color);
        return this;
    }

    public virtual LogViewModelBuilder SetMessageFormat(ITextFormatter textFormatter)
    {
        _messageTextFormatter = textFormatter;
        return this;
    }

    public virtual LogViewModelBuilder SetSourceContextForeground(Color color)
    {
        _sourceContextForeground = GetSolidColorBrush(color);
        return this;
    }

    public virtual LogViewModelBuilder SetSourceContextFormat(ITextFormatter textFormatter)
    {
        _sourceContextTextFormatter = textFormatter;
        return this;
    }

    public virtual LogViewModelBuilder SetTimestampForeground(Color color)
    {
        _timestampForeground = GetSolidColorBrush(color);
        return this;
    }

    public virtual LogViewModelBuilder SetTimestampFormat(ITextFormatter textFormatter)
    {
        _timestampTextFormatter = textFormatter;
        return this;
    }

    protected static LogViewModelElement BuildLogViewModelElement(LogEvent logEvent, ITextFormatter? textFormatter, SolidColorBrush? foreground)
    {
        StringWriter stringWriter = new();
        textFormatter?.Format(logEvent, stringWriter);

        // The ItemsRepeater control has a known bug with different item heights.
        // To avoid this bug crashing the app, we need to remove new lines.
        // https://github.com/microsoft/microsoft-ui-xaml/issues/1829
        string text = stringWriter.ToString().Replace("\r", "").Replace("\n", "");

        return new LogViewModelElement() { Text = text, Foreground = foreground };
    }

    protected virtual LogViewModelElement BuildExceptionElement(LogEvent logEvent)
    {
        _exceptionForeground ??= _defaultForeground;
        return BuildLogViewModelElement(logEvent, _exceptionTextFormatter, _exceptionForeground);
    }

    protected virtual LogViewModelElement BuildLevelElement(LogEvent logEvent)
    {
        _levelForegrounds.TryGetValue(logEvent.Level, out var levelForeground);
        levelForeground ??= _defaultForeground;
        return BuildLogViewModelElement(logEvent, _levelTextFormatter, levelForeground);
    }

    protected virtual LogViewModelElement BuildMessageElement(LogEvent logEvent)
    {
        if (_messageForegrounds.TryGetValue(logEvent.Level, out SolidColorBrush? foregound) is false)
        {
            foregound = _defaultForeground;
        }

        return BuildLogViewModelElement(logEvent, _messageTextFormatter, foregound);
    }

    protected virtual LogViewModelElement BuildSourceContextElement(LogEvent logEvent)
    {
        _sourceContextForeground ??= _defaultForeground;
        return BuildLogViewModelElement(logEvent, _sourceContextTextFormatter, _sourceContextForeground);
    }

    protected virtual LogViewModelElement BuildTimestampElement(LogEvent logEvent)
    {
        _timestampForeground ??= _defaultForeground;
        return BuildLogViewModelElement(logEvent, _timestampTextFormatter, _timestampForeground);
    }

    protected virtual SolidColorBrush GetSolidColorBrush(Color color)
    {
        if (_colorCache.TryGetValue(color, out SolidColorBrush? brush) is false)
        {
            brush = new SolidColorBrush(color);
            _colorCache[color] = brush;
        }
        return brush;
    }
}
