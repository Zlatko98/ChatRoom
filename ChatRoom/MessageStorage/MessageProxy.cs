using Contracts;
using Manager;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;

namespace MessageStorage
{
    public class MessageProxy : ChannelFactory<IStore>, IStore, IDisposable
    {
        IStore factory;
        static List<Message> msgs;
        public MessageProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            
        }
        public MessageProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
            Credentials.ServiceCertificate.Authentication.CertificateValidationMode =
                System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            /// Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);


            factory = this.CreateChannel();

            
            //Credentials.Windows.AllowNtlm = false;
        }


        public List<Message> GetData()
        {
           
            msgs = new List<Message>();
            try
            {
                msgs = factory.GetData();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            return msgs;
        }

    }
}
