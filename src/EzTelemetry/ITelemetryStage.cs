using System.Collections.Generic;

namespace EzTelemetry
{
    public interface ITelemetryStage<TTelemetryStage>
    {
        TTelemetryStage AddProperty(string key, string value);
        TTelemetryStage AddProperties(IEnumerable<KeyValuePair<string, string>> properties);
    }
}
