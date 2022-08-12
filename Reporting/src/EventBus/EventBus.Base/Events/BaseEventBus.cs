using EventBus.Base.Abstraction;
using EventBus.Base.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly IEventBusSubscriptionManager SubsManager;

        private EventBusConfig eventBusConfig;

        public BaseEventBus(EventBusConfig config, IServiceProvider serviceProvider)
        {
            eventBusConfig = config;
            ServiceProvider = serviceProvider;
            SubsManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);//Subscription manager
        }


        public virtual string ProcessEventName(string eventName)
        {
            if (eventBusConfig.DeleteEventPrefix) //Eğer başından bir şeyin silinmesi seçilmişse başındaki harfleri kırp.
                eventName = eventName.TrimStart(eventBusConfig.EventNamePrefix.ToArray());

            if (eventBusConfig.DeleteEventSuffix)//Sonundaki harfleri kırpacak.
                eventName = eventName.TrimEnd(eventBusConfig.EventNameSuffix.ToArray());

            return eventName;
        }

        public virtual string GetSubName(string eventName) //Event name i getirme metodu.Override edilebilir.
        {
            return $"{eventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }
        //Metod ezilebilsin diye virtual olarak işaretledim.
        public virtual void Dispose()
        {
            eventBusConfig = null; //Dispose işlemi gerçekleştiğinde evenBusConfig' e null değeri atanacak. 
        }

        public async Task<bool> ProcessEvent(string eventName, string message) //RabbitMQ nun fırlattığı event bu metoda düşecek.
        {
            eventName = ProcessEventName(eventName);//Event name'in son halini eventName'e ata.

            var processed = false;
            //Event dinlenmiş mi 
            if (SubsManager.HasSubscriptionsForEvent(eventName))
            {
                //dinlenmişse eğer kaç kişi dinlediğini subscriptions değişkenine atadım.
                var subscriptions = SubsManager.GetHandlersForEvent(eventName);
                //Hepsinin aynı scope'da türetilmesini istediğimiz için Service Provider ile Scope Creation işlemi yaptım.
                using (IServiceScope? scope = ServiceProvider.CreateScope())
                {

                    foreach (var subscription in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subscription.HandlerType);
                        if (handler == null) continue;

                        var eventType = SubsManager.GetEventTypeByName($"{eventBusConfig.EventNamePrefix}{eventName}{eventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);


                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });


                    }
                }

                processed = true;
            }

            return processed;
        }
        // Aşağıdaki Publish, Subscribe ve Unsubscribe işlemleri kullanacağımız RabbitMQ ya özel olacak
        public abstract void Publish(IntegrationEvent @event);


        public abstract void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        public abstract void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

      
    }
}
