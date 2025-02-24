﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CsvToJson.UnitTests.Helpers
{
    public class TestLogger : ILogger
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private BlockingCollection<LogMessage> _logMessages = new BlockingCollection<LogMessage>();

        public TestLogger(string category, Func<string, LogLevel, bool> filter = null)
        {
            Category = category;
            _filter = filter;
        }

        public string Category { get; private set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter?.Invoke(Category, logLevel) ?? true;
        }

        public IList<LogMessage> GetLogMessages()
        {
            _logMessages.CompleteAdding();
            return _logMessages.ToList();
        }

        //  public void ClearLogMessages() => _logMessages.Clear();
        public void ClearLogMessages()
        {
            while (_logMessages.TryTake(out var _)) { }
        }

        public bool AllEntriesAreUnique()
        {
             var diffChecker = new HashSet<string>();
             return GetLogMessages().All(x => diffChecker.Add(x.FormattedMessage));
        }

        public void CompleteAdding() => _logMessages.CompleteAdding();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var logMessage = new LogMessage
            {
                Level = logLevel,
                EventId = eventId,
                State = state as IEnumerable<KeyValuePair<string, object>>,
                Exception = exception,
                FormattedMessage = formatter(state, exception),
                Category = Category,
                Timestamp = DateTime.UtcNow
            };

            _logMessages.Add(logMessage);
        }
    }

    public class LogMessage
    {
        public LogLevel Level { get; set; }

        public EventId EventId { get; set; }

        public IEnumerable<KeyValuePair<string, object>> State { get; set; }

        public Exception Exception { get; set; }

        public string FormattedMessage { get; set; }

        public string Category { get; set; }

        public DateTime Timestamp { get; set; }

        public override string ToString() => $"[{Timestamp.ToString("HH:mm:ss.fff")}] [{Category}] {FormattedMessage}";
    }
}