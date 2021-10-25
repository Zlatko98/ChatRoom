using Contracts;
using Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageStorage
{
    class Program
    {
        static List<Message> messages;
        static void Main(string[] args)
        {
            NetTcpBinding binding_alt = new NetTcpBinding();
            string address = "net.tcp://localhost:8070/WPFServer";
            binding_alt.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            NetTcpBinding binding_red = new NetTcpBinding();
            binding_red.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            string addr = "net.tcp://localhost:8069/RedundancyStorage";
            string srvCertCN = "wcfservice";
            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);

            if (srvCert != null)
            {
                try
                {
                    Auditing.Audit.SecondaryServiceAuthenticationSuccess(Environment.UserName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                try
                {
                    Auditing.Audit.SecondaryServiceAuthenticationFailed(Environment.UserName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            EndpointAddress endpointAddress = new EndpointAddress(new Uri(address), new X509CertificateEndpointIdentity(srvCert));
            EndpointAddress endpointAddress1 = new EndpointAddress(new Uri(addr), EndpointIdentity.CreateUpnIdentity("redundancy"));
            using (MessageProxy proxy = new MessageProxy(binding_alt, endpointAddress))
            using (RedundancyProxy px = new RedundancyProxy(binding_red, endpointAddress1))
            {

                messages = new List<Message>();
                while (true)
                {
                    int size = messages.Count;
                    messages = proxy.GetData();
                    Thread.Sleep(5000);
                    messages = proxy.GetData();
                    StoreData(messages);
                    px.StoreData(messages);

                    if (messages.Count > size)
                    {
                        try
                        {
                            Auditing.Audit.MessageSentToSecondaryService(Environment.UserName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                }

            }
        }

        public static void StoreData(List<Message> messages)
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
