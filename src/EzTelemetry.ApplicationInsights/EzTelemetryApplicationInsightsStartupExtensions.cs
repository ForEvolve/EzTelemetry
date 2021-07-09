using EzTelemetry;
using EzTelemetry.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EzTelemetryApplicationInsightsStartupExtensions
    {
        public static IServiceCollection AddEzTelemetry(this IServiceCollection services)
        {
            services.AddScoped<ITelemetrySession, TelemetrySession>();
            return services;
        }
    }
}
