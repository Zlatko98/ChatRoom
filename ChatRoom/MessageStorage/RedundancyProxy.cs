using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessageStorage
{

    public class RedundancyProxy : ChannelFactory<IRedundancy>, IRedundancy, IDisposable
    {
        IRedundancy factory;
        
        public RedundancyProxy(NetTcpBinding binding, string address) : base(binding, address)
        {

        }
        public RedundancyProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
              
            factory = this.CreateChannel();
            //Credentials.Windows.AllowNtlm = false;
        }

        public void StoreData(List<Message> messages)
        {
            try
            {
                factory.StoreData(messages);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
            
    }
}

