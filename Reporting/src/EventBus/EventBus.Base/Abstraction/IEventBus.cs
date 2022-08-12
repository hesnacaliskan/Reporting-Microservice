using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    public interface IEventBus
    {
        void Publish(IntegrationEvent @event); //Servisimiz event fırlatacağı zaman bu metodu kullanacak.

        void Subscribe<T, TH>()
            where T: IntegrationEvent 
            where TH : IIntegrationEventHandler<T>; // where diyerek kısıtlama yapmış oluyoruz bu burdan olmak zorunda şeklinde.

        void Unsubscribe<T, TH>() 
            where T: IntegrationEvent 
            where TH: IIntegrationEventHandler<T>;

    }
}
