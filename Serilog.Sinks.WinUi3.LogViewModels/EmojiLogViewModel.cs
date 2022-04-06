namespace Serilog.Sinks.WinUi3.LogViewModels;

public record EmojiLogViewModel : LogViewModel
{
    public string? Emoji { get; set; }
}
