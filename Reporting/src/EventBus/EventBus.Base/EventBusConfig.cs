using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base
{
    public class EventBusConfig
    {
        

        public int ConnectionRetryCount { get; set; } = 5;
        //RabbitMQ'ya bağlanırken en fazla beş kere dene özelliği.


        public string DefaultTopicName { get; set; } = "SellingBuddyEventBus";
        //Dışarıdan bir TopicName alamazsa sistemimiz hata almaması için Default bir TopicName veriyoruz.


        public string EventBusConnectionString { get; set; } = String.Empty;


        public string SubscriberClientAppName { get; set; } = String.Empty;
        //Hangi servisin yeni bir Q yaratacağını belirler.


        public string EventNamePrefix { get; set; } = String.Empty;


        public string EventNameSuffix { get; set; } = "IntegrationEvent";


        public EventBusType EventBusType { get; set; } = EventBusType.RabbitMQ;
        //Default olarak dışarıdan bağlanıcağımız EventBusımızın tipi RabbitMQ olacak.


        public object Connection { get; set; }


        public bool DeleteEventPrefix => !String.IsNullOrEmpty(EventNamePrefix);

        public bool DeleteEventSuffix => !String.IsNullOrEmpty(EventNameSuffix);

  
    }
    public enum EventBusType
    {
        RabbitMQ = 0,
        AzureServicesBus = 1

    }

}



        










        


           


    
