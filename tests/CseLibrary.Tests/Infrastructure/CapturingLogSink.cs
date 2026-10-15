using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace CseLibrary.Tests.Infrastructure;

/// <summary>Keeps the app's Serilog events (picked up by ReadFrom.Services in Program.cs) for B1.</summary>
public sealed class CapturingLogSink : ILogEventSink
{
    public ConcurrentQueue<LogEvent> Events { get; } = new();

    public void Emit(LogEvent logEvent) => Events.Enqueue(logEvent);
}
