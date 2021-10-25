using Auditing;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Single, UseSynchronizationContext = false)]
    public class DataService : IStore
    {
        static List<Message> messages = new List<Message>();

        public List<Message> GetData()
        {
            return messages;
        }

        public void GetMessages(List<Message> msgs)
        {
            messages = msgs;
        }

    }
}
