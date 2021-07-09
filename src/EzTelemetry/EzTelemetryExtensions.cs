using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace EzTelemetry
{
    public static class EzTelemetryExtensions
    {
        internal const string UnhandledExceptionTelemetryKey = "EzTelemetry:UnhandledException:Source";

        /// <summary>
        /// Start a telemetry operation, catch and log exceptions.
        /// This method never throws.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="action"></param>
        public static void StartSafeOperation(this ITelemetrySession session, Action<ITelemetryOperation> action)
        {
            var operation = session.StartOperation();
            try
            {
                action(operation);
            }
            catch (Exception ex)
            {
                try
                {
                    operation.AddException(ex, new Dictionary<string, string>
                    {
                        [UnhandledExceptionTelemetryKey] = nameof(StartSafeOperation)
                    });
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Start a telemetry operation, catch and log exceptions.
        /// Throws back exceptions.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="session"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TResult StartOperation<TResult>(this ITelemetrySession session, Func<ITelemetryOperation, TResult> action)
        {
            var operation = session.StartOperation();
            try
            {
                return action(operation);
            }
            catch (Exception ex)
            {
                operation.AddException(ex, new Dictionary<string, string>
                {
                    [UnhandledExceptionTelemetryKey] = nameof(StartOperation)
                });
                throw;
            }
        }

        /// <summary>
        /// Start a telemetry operation, catch and log exceptions.
        /// Throws back exceptions.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="action"></param>
        public static void StartOperation(this ITelemetrySession session, Action<ITelemetryOperation> action)
        {
            var operation = session.StartOperation();
            try
            {
                action(operation);
            }
            catch (Exception ex)
            {
                operation.AddException(ex, new Dictionary<string, string>
                {
                    [UnhandledExceptionTelemetryKey] = nameof(StartOperation)
                });
                throw;
            }
        }


        public static TTelemetryStage AddProperties<TTelemetryStage, TProperties>(this TTelemetryStage stage, TProperties properties, string prefix = null, string spacer = ":")
            where TTelemetryStage : ITelemetryStage<TTelemetryStage>
        {
            var items = ConvertToKeyValuePairs(properties);
            foreach (var item in items)
            {
                var key = string.IsNullOrWhiteSpace(prefix) ? item.Key : prefix + spacer + item.Key;
                stage.AddProperty(key, item.Value);
            }
            return stage;
        }

        public static ITelemetryOperation AddEvent<TProperties>(this ITelemetryOperation operation, string eventName, TProperties properties)
        {
            var items = ConvertToKeyValuePairs(properties);
            return operation.AddEvent(eventName, items);
        }

        public static ITelemetryOperation AddMetric<TProperties>(this ITelemetryOperation operation, string name, double value, TProperties properties)
        {
            var items = ConvertToKeyValuePairs(properties);
            return operation.AddMetric(name, value, items);
        }

        public static ITelemetryOperation AddException<TProperties>(this ITelemetryOperation operation, Exception exception, TProperties properties)
        {
            var items = ConvertToKeyValuePairs(properties);
            return operation.AddException(exception, items);
        }

        public static ITelemetryOperation AddTrace<TProperties>(this ITelemetryOperation operation, string message, TProperties properties)
        {
            var items = ConvertToKeyValuePairs(properties);
            return operation.AddTrace(message, items);
        }

        private static IEnumerable<KeyValuePair<string, string>> ConvertToKeyValuePairs<TProperties>(TProperties properties)
        {
            var bindingAttr = BindingFlags.Public | BindingFlags.Instance;
            var type = typeof(TProperties);
            var objectProperties = type.GetProperties(bindingAttr);
            foreach (var propertyInfo in objectProperties)
            {
                var value = propertyInfo.GetValue(properties, null);
                //
                // TODO: implement object properties exploration
                // [x] IEnumerable should render to "{propertyInfo.Name}[{index}]" = value
                // Sub-objects should render to JSON? or "{propertyInfo.Name}.{sub-prop name}"
                // Limit the depth of this to something fairly low; 1 or 2 sub-levels?
                //
                //
                var propertyValue = value.ToString();
                if (propertyValue == propertyInfo.PropertyType.FullName)
                {
                    if (propertyInfo.PropertyType.IsArray)
                    {
                        var array = value as IEnumerable;
                        var index = 0;
                        foreach (var item in array)
                        {
                            if (item.ToString() == item.GetType().FullName)
                            {
                                var subItems = ConvertToKeyValuePairs(item);
                                foreach (var itemProp in subItems)
                                {
                                    yield return new($"{propertyInfo.Name}[{index++}]:{itemProp.Key}", itemProp.Value);
                                }
                            }
                            else
                            {
                                yield return new($"{propertyInfo.Name}[{index++}]", item.ToString());
                            }
                        }
                    }
                    else
                    {
                        // TODO: Sub-objects
                    }
                }
                else
                {
                    yield return new(propertyInfo.Name, propertyValue);
                }
            }
        }
    }
}
