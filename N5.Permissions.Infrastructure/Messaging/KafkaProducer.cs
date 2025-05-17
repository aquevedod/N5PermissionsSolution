using Confluent.Kafka;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Application.Common.Models;
using System.Text.Json;

namespace N5.Permissions.Infrastructure.Messaging
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;
        private const string Topic = "permissions-events";

        public KafkaProducer(string bootstrapServers)
        {
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task ProduceAsync(PermissionEvent permissionEvent, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(permissionEvent);

            var message = new Message<Null, string> { Value = json };

            var result = await _producer.ProduceAsync(Topic, message, cancellationToken);
        }
    }
}
