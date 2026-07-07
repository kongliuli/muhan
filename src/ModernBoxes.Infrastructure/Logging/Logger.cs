using NLog;
using System;

namespace ModernBoxes.Infrastructure
{
    public static class Logger
    {
        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        public static void Info(string message) => _logger.Info(message);
        public static void Warn(string message) => _logger.Warn(message);
        public static void Error(Exception ex, string context) => _logger.Error(ex, context);
        public static void Error(string message) => _logger.Error(message);
        public static void Debug(string message) => _logger.Debug(message);
        public static void Fatal(Exception ex, string context) => _logger.Fatal(ex, context);
    }
}
