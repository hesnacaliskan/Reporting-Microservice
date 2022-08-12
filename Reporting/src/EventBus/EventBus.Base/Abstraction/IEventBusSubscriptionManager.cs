using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    public interface IEventBusSubscriptionManager
    {
        bool IsEmpty { get; }// Herhangi bir eventi dinliyor muyuz ? Ona bakıyoruz.
        
        event EventHandler<string> OnEventRemoved;// ??
       
        /*void AddDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;*/

        void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void RemoveSubscription<T, TH>()
                where TH : IIntegrationEventHandler<T>
                where T : IntegrationEvent;
        /*void RemoveDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;*/

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;
        // Dışarıdan event gönderildiğinde onu zaten dinleyip dinlemediğimizi kontrol edicek.
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        //Event name gönderildiğinde onun bir type'nı geri dönücek.
        //IntegrationHandler type'nı göndericez.
        void Clear();// Listeyi silmek.
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        //Dışarıdan gelen bir eventin bütün subscriptionlarını ve bütün handlerlarını geriye dönüceğimiz metot.
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        //Dışarıdan event isminin parametre olarak alınması (dynamic değil).
        string GetEventKey<T>();
    }
}
