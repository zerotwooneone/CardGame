using Microsoft.Extensions.Logging;
using System;

namespace CardGame.Domain.Tests.TestDoubles
{
    public class FakeLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true; // Ensure all levels are "enabled" for capturing

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Output to console so it's visible in test runner output
            Console.WriteLine($"TEST_LOG [{logLevel}] [{typeof(T).Name}]: {formatter(state, exception)}");
            if (exception != null)
            {
                Console.WriteLine($"TEST_LOG_EXCEPTION [{logLevel}] [{typeof(T).Name}]: {exception}");
            }
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            private NullScope() { }
            public void Dispose() { /* No-op */ }
        }
    }
}
