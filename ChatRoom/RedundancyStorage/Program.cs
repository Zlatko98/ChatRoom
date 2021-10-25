using Auditing;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RedundancyStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            string address = "net.tcp://localhost:8069/RedundancyStorage";
            ServiceHost host = new ServiceHost(typeof(ReduntantStorage));
            host.AddServiceEndpoint(typeof(IRedundancy), binding, address);

            

            try
            {
                host.Open();

                try
                {
                    Audit.BackUpServiceAuthenticationSuccess(Environment.UserName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            catch (Exception ex)
            {
                try
                {
                    Audit.BackUpServiceAuthenticationFailed(Environment.UserName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }


                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}
