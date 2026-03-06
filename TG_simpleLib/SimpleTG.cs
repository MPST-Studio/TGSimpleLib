using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Concurrent;
using System.IO;

namespace TGSimpleLib;

internal class PendingQuestion
{
    public TaskCompletionSource<string?> Tcs { get; } = new();
    public List<(long ChatId, int MessageId)> SentMessages { get; } = new();
}

public class SimpleTG
{
    private readonly TelegramBotClient _client;
    private readonly HashSet<long> _adminIds;
    private readonly int _defaultTimeout;

    private readonly ConcurrentDictionary<int, PendingQuestion> _pendingRequests = new();
    private readonly ConcurrentDictionary<string, Func<Task>> _commands = new();

    /// <summary>
    /// Occurs when a new log entry is generated.
    /// </summary>
    public event EventHandler<string>? OnLog;

    /// <summary>
    /// Initializes a new instance of the SimpleTG bot wrapper.
    /// </summary>
    /// <param name="token">Your Telegram Bot Token.</param>
    /// <param name="defaultTimeoutSeconds">Default wait time for responses (seconds).</param>
    /// <param name="targetChatIds">Array of administrator Chat IDs.</param>
    public SimpleTG(string token, int defaultTimeoutSeconds = 10, params long[] targetChatIds)
    {
        _client = new TelegramBotClient(token);
        _adminIds = new HashSet<long>(targetChatIds);
        _defaultTimeout = defaultTimeoutSeconds;

        _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
    }

    #region Functional Part

    /// <summary>
    /// Sends a simple text notification to all administrators.
    /// </summary>
    /// <param name="text">Notification text.</param>
    public async Task NotifyAsync(string text)
    {
        Log($"Notification sent to all admins: {text}");
        foreach (var id in _adminIds)
        {
            try { await _client.SendMessage(id, text); } catch { }
        }
    }

    /// <summary>
    /// Asks a Yes/No question using buttons.
    /// </summary>
    /// <param name="text">The question text.</param>
    /// <param name="timeout">Optional timeout in seconds.</param>
    /// <returns>True if "Yes" was pressed, False if "No" or timeout occurred.</returns>
    public async Task<bool> AskBoolAsync(string text, int? timeout = null)
    {
        // Мы не вызываем здесь Log, так как он сработает внутри AskChoiceAsync
        var res = await AskChoiceAsync(text, new[] { "Yes", "No" }, timeout);
        return res == "Yes";
    }

    /// <summary>
    /// Sends a message with custom choice buttons.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <param name="choices">Array of button labels.</param>
    /// <param name="timeout">Optional timeout in seconds.</param>
    /// <returns>The label of the pressed button or null if timeout occurred.</returns>
    public async Task<string?> AskChoiceAsync(string text, string[] choices, int? timeout = null)
    {
        var buttons = choices
            .Select(c => InlineKeyboardButton.WithCallbackData(c, c))
            .Select((item, index) => new { item, index })
            .GroupBy(x => x.index / 2)
            .Select(g => g.Select(x => x.item).ToArray())
            .ToArray();
        var keyboard = new InlineKeyboardMarkup(buttons);
        var question = new PendingQuestion();

        foreach (var chatId in _adminIds)
        {
            try
            {
                var msg = await _client.SendMessage(chatId, text, replyMarkup: keyboard);
                question.SentMessages.Add((chatId, msg.Id));
                _pendingRequests[msg.Id] = question;
            }
            catch { }
        }
        Log($"Question sent: {text} | Options: {string.Join(", ", choices)}");
        return await WaitForResponseAsync(question, timeout ?? _defaultTimeout);
    }

    /// <summary>
    /// Sends a file (document) to all administrators using a file path.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="caption">Optional caption for the file.</param>
    public async Task SendFileAsync(string filePath, string caption = null)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found at the specified path.", filePath);

