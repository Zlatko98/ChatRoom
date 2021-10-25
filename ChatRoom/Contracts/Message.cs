using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public string Sender { get; set; }
        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
        [DataMember]
        public string MessageSenderGroup { get; set; }

        public override string ToString()
        {
            return Time + " " + MessageSenderGroup + " " + Sender + " " + Msg;
            
        }

    }
}
