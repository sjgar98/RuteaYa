using Microsoft.Extensions.Logging;

namespace RuteaYa.Tests
{
    // Guarda el nivel de los mensajes de advertencia o peores, solo para poder verificarlos.
    internal sealed class LoggerEspia<T> : ILogger<T>
    {
        public List<LogLevel> Niveles { get; } = new List<LogLevel>();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel >= LogLevel.Warning)
            {
                Niveles.Add(logLevel);
            }
        }
    }
}