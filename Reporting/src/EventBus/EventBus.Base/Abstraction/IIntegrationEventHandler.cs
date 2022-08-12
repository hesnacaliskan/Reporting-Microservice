using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    // TIntegrationEvent interface'in aldığı dinamik bir tip. Bu tipin IntegrationEvent olma zorunluluğu var.
    public interface IIntegrationEventHandler<in TIntegrationEvent>: IntegrationEventHandler 
           where TIntegrationEvent : IntegrationEvent 
    {
        Task Handle(TIntegrationEvent @event); // @event parametre ismi.
                                               // Handle metodu ile gelen IntegrationEvent'i implement etmiş olucaz.
    }

    public interface IntegrationEventHandler
    {
    }
}
