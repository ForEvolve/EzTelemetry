using EzTelemetry;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MediatRStartupExtensions
    {
        public static IServiceCollection AddEzTelemetrySessionBehavior(this IServiceCollection services)
        {
            return services
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>))
                .AddSingleton<IRequestNameFormatProcessor, RequestNameFormatProcessor>()
            ;
        }
    }
}