using Confluent.Kafka;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Application.Common.Models;
using System.Net;
using System.Text.Json;

namespace N5.Permissions.Infrastructure.Messaging
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;
        private const string Topic = "permissions-events";

        public KafkaProducer(string bootstrapServers)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                ClientId = Dns.GetHostName(),
                SocketTimeoutMs = 6000,
                MessageTimeoutMs = 5000,
                RequestTimeoutMs = 5000,
                EnableDeliveryReports = true,
                Acks = Acks.All

            };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task ProduceAsync(PermissionEvent permissionEvent, CancellationToken cancellationToken)
        {
            try
            {
                var json = JsonSerializer.Serialize(permissionEvent);
                var message = new Message<Null, string> { Value = json };

                var result = await _producer.ProduceAsync(Topic, message, cancellationToken);

                if (result.Status != PersistenceStatus.Persisted)
                {
                    throw new Exception($"Message was not persisted. Message: {result.Message.Value}");
                }
            }
            catch (ProduceException<Null, string> ex)
            {
                throw new Exception($"Error producing message: {ex.Error.Reason}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Kafka producer", ex);
            }
        }
    }
}
