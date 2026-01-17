namespace Minicraft.Engine.Diagnostics;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public static class Logger
{
    private static string? _logFilePath;
    private static readonly Lock Lock = new();

    public static void Initialize()
    {
        var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);

        var fileName = $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        _logFilePath = Path.Combine(logDir, fileName);

        Log(LogLevel.Info, "Logger Initialized.");
    }

    public static void Log(LogLevel level, string message)
    {
        lock (Lock)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var formattedMessage = $"[{timestamp}] [{level}] {message}";

            WriteToConsole(level, formattedMessage);
            WriteToFile(formattedMessage);
        }
    }

    public static void Debug(string message) => Log(LogLevel.Debug, message);
    public static void Info(string message) => Log(LogLevel.Info, message);
    public static void Warn(string message) => Log(LogLevel.Warning, message);
    public static void Error(string message) => Log(LogLevel.Error, message);
    public static void Error(string message, Exception ex) => 
        Log(LogLevel.Error, $"{message}\nException: {ex.Message}\nStack Trace: {ex.StackTrace}");

    private static void WriteToConsole(LogLevel level, string message)
    {
        var originalColor = Console.ForegroundColor;

        Console.ForegroundColor = level switch
        {
            LogLevel.Debug => ConsoleColor.DarkGray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error or LogLevel.Fatal => ConsoleColor.Red,
            _ => Console.ForegroundColor
        };

        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }

    private static void WriteToFile(string message)
    {
        if (string.IsNullOrEmpty(_logFilePath)) return;

        try
        {
            File.AppendAllText(_logFilePath, message + Environment.NewLine);
        }
        catch
        {
            Console.WriteLine("Failed to write to log file.");
        }
    }
}