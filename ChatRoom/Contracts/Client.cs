using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Contracts
{
    [DataContract]
    public class Client
    {

        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Group { get; set; }
        public Client(string name)
        {
            this.Username = name;

        }

        public Client() {
            
        }
    }
}
