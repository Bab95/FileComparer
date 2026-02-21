using System;

namespace FileComparer
{
    public static class Logger
    {
        public static void LogInfo(string message) => Write(LogLevel.Info, message);

        public static void LogWarn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write(LogLevel.Warn, message);
            Console.ResetColor();
        }

        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(LogLevel.Error, message);
            Console.ResetColor();
        }

        private static void Write(LogLevel level, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[{timestamp}] [{level}] {message}");
        }

        private enum LogLevel
        {
            Info,
            Warn,
            Error
        }
    }
}
