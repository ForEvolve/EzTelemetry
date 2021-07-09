using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace EzTelemetry
{
    public class SessionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IBaseRequest
    {
        private readonly ITelemetrySession _telemetrySession;
        private readonly IEnumerable<IRequestNameFormatter> _formatters;
        public SessionBehavior(ITelemetrySession telemetrySession, IEnumerable<IRequestNameFormatter> formatters)
        {
            _telemetrySession = telemetrySession ?? throw new ArgumentNullException(nameof(telemetrySession));
            _formatters = formatters ?? throw new ArgumentNullException(nameof(formatters));
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var requestName = typeof(TRequest).FullName;

            _telemetrySession.AddProperty("Request:FullName", requestName);
            _telemetrySession.AddProperties(request, prefix: requestName);

            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            var operation = _telemetrySession.StartOperation();
            operation.AddMetric($"{requestName}:ElapsedTime", stopwatch.ElapsedMilliseconds);
            operation.AddEvent(requestName);

            return response;
        }
    }
    public class RequestNameFormatProcessor : IRequestNameFormatProcessor
    {
        private readonly IEnumerable<IRequestNameFormatter> _formatters;
        public RequestNameFormatProcessor(IEnumerable<IRequestNameFormatter> formatters)
        {
            _formatters = formatters ?? throw new ArgumentNullException(nameof(formatters));
        }

        public string Format<TRequest>(TRequest request) where TRequest : IBaseRequest
        {
            var requestName = typeof(TRequest).FullName;
            foreach (var formatter in _formatters)
            {
                requestName = formatter.Format(requestName);
            }
            return requestName;
        }
    }

    public interface IRequestNameFormatProcessor
    {
        string Format<TRequest>(TRequest request) where TRequest : IBaseRequest;
    }

    public interface IRequestNameFormatter
    {
        string Format(string name);
    }

    public class RemovePrefixFromRequestNameFormatter : IRequestNameFormatter
    {
        private readonly string _namespace;
        private readonly int _startIndex;
        public RemovePrefixFromRequestNameFormatter(string @namespace)
        {
            _namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            _startIndex = _namespace.Length + 1;
        }

        public string Format(string name)
        {
            if (name.Length > _startIndex && name.StartsWith(_namespace, StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring(_startIndex);
            }
            return name;
        }
    }
}
