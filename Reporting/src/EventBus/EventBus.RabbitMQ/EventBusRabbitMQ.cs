using EventBus.Base;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{

    public class EventBusRabbitMQ : BaseEventBus
    {
        RabbitMQPersistentConnection persistentConnection;
        private readonly IConnectionFactory connectionFactory;
        private readonly IModel consumerChannel;

        public object RefenceLoopHandling { get; private set; }
        public EventHandler<BasicDeliverEventArgs> Consumer_Received { get; private set; }
        public EventHandler<string> SubsManager_OnEventRemoved { get; private set; }

        public EventBusRabbitMQ(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {

            if (config.Connection != null)// EventBusConfig ile gelen connection'ın null'dan farklı olup olmadığı bilgisi
            {

                var connJson = JsonConvert.SerializeObject(EventBusConfig.Connection , new JsonSerializerSettings()
                //JsonConvert için NewtonSoft.Json eklenmeli
                {
                    // Self referencing loop detected for property
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    
                });

                connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson);
                // Dışarıdan gönderilmiş ise kendi ConnectionFactory modelimize çevirdik.

            }
            else //Dışarıdan gönderilmemiş ise default değerlerle kullanabilmek için 
                connectionFactory = new ConnectionFactory();


            persistentConnection = new RabbitMQPersistentConnection(connectionFactory, config.ConnectionRetryCount);

            consumerChannel = CreatConsumerChannel();

            SubsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
             eventName = ProcessEventName(eventName);
             if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();  
            }   

            // using var channel = persistentConnection.CreateModel();
            consumerChannel.QueueBind(queue: eventName, // Queue'lar silinemediğinden
                                                        // onları dinlemekten vazgeçiyoruz.(UnBind)
                exchange: EventBusConfig.DefaultTopicName,
                routingKey: eventName);

            if (SubsManager.IsEmpty)
            {
                consumerChannel.Close();
            }

             
        }

        private object ProcessEventName(object eventName)
        {
            throw new NotImplementedException();
        }

        public override void Publish(IntegrationEvent @event)
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()//using.Polly(); ekle
                .Or<SocketException>()
                .WaitAndRetry(EventBusConfig.ConnectionRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    // loglama yapılabilir
                });
            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            consumerChannel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct"); // Ensure exchange exsist while publishing

            var message = JsonConvert.SerializeObject(@event);// Dışarıdan gelen bir event var ve message'a
                                                              // set ettik yani string'e dönüştürdük.
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                consumerChannel.QueueDeclare(queue: GetSubName(eventName), //ensure queue exsist while publishing
                    //queue'nun olup olmadığını kontrol ediyor.
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                consumerChannel.BasicPublish( //BasicPublish metodunu kullanarak datamızı karşı tarafa gönderme.
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

            });


        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if (!SubsManager.HasSubscriptionsForEvent(eventName))
            {
                if (persistentConnection.IsConnected)//bağlı olup olmama kontrolü
                {
                    persistentConnection.TryConnect();
                }

                consumerChannel.QueueDeclare(queue: GetSubName(eventName), // ensure queue exists while consuming     
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                consumerChannel.QueueBind(queue: GetSubName(eventName),
                                  exchange: EventBusConfig.DefaultTopicName,
                                  routingKey: eventName);

            }

            SubsManager.AddSubscription<T, TH>();
            StartBasicConsume(eventName);
        }

        public override void Unsubscribe<T, TH>()
        {
            SubsManager.RemoveSubscription<T, TH>();
        }

        private IModel CreatConsumerChannel()
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();//connected değil ise bağlanmayı dene.
            }

            var channel = persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName,
                                     type: "direct");
            return channel;
        }


        private void StartBasicConsume(string eventName)
        {
            if (consumerChannel != null)
            {
                var consumer = new /*Async*/EventingBasicConsumer(consumerChannel);// Bir consumer yarattık.

                consumer.Received += Consumer_Received;

                consumerChannel.BasicConsume(
                    queue: GetSubName(eventName),
                    autoAck: false,
                    consumer: consumer);
            }
        }

        private async void Consumer_Receveid(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            eventName = ProcessEventName(eventName);
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);// Dışarıdan gelen mesajı stringe çevirdik
                                                                       // ve process event'e gönderdik.

            try
            {
                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                 // logging kullanılabilir.
            }

            consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
    }
}
