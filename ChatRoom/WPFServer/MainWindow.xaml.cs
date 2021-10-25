using Contracts;
using Manager;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows;

namespace WPFServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        ServiceHost host;
        ServiceHost host_alt;
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = true;
            NetTcpBinding binding = new NetTcpBinding();
            NetTcpBinding binding_alt = new NetTcpBinding();
            binding_alt.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            string address = "net.tcp://" + textBoxIP.Text.ToString() + ":" + textBoxPort.Text.ToString() + "/WPFServer";
            string address_alt = "net.tcp://localhost:8070/WPFServer";

            host = new ServiceHost(typeof(ChatService));
            
            host_alt = new ServiceHost(typeof(DataService));
            host.AddServiceEndpoint(typeof(IChat), binding, address);
            host_alt.AddServiceEndpoint(typeof(IStore), binding_alt, address_alt);

            ServiceMetadataBehavior mBehave = new ServiceMetadataBehavior();
            host.Description.Behaviors.Add(mBehave);

            host.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexTcpBinding(),
                "net.tcp://" + textBoxIP.Text.ToString() + ":" +
                "7997" + "/WPFServer/mex");

            host_alt.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
                    System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            host_alt.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            string srvCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            host_alt.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            ServiceSecurityAuditBehavior newAudit = new ServiceSecurityAuditBehavior();
            newAudit.AuditLogLocation = AuditLogLocation.Application;
            newAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;

            host.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            host.Description.Behaviors.Add(newAudit);

            host_alt.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            host_alt.Description.Behaviors.Add(newAudit);

            string hostName = Dns.GetHostName(); // Retrive the Name of HOST  

            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            

            try
            {
                host.Open();
                host_alt.Open();
                statusText.Text = "Connection opened at " + "net.tcp://" + myIP + ":" + textBoxPort.Text.ToString();
            } catch(Exception ex)
            {
                statusText.Text = ex.Message;
            }
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            if (host != null)
            {
                try
                {
                    host.Close();
                    host_alt.Close();
                }
                catch (Exception ex)
                {
                    statusText.Text = ex.Message.ToString();
                }
                finally
                {
                    if (host.State == CommunicationState.Closed)
                    {
                        statusText.Text = "Closed";
                        buttonStart.IsEnabled = true;
                        buttonStop.IsEnabled = false;
                    }
                }
            }
        }
    }
}