        using (var stream = File.OpenRead(filePath))
        {
            stream.Position = 0;
            string fileName = Path.GetFileName(filePath);
            await SendFileAsync(stream, fileName, caption);
        }
    }

    /// <summary>
    /// Sends a file (document) to all administrators from a Stream.
    /// </summary>
    /// <param name="stream">Data stream.</param>
    /// <param name="fileName">The name the user will see.</param>
    /// <param name="caption">Optional caption for the file.</param>
    public async Task SendFileAsync(Stream stream, string fileName, string caption = null)
    {
        var inputFile = InputFile.FromStream(stream, fileName);

        foreach (var id in _adminIds)
        {
            try
            {
                await _client.SendDocument(
                    chatId: id,
                    document: inputFile,
                    caption: caption
                );
            }
            catch { }
        }
    }

    /// <summary>
    /// Sends a message and waits for a text response from any administrator.
    /// </summary>
    /// <param name="text">The prompt text.</param>
    /// <param name="timeout">Optional timeout in seconds.</param>
    /// <returns>The user's response or null if timeout occurred.</returns>
    public async Task<string?> AskStringAsync(string text, int? timeout = null)
    {
        var question = new PendingQuestion();

        foreach (var chatId in _adminIds)
        {
            try
            {
                var msg = await _client.SendMessage(chatId, text, replyMarkup: new ForceReplyMarkup());
                question.SentMessages.Add((chatId, msg.Id));
                _pendingRequests[msg.Id] = question;
            }
            catch { }
        }
        Log($"Text request sent: {text}");
        return await WaitForResponseAsync(question, timeout ?? _defaultTimeout);
    }

    /// <summary>
    /// Formats and sends an error report (Exception) to all admins.
    /// </summary>
    /// <param name="context">Brief description of where the error occurred.</param>
    /// <param name="ex">The exception object.</param>
    public async Task ReportErrorAsync(string context, Exception ex)
    {
        string errorText = $"⚠️ *Error Report:* {context}\n\n" +
                           $"*Type:* `{ex.GetType().Name}`\n" +
                           $"*Message:* _{ex.Message}_\n\n" +
                           $"*Stack Trace:*\n```{ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace.Length, 1000))}```";

        foreach (var id in _adminIds)
        {
            try
            {
                await _client.SendMessage(
                    chatId: id,
                    text: errorText,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
                );
            }
            catch { }
        }
    }

    /// <summary>
    /// Creates a progress bar message for all administrators.
    /// </summary>
    /// <param name="title">The title of the operation.</param>
    /// <returns>A TGProgressBar object to control the updates.</returns>
    public async Task<TGProgressBar> CreateProgressBarAsync(string title)
    {
        var sentMessages = new List<(long ChatId, int MessageId)>();
        string initialText = $"⏳ *{title}*\n□□□□□□□□□□ 0%";

        foreach (var id in _adminIds)
        {
            try
            {
                var msg = await _client.SendMessage(id, initialText, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                sentMessages.Add((id, msg.Id));
            }
            catch { }
        }

        return new TGProgressBar(_client, sentMessages, title);
    }

    /// <summary>
    /// Registers a command (e.g., /start, /stop) and associates it with an action.
    /// </summary>
    /// <param name="command">Command text (e.g., "/stop").</param>
    /// <param name="action">The async function to execute.</param>
    public void OnCommand(string command, Func<Task> action)
    {

        _commands[command.ToLower()] = action;
    }

    #endregion

    #region Internal Logic

    private void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logEntry = $"[{timestamp}] | {message}";
        OnLog?.Invoke(this, logEntry);
    }

    private async Task<string?> WaitForResponseAsync(PendingQuestion question, int timeoutSeconds)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        cts.Token.Register(() => question.Tcs.TrySetResult(null));

        try
        {
            var result = await question.Tcs.Task;
            foreach (var (chatId, msgId) in question.SentMessages)
            {
                try
                {
                    await _client.EditMessageReplyMarkup(chatId, msgId, replyMarkup: null);
                }
                catch { }
            }

            return result;
        }
        finally
        {
            foreach (var m in question.SentMessages)
            {
                _pendingRequests.TryRemove(m.MessageId, out _);
            }
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        long? currentChatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
        if (currentChatId == null || !_adminIds.Contains(currentChatId.Value)) return;

        int? msgId = update.CallbackQuery?.Message?.Id ?? update.Message?.ReplyToMessage?.Id;

        if (msgId.HasValue && _pendingRequests.TryGetValue(msgId.Value, out var question))
        {
            string? response = update.CallbackQuery?.Data ?? update.Message?.Text;
            string userName = update.CallbackQuery?.From.FirstName ?? update.Message?.From?.FirstName ?? "Admin";

            if (question.Tcs.TrySetResult(response))
            {
                Log($"Response received from {userName} (ID: {currentChatId}): {response}");
                if (update.CallbackQuery != null)
                {
                    await bot.AnswerCallbackQuery(update.CallbackQuery.Id, $"Accepted from {userName}");
                }
            }
            return;
        }

        if (update.Message?.Text != null && update.Message.ReplyToMessage == null)
        {
            string incomingText = update.Message.Text.ToLower().Trim();

            if (_commands.TryGetValue(incomingText, out var action))
            {
                try
                {
                    Log($"Command executed by admin (ID: {currentChatId}): {incomingText}");
                    await action.Invoke();
                }
                catch (Exception ex)
                {
                    await ReportErrorAsync("Command Execution Error", ex);
                }
            }
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient b, Exception e, CancellationToken c) => Task.CompletedTask;

    #endregion
}