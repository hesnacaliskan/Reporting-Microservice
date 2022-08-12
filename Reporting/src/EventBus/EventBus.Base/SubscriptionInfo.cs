using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base
{
    public class SubscriptionInfo //Dışarıdan bize gönderilen verilerin tutulması için kullanılır.
    {
        public bool IsDynamic { get; }
        public Type HandlerType { get; }

        private SubscriptionInfo(bool isDynamic, Type handlerType)
        {
            IsDynamic = isDynamic;
            HandlerType = handlerType;
        }

        public static SubscriptionInfo Dynamic(Type handlerType) => new SubscriptionInfo(true, handlerType);

        public static SubscriptionInfo Typed(Type handlerType) => new SubscriptionInfo(false, handlerType);
    }
}
