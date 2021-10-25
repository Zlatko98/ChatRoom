using Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RedundancyStorage
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Single, UseSynchronizationContext = false)]
    public class ReduntantStorage : IRedundancy
    {
        static List<Message> messages = new List<Message>();

        public void StoreData(List<Message> messages)
        {
            List<string> groups = new List<string>();

            foreach (Message m in messages)
            {
                if (groups.Contains(m.MessageSenderGroup))
                {
                    continue;
                }
                else
                {
                    groups.Add(m.MessageSenderGroup);
                }
            }
            foreach (string g in groups)
            {
                using (StreamWriter sw = new StreamWriter(new FileStream(g + ".txt", FileMode.Create)))
                {
                    foreach (Message msg in messages)
                    {
                        if (msg.MessageSenderGroup == g)
                            sw.WriteLine(msg.ToString());

                    }
                    sw.Close();

                }


            }
        }
    }
}
