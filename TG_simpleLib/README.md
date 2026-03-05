# TGSimpleLib 🤖

[![GitHub](https://img.shields.io/badge/GitHub-Repo-blue?logo=github)](https://github.com/wertkiz/TGSimpleLib)

[English](#english) | [Русский](#русский)

---

## English

**TGSimpleLib** is a lightweight C# wrapper for Telegram bots. It's designed for developers who need to send notifications, files, and get instant feedback without setting up complex bot architectures.

### What's new in v1.1.0:
*   **File Uploads**: Send logs, documents, or memory streams directly to admins.
*   **Error Reporting**: Beautifully formatted exception reports with stack traces.
*   **Full Localization**: All built-in buttons and messages are now in English for global compatibility.

### Usage:
```csharp
// Initialize: Token, Timeout (sec), and Admin IDs
var bot = new SimpleTG("YOUR_TOKEN", 30, 12345678);

// 1. Simple notification
await bot.NotifyAsync("🚀 System started!");

// 2. File upload from disk
await bot.SendFileAsync("logs/app.log", "Today's server log");

// 3. File upload from Memory (Stream) without disk access
byte[] logBytes = System.Text.Encoding.UTF8.GetBytes("This is an in-memory report");
using (var ms = new System.IO.MemoryStream(logBytes))
{
    await bot.SendFileAsync(ms, "report.txt", "Dynamic report");
}

// 4. Exception reporting (formatted report)
try {
    // some risky code
} catch (Exception ex) {
    await bot.ReportErrorAsync("Database sync failed", ex);
}

// 5. Interactive questions (Yes/No or Custom)
bool confirm = await bot.AskBoolAsync("Deploy to production?");
string mode = await bot.AskChoiceAsync("Select mode:", new[] { "Eco", "Turbo" });

// 6. Wait for text input (via ForceReply)
string comment = await bot.AskStringAsync("Enter your comment:");
```



## Русский

**TGSimpleLib** — это максимально простая библиотека-обертка для Telegram ботов на C#. Она создана для тех, кому нужно быстро добавить систему уведомлений, отправку файлов и опросов в свои приложения без написания сложной архитектуры.

### Что нового в v1.1.0:
*   **Отправка файлов**: Отправляйте логи или документы прямо с диска или из оперативной памяти (`Stream`).
*   **Отчеты об ошибках**: Красиво отформатированные отчеты об исключениях (`Exception`) со стек-трейсом.
*   **Локализация**: Все встроенные кнопки и системные сообщения переведены на английский для универсальности.

### Примеры использования:

```csharp
// Инициализация: Токен, Таймаут (сек) и ID админов
var bot = new SimpleTG("ВАШ_ТОКЕН", 30, 12345678);

// 1. Простое уведомление
await bot.NotifyAsync("🚀 Система запущена!");

// 2. Отправка файлов с диска
await bot.SendFileAsync("logs/app.log", "Лог сервера");

// 3. Отправка данных из памяти (Stream) без сохранения файла на диск
byte[] logBytes = System.Text.Encoding.UTF8.GetBytes("Это отчет из памяти");
using (var ms = new System.IO.MemoryStream(logBytes))
{
    await bot.SendFileAsync(ms, "report.txt", "Динамический отчет");
}

// 4. Отчет об ошибке (красивый формат с деталями)
try {
    // ваш код
} catch (Exception ex) {
    await bot.ReportErrorAsync("Ошибка в модуле БД", ex);
}

// 5. Вопросы (Да/Нет или свои варианты)
bool confirm = await bot.AskBoolAsync("Выполнить деплой?");
string mode = await bot.AskChoiceAsync("Выберите режим:", new[] { "Эконом", "Турбо" });

// 6. Ожидание ввода текста
string comment = await bot.AskStringAsync("Введите ваш комментарий:");
```