using System;

namespace FileComparer
{
    public static class Logger
    {
        public static void LogInfo(string message) => Write(LogLevel.Info, message);

        public static void LogWarn(string message)
        {
            Write(LogLevel.Warn, message);
        }

        public static void LogError(string message)
        {
            Write(LogLevel.Error, message);
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
