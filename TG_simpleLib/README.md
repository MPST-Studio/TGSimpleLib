# TGSimpleLib 🤖

[![NuGet](https://img.shields.io/nuget/v/TGSimpleLib.svg)](https://www.nuget.org/packages/TGSimpleLib/)
[![Downloads](https://img.shields.io/nuget/dt/TGSimpleLib.svg)](https://www.nuget.org/packages/TGSimpleLib/)
[![GitHub](https://img.shields.io/badge/GitHub-Organization-blue?logo=github)](https://github.com/MPST-Studio/TGSimpleLib)

[English](#english) | [Русский](#русский)

---

## English

**TGSimpleLib** by **MPST-Studio** is a high-level C# wrapper for Telegram bots. It's designed for developers who need professional monitoring tools (notifications, files, commands, progress bars, and logs) with zero boilerplate.

### What's new in v1.2.0:
*   **Command Handling**: Register commands like `/start` or `/status` in one line.
*   **Progress Bars**: Visual feedback for long-running tasks without spamming the chat.
*   **Logging System**: Built-in `OnLog` event to track every action and response.
*   **Branding**: Officially moved to **MPST-Studio** organization with a new icon.

### Full Usage Example:

```csharp
using TGSimpleLib;

// 1. Initialization (Token, Timeout, Admin IDs)
var bot = new SimpleTG("YOUR_TOKEN", 30, 12345678, 87654321);

// 2. Logging (Capture every action into a file or console)
bot.OnLog += (sender, logMessage) => File.AppendAllText("bot.log", logMessage + Environment.NewLine);

// 3. Command Handling (Bot is listening to admins)
bot.OnCommand("/status", async () => {
    await bot.NotifyAsync("✅ All systems are running normally.");
});

// 4. Simple Notification
await bot.NotifyAsync("🚀 Bot is now online!");

// 5. Send Files (Logs or Documents)
await bot.SendFileAsync("app_log.txt", "Current server log");

// 6. Progress Bar (Updates one message)
var pb = await bot.CreateProgressBarAsync("Long Task");
for (int i = 0; i <= 100; i += 25) {
    await Task.Delay(1000);
    await pb.UpdateAsync(i); // Updates to [■■■□□] 50%
}
await pb.CompleteAsync("Task finished!");

// 7. Interactive Questions (Yes/No)
bool deploy = await bot.AskBoolAsync("Run deployment?");

// 8. Custom Choice Buttons
string mode = await bot.AskChoiceAsync("Select mode:", new[] { "Eco", "Normal", "Turbo" });

// 9. Wait for Text Input
string comment = await bot.AskStringAsync("Please enter your feedback:");

// 10. Error Reporting (Formats exception with stack trace)
try {
    int x = 0; int y = 5 / x;
} catch (Exception ex) {
    await bot.ReportErrorAsync("Main Calculation Loop", ex);
}
```


---

## Русский

**TGSimpleLib** от **MPST-Studio** — это высокоуровневая библиотека-обертка для Telegram ботов на C#. Создана для разработчиков, которым нужны профессиональные инструменты мониторинга (уведомления, файлы, команды, прогресс-бары и логи) без лишнего кода.

### Что нового в v1.2.0:
*   **Обработка команд**: Регистрация команд вроде `/start` или `/stop` одной строкой.
*   **Прогресс-бары**: Визуальное отображение хода выполнения задач без спама в чате.
*   **Система логирования**: Событие `OnLog` для фиксации каждого действия бота и ответов админов.
*   **Брендинг**: Библиотека официально перенесена в организацию **MPST-Studio** и получила иконку.

### Полный пример всех функций:

```csharp
using TGSimpleLib;

// 1. Инициализация (Токен, Таймаут, ID админов через запятую)
var bot = new SimpleTG("ВАШ_ТОКЕН", 30, 12345678, 87654321);

// 2. Логирование (сохранение всех действий бота и ответов в файл)
bot.OnLog += (s, log) => File.AppendAllText("actions.log", log + Environment.NewLine);

// 3. Обработка команд (Бот реагирует на сообщения админов)
bot.OnCommand("/restart", async () => {
    await bot.NotifyAsync("🔄 Перезагрузка системы инициирована...");
});

// 4. Простая отправка сообщения
await bot.NotifyAsync("🚀 Бот запущен и готов к работе!");

// 5. Отправка файлов (логи или документы)
await bot.SendFileAsync("server.log", "Лог-файл сервера");

// 6. Прогресс-бар (динамическое обновление одного сообщения)
var pb = await bot.CreateProgressBarAsync("Резервное копирование");
for (int i = 0; i <= 100; i += 25) {
    await Task.Delay(1000);
    await pb.UpdateAsync(i); // Сообщение изменится на [■■■■□□□□□□] 40%
}
await pb.CompleteAsync("Копирование завершено!");

// 7. Вопрос Да/Нет (возвращает bool)
bool result = await bot.AskBoolAsync("Выполнить обновление?");

// 8. Выбор из ваших вариантов (возвращает строку)
string selection = await bot.AskChoiceAsync("Выберите режим:", new[] { "Тихий", "Обычный", "Турбо" });

// 9. Ожидание текстового ответа (возвращает string)
string feedback = await bot.AskStringAsync("Введите комментарий к отчету:");

// 10. Отчет об ошибке (красивый формат со стек-трейсом)
try {
    throw new Exception("Пример ошибки базы данных");
} catch (Exception ex) {
    await bot.ReportErrorAsync("Модуль базы данных", ex);
}