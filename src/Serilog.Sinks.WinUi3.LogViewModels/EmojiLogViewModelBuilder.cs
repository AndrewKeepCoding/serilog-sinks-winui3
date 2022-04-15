using Microsoft.UI.Xaml;
using Serilog.Events;
using System.Collections.Generic;
using Windows.UI;

namespace Serilog.Sinks.WinUi3.LogViewModels;

public class EmojiLogViewModelBuilder : LogViewModelBuilder
{
    protected Dictionary<LogEventLevel, string> _levelEmojis = new();

    public EmojiLogViewModelBuilder(Color? defaultForeground) : base(defaultForeground)
    {
        _levelEmojis[LogEventLevel.Verbose] = "💬";
        _levelEmojis[LogEventLevel.Debug] = "👩🏾‍💻";
        _levelEmojis[LogEventLevel.Information] = "ℹ";
        _levelEmojis[LogEventLevel.Warning] = "⚠";
        _levelEmojis[LogEventLevel.Error] = "❗";
        _levelEmojis[LogEventLevel.Fatal] = "🚨";
    }

    public override ILogViewModel Build(LogEvent logEvent)
    {
        return new EmojiLogViewModel()
        {
            Emoji = GetEmoji(logEvent.Level),

            Id = NextId++,
            Timestamp = BuildTimestampElement(logEvent),
            Level = BuildLevelElement(logEvent),
            SourceContext = BuildSourceContextElement(logEvent),
            Message = BuildMessageElement(logEvent),
            Exception = BuildExceptionElement(logEvent),
            ExceptionVisibility = ((logEvent.Exception is not null) ? Visibility.Visible : Visibility.Collapsed)
        };
    }

    public virtual EmojiLogViewModelBuilder SetLevelEmoji(LogEventLevel level, string emoji)
    {
        _levelEmojis[level] = emoji;
        return this;
    }

    protected virtual string GetEmoji(LogEventLevel level)
    {
        if (_levelEmojis.TryGetValue(level, out string? emoji) is false)
        {
            emoji = string.Empty;
            _levelEmojis[level] = emoji;
        }
        return emoji;
    }
}
