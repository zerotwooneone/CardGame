using Microsoft.Extensions.Logging;

namespace CardGame.Domain.Tests.TestDoubles
{
    public class FakeLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            // No-op for this fake.
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FakeLogger<object>();
        }

        public ILogger<T> CreateLogger<T>()
        {
            return new FakeLogger<T>();
        }

        public void Dispose()
        {
            // No-op for this fake.
        }
    }
}
