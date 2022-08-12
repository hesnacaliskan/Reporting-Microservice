using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public class IntegrationEvent
    {
        //properties. Bu event ne zaman oluşturuldu ve bu event'in id'si.        
        [JsonInclude]
        public Guid Id { get; private set; }

        [JsonInclude]
        public DateTime CreatedDate { get; private set; } /* set kısımlarını private yapma sebebimiz sadece constructor
                                                             tarafından erişilmesini sağlamak.*/



        //Bunları constructor'dan alalım. Dışarıdan da parametre olarak geliyor gibi düşünelim.
        [JsonConstructor]
        /* Json kullanılarak bir serilization işlemi yapıldıysa burdan alınıp set edilir.
           Serilization : Bir veri yapısını veya nesne durumunu, daha sonra saklanabilecek veya
           iletilebilecek ve yeniden oluşturulabilecek bir formata çevirme işlemidir.*/
        public IntegrationEvent(Guid id, DateTime createdDate)
        {
            Id = id;
            CreatedDate = createdDate;
        }

        // Dışarıdan parametre gelmediği durumu
        public IntegrationEvent()
        {
            Id = Guid.NewGuid(); //Yeni bir id verir.
            CreatedDate = DateTime.UtcNow; // UTC olarak ifade edilen geçerli anın tarih ve saatini alır.
        }
    }
}
