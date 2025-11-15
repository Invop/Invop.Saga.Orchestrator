using System.Reflection;
using System.Text;
using Invop.Saga.Orchestrator.Core.Abstractions;
using Invop.Saga.Orchestrator.Core.Attributes;

namespace Invop.Saga.Orchestrator.Core.Extensions;

internal static class SagaStepExtensions
{
    extension(ISagaStep sagaStepSource)
    {
        public string GetIdempotencyKey()
        {
            ArgumentNullException.ThrowIfNull(sagaStepSource);
            var sb = new StringBuilder();
            sb.Append(sagaStepSource.CorrelationId);
            var stepType = sagaStepSource.GetType();
            var properties = stepType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<IdempotencyKeyAttribute>()
                })
                .Where(x => x.Attribute is not null && x.Property.Name != nameof(IHasCorrelationId.CorrelationId))
                .OrderBy(x => x.Attribute!.Order)
                .ToList();

            foreach (var prop in properties)
            {
                var value = prop.Property.GetValue(sagaStepSource);
                if (value is not null)
                {
                    sb.Append(value);
                }
            }

            var key = sb.ToString();
            return key.ComputeSha256Hash();
        }
    }
}
