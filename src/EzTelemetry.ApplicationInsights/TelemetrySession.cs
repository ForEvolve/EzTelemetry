using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EzTelemetry.ApplicationInsights
{
    public class TelemetrySession : ITelemetrySession
    {
        private readonly Dictionary<string, string> _sessionProperties = new();
        private readonly HashSet<TelemetryOperation> _operations = new();

        protected readonly TelemetryClient _telemetry;
        public TelemetrySession(TelemetryClient telemetry)
        {
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        public ITelemetrySession AddProperty(string key, string value)
        {
            _sessionProperties.TryAdd(key, value);
            return this;
        }

        public ITelemetrySession AddProperties(IEnumerable<KeyValuePair<string, string>> properties)
        {
            foreach (var item in properties)
            {
                AddProperty(item.Key, item.Value);
            }
            return this;
        }

        private void Save()
        {
            foreach (var operation in _operations)
            {
                operation.Save(_telemetry, _sessionProperties);
            }
        }

        public ITelemetryOperation StartOperation()
        {
            var operation = new TelemetryOperation();
            _operations.Add(operation);
            return operation;
        }

        #region Dispose

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Save();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public class TelemetryOperation : ITelemetryOperation
        {
            private readonly Dictionary<string, string> _operationProperties = new();

            private readonly HashSet<Item<string>> _events = new();
            private readonly HashSet<Item<string>> _traces = new();
            private readonly HashSet<Item<(string name, double value)>> _metrics = new();
            private readonly HashSet<Item<Exception>> _exceptions = new();

            public void Save(TelemetryClient telemetryClient, Dictionary<string, string> sessionProperties)
            {
                var operationProperties = MergeProperties(_operationProperties, sessionProperties);
                foreach (var item in _events)
                {
                    var properties = MergeProperties(item.Properties, operationProperties);
                    telemetryClient.TrackEvent(item.Element, properties);
                }

                foreach (var item in _traces)
                {
                    var properties = MergeProperties(item.Properties, operationProperties);
                    telemetryClient.TrackTrace(item.Element, properties);
                }

                foreach (var item in _metrics)
                {
                    var properties = MergeProperties(item.Properties, operationProperties);
                    telemetryClient.TrackMetric(item.Element.name, item.Element.value, properties);
                }

                foreach (var item in _exceptions)
                {
                    var properties = MergeProperties(item.Properties, operationProperties);
                    telemetryClient.TrackException(item.Element, properties);
                }
            }


            public IDictionary<string, string> MergeProperties(params IDictionary<string, string>[] properties)
            {
                var result = new Dictionary<string, string>();
                foreach (var set in properties)
                {
                    foreach (var prop in set)
                    {
                        result.TryAdd(prop.Key, prop.Value);
                    }
                }
                return result;
            }

            public ITelemetryOperation AddProperty(string key, string value)
            {
                _operationProperties.TryAdd(key, value);
                return this;
            }

            public ITelemetryOperation AddProperties(IEnumerable<KeyValuePair<string, string>> properties)
            {
                foreach (var item in properties)
                {
                    AddProperty(item.Key, item.Value);
                }
                return this;
            }

            public ITelemetryOperation AddEvent(string eventName)
            {
                _events.Add(new(eventName));
                return this;
            }

            public ITelemetryOperation AddEvent(string eventName, IEnumerable<KeyValuePair<string, string>> properties)
            {
                var @event = new Item<string>(eventName);
                foreach (var item in properties)
                {
                    @event.Properties.TryAdd(item.Key, item.Value);
                }
                _events.Add(@event);
                return this;
            }

            public ITelemetryOperation AddMetric(string name, double value)
            {
                _metrics.Add(new((name, value)));
                return this;
            }

            public ITelemetryOperation AddMetric(string name, double value, IEnumerable<KeyValuePair<string, string>> properties)
            {
                var metric = new Item<(string, double)>((name, value));
                foreach (var item in properties)
                {
                    metric.Properties.TryAdd(item.Key, item.Value);
                }
                _metrics.Add(metric);
                return this;
            }

            public ITelemetryOperation AddException(Exception exception)
            {
                _exceptions.Add(new(exception));
                return this;
            }

            public ITelemetryOperation AddException(Exception exception, IEnumerable<KeyValuePair<string, string>> properties)
            {
                var exceptionItem = new Item<Exception>(exception);
                foreach (var item in properties)
                {
                    exceptionItem.Properties.TryAdd(item.Key, item.Value);
                }
                _exceptions.Add(exceptionItem);
                return this;
            }

            public ITelemetryOperation AddTrace(string message)
            {
                _traces.Add(new(message));
                return this;
            }

            public ITelemetryOperation AddTrace(string message, IEnumerable<KeyValuePair<string, string>> properties)
            {
                var trace = new Item<string>(message);
                foreach (var item in properties)
                {
                    trace.Properties.TryAdd(item.Key, item.Value);
                }
                _traces.Add(trace);
                return this;
            }
        }

        private record Item<T>(T Element)
        {
            public Dictionary<string, string> Properties { get; init; } = new();
        }
    }
}