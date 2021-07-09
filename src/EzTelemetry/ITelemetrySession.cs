using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EzTelemetry
{
    public interface ITelemetrySession : ITelemetryStage<ITelemetrySession>, IDisposable
    {
        ITelemetryOperation StartOperation();
    }
}
