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

        public bool IsEnabled(LogLevel logLevel)
        {
            return false; 
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // No-op for this fake.
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            private NullScope() { }
            public void Dispose() { /* No-op */ }
        }
    }
}
