using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Services
{
    public class LoggerProvider <T> : ILoggerProvider where T : ILogger
    {
        private readonly ConcurrentDictionary<string, T> _loggers = new();

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, (T)Activator.CreateInstance(typeof(T), categoryName));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
