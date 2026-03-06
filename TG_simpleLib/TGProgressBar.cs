using System.Text;
using Telegram.Bot;

namespace TGSimpleLib;

public class TGProgressBar
{
    private readonly TelegramBotClient _client;
    private readonly List<(long ChatId, int MessageId)> _messages;
    private readonly string _title;

    internal TGProgressBar(TelegramBotClient client, List<(long ChatId, int MessageId)> messages, string title)
    {
        _client = client;
        _messages = messages;
        _title = title;
    }

    /// <summary>
    /// Updates the progress bar percentage (0-100).
    /// </summary>
    public async Task UpdateAsync(int percentage)
    {
        percentage = Math.Max(0, Math.Min(100, percentage));
        string text = GenerateBar(percentage);

        foreach (var (chatId, msgId) in _messages)
        {
            try
            {
                await _client.EditMessageText(chatId, msgId, text);
            }
            catch (Exception ex) when (ex.Message.Contains("not modified"))
            {
            }
        }
    }

    /// <summary>
    /// Completes the progress bar with a final message.
    /// </summary>
    public async Task CompleteAsync(string finalMessage = "Completed!")
    {
        string text = $"✅ *{_title}*\n{finalMessage}";

        foreach (var (chatId, msgId) in _messages)
        {
            try
            {
                await _client.EditMessageText(chatId, msgId, text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            catch { }
        }
    }

    private string GenerateBar(int percentage)
    {
        int totalBlocks = 10;
        int filledBlocks = percentage / 10;

        StringBuilder bar = new StringBuilder();
        bar.AppendLine($"⏳ *{_title}*");

        for (int i = 0; i < totalBlocks; i++)
        {
            if (i < filledBlocks) bar.Append("■");
            else bar.Append("□");
        }

        bar.Append($" {percentage}%");
        return bar.ToString();
    }
}