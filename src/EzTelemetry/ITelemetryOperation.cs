using System;
using System.Collections.Generic;

namespace EzTelemetry
{
    public interface ITelemetryOperation : ITelemetryStage<ITelemetryOperation>
    {
        ITelemetryOperation AddEvent(string eventName);
        ITelemetryOperation AddEvent(string eventName, IEnumerable<KeyValuePair<string, string>> properties);

        ITelemetryOperation AddMetric(string name, double value);
        ITelemetryOperation AddMetric(string name, double value, IEnumerable<KeyValuePair<string, string>> properties);

        ITelemetryOperation AddException(Exception exception);
        ITelemetryOperation AddException(Exception exception, IEnumerable<KeyValuePair<string, string>> properties);

        ITelemetryOperation AddTrace(string message);
        ITelemetryOperation AddTrace(string message, IEnumerable<KeyValuePair<string, string>> properties);
    }
}
