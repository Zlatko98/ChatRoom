using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ServiceModel;
using WPFClient.SVC;
using System.Windows.Threading;
using Contracts;
using System.Threading;
using Auditing;
using System.Linq;

namespace WPFClient
{
    /// < summary>
    /// Interaction logic for Window1.xaml
    /// < /summary>
    public partial class Window1 : Window, SVC.IChatCallback
    {
        //SVC holds references to the proxy and cotracts..
        ChatClient proxy = null;
        Client receiver = null;
        Client localClient = null;
        
        
        //When the communication object 
        //turns to fault state it will
        //require another thread to invoke a fault event
        private delegate void FaultedInvoker();

        //This will hold each online client with 
        //a listBoxItem to quickly handle adding
        //and removing clients when they join or leave
        Dictionary<ListBoxItem, Client> OnlineClients =
                      new Dictionary<ListBoxItem, Client>();


        public Window1()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
            chatListBoxNames.SelectionChanged += new
              SelectionChangedEventHandler(
              chatListBoxNames_SelectionChanged);
            chatTxtBoxType.KeyDown +=
              new KeyEventHandler(chatTxtBoxType_KeyDown);
            chatTxtBoxType.KeyUp +=
              new KeyEventHandler(chatTxtBoxType_KeyUp);
            
        }


        //Service might be disconnected or stopped for any reason,
        //so we have to handle the state of the communication object,
        //the communication object will fire 
        //an event for each transitioning
        //from a state to another, notice that when a connection state goes
        //from opening to opened or from opened to closing state.. it can't go
        //back so, if it is closed or faulted you have to set the proxy = null;
        //to be able to create a proxy again and open a connection
        //..
        //I have made a method called HandleProxy() to handle the state
        //of the connection, so in each event like opened, closed or faulted
        //we will call this method, and it will switch on the connection state
        //and apply a suitable reaction.
        //..
        //Because this events will need to be invoked on another thread
        //you can do like so in WPF applications (I've got this idea from
        //Sacha Barber's greate article on WCF WPF Application)

        void InnerDuplexChannel_Closed(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        void InnerDuplexChannel_Opened(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        #region Private Methods

        /// < summary>
        /// This is the most method I like, it helps us alot
        /// We may can't know when a connection is lost in 
        /// of network failure or service stopped.
        /// And also to maintain performance client doesnt know
        /// that the connection will be lost when hitting the 
        /// disconnect button, but when a session is terminated
        /// this method will be called, and it will handle everything.
        /// < /summary>
        private void HandleProxy()
        {
            
            if (proxy != null)
            {
                switch (this.proxy.State)
                {
                    case CommunicationState.Closed:
                        proxy = null;
                        chatListBoxMsgs.Items.Clear();
                        chatListBoxNames.Items.Clear();
                        loginLabelStatus.Content = "Disconnected";
                        ShowChat(false);
                        ShowLogin(true);
                        loginButtonConnect.IsEnabled = true;
                        break;
                    case CommunicationState.Closing:
                        break;
                    case CommunicationState.Created:
                        break;
                    case CommunicationState.Faulted:
                        proxy.Abort();
                        proxy = null;
                        chatListBoxMsgs.Items.Clear();
                        chatListBoxNames.Items.Clear();
                        ShowChat(false);
                        ShowLogin(true);
                        loginLabelStatus.Content = "Disconnected";
                        loginButtonConnect.IsEnabled = true;
                        break;
                    case CommunicationState.Opened:
                        ShowLogin(false);
                        ShowChat(true);

                        chatLabelCurrentStatus.Content = "online";
                        chatLabelCurrentUName.Content = this.localClient.Username;

                        break;
                    case CommunicationState.Opening:
                        break;
                    default:
                        break;
                }
            }

        }

        /// < summary>
        /// This is the second important method, which creates 
        /// the proxy, subscribe to connection state events
        /// and open a connection with the service
        /// < /summary>
        private void Connect()
        {

            if (proxy == null)
            {
                try
                {
                    this.localClient = new Client();

                    this.localClient.Username = loginTxtBoxUName.Text.ToString();
                 
                    InstanceContext context = new InstanceContext(this);
                    proxy = new ChatClient(context);


                    //As the address in the configuration file is set to localhost
                    //we want to change it so we can call a service in internal 
                    //network, or over internet
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.Transport;
                    binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
        
                    proxy.Endpoint.Binding = binding;

                    string servicePath = proxy.Endpoint.ListenUri.AbsolutePath;
                    string serviceListenPort =
                      loginTxtBoxPort.Text.ToString();
                    proxy.Endpoint.Address = new EndpointAddress("net.tcp://"
                       + loginTxtBoxIP.Text.ToString() + ":" +
                       serviceListenPort + servicePath);

                    proxy.Open();

                    proxy.InnerDuplexChannel.Faulted +=
                      new EventHandler(InnerDuplexChannel_Faulted);
                    proxy.InnerDuplexChannel.Opened +=
                      new EventHandler(InnerDuplexChannel_Opened);
                    proxy.InnerDuplexChannel.Closed +=
                      new EventHandler(InnerDuplexChannel_Closed);
                    proxy.ConnectAsync(this.localClient);
                    proxy.ConnectCompleted += new EventHandler<
                          ConnectCompletedEventArgs>(proxy_ConnectCompleted);

                    try
                    {
                        Audit.AuthenticationSuccess(Environment.UserName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                catch (Exception ex)
                {
                    loginTxtBoxUName.Text = ex.Message.ToString();
                    loginLabelStatus.Content = "Offline";
                    loginButtonConnect.IsEnabled = true;
                }
            }
            else
            {
                HandleProxy();
            }
        }

        private void Send()
        {
            if (proxy != null && chatTxtBoxType.Text != "")
            {
                if (proxy.State == CommunicationState.Faulted)
                {
                    HandleProxy();
                }
                else
                {
                    //Create message, assign its properties
                    Message msg = new Message();
                    msg.Sender = this.localClient.Username;
                    msg.Msg = chatTxtBoxType.Text.ToString();
                    //msg.Msg = this.localClient.Username;

                    proxy.SayAsync(msg);
                    chatTxtBoxType.Text = "";
                    chatTxtBoxType.Focus();

                    //Tell the service to tell back 
                    //all clients that this client
                    //has just finished typing..
                    proxy.IsWritingAsync(null);
                }
            }
        }


        /// < summary>
        /// This method to enable us scrolling the list box of messages
        /// when a new message comes from the service..
        /// < /summary>
        private ScrollViewer FindVisualChild(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ScrollViewer)
                {
                    return (ScrollViewer)child;
                }
                else
                {
                    ScrollViewer childOfChild = FindVisualChild(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        /// < summary>
        /// This is an important method which is called whenever
        /// a message comes from the service, a client joins or
        /// leaves, to return a ready item to be added in the
        /// list box (either the one for messages or the one for
        /// clients).
        /// < /summary>
        private ListBoxItem MakeItem(string text)
        {
            ListBoxItem item = new ListBoxItem();


            TextBlock txtblock = new TextBlock();
            txtblock.Text = text;
            txtblock.VerticalAlignment = VerticalAlignment.Center;

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(item);
            panel.Children.Add(txtblock);

            ListBoxItem bigItem = new ListBoxItem();
            bigItem.Content = panel;

            return bigItem;
        }


        /// < summary>
        /// Show or hide login controls depends on the parameter
        /// < /summary>
        /// < param name="show">< /param>
        private void ShowLogin(bool show)
        {
            if (show)
            {
                loginButtonConnect.Visibility = Visibility.Visible;
                loginLabelIP.Visibility = Visibility.Visible;
                loginLabelStatus.Visibility = Visibility.Visible;
                loginLabelTitle.Visibility = Visibility.Visible;
                loginLabelUName.Visibility = Visibility.Visible;
                loginPolyLine.Visibility = Visibility.Visible;
                loginTxtBoxIP.Visibility = Visibility.Visible;
                loginTxtBoxUName.Visibility = Visibility.Visible;
            }
            else
            {
                loginButtonConnect.Visibility = Visibility.Collapsed;
                loginLabelIP.Visibility = Visibility.Collapsed;
                loginLabelStatus.Visibility = Visibility.Collapsed;
                loginLabelTitle.Visibility = Visibility.Collapsed;
                loginLabelUName.Visibility = Visibility.Collapsed;
                loginPolyLine.Visibility = Visibility.Collapsed;
                loginTxtBoxIP.Visibility = Visibility.Collapsed;
                loginTxtBoxUName.Visibility = Visibility.Collapsed;
            }
        }


        /// < summary>
        /// Show or hide chat controls depends on the parameter
        /// < /summary>
        /// < param name="show">< /param>
        private void ShowChat(bool show)
        {
            if (show)
            {
                chatButtonDisconnect.Visibility = Visibility.Visible;
                chatButtonSend.Visibility = Visibility.Visible;
                chatLabelCurrentStatus.Visibility = Visibility.Visible;
                chatLabelCurrentUName.Visibility = Visibility.Visible;
                chatListBoxMsgs.Visibility = Visibility.Visible;
                chatListBoxNames.Visibility = Visibility.Visible;
                chatTxtBoxType.Visibility = Visibility.Visible;
                chatLabelWritingMsg.Visibility = Visibility.Visible;
                chatLabelSendFileStatus.Visibility = Visibility.Visible;
            }
            else
            {
                chatButtonDisconnect.Visibility = Visibility.Collapsed;
                chatButtonSend.Visibility = Visibility.Collapsed;
                chatLabelCurrentStatus.Visibility = Visibility.Collapsed;
                chatLabelCurrentUName.Visibility = Visibility.Collapsed;
                chatListBoxMsgs.Visibility = Visibility.Collapsed;
                chatListBoxNames.Visibility = Visibility.Collapsed;
                chatTxtBoxType.Visibility = Visibility.Collapsed;
                chatLabelWritingMsg.Visibility = Visibility.Collapsed;
                chatLabelSendFileStatus.Visibility = Visibility.Collapsed;
            }
        }

        #endregion


        #region UI_Events

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            ShowChat(false);
            ShowLogin(true);

        }


        protected override void OnClosing(
                  System.ComponentModel.CancelEventArgs e)
        {
            if (proxy != null)
            {
                if (proxy.State == CommunicationState.Opened)
                {
                    proxy.Disconnect(this.localClient);
                    //dont set proxy.Close(); because 
                    //isTerminating = true on Disconnect()
                    //and this by default will call 
                    //HandleProxy() to take care of this.
                }
                else
                {
                    HandleProxy();
                }
            }
        }

        private void buttonConnect_Click(object sender,
                                   RoutedEventArgs e)
        {
            loginButtonConnect.IsEnabled = false;
            loginLabelStatus.Content = "Connecting..";
            proxy = null;
            Connect();

        }

        void proxy_ConnectCompleted(object sender,
                   ConnectCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                loginLabelStatus.Foreground =
                        new SolidColorBrush(Colors.Red);
                loginTxtBoxUName.Text = e.Error.Message.ToString();
                loginButtonConnect.IsEnabled = true;
            }
            else if (e.Result)
            {
                HandleProxy();
            }
            else if (!e.Result)
            {
                loginLabelStatus.Content = "Name found";
                loginButtonConnect.IsEnabled = true;
            }


        }

        private void chatButtonSend_Click(object sender,
                                          RoutedEventArgs e)
        {
            Send();
        }

        private void chatButtonDisconnect_Click(object sender,
                                          RoutedEventArgs e)
        {
            if (proxy != null)
            {
                if (proxy.State == CommunicationState.Faulted)
                {
                    HandleProxy();
                }
                else
                {
                    proxy.DisconnectAsync(this.localClient);
                }
            }
        }


        void chatTxtBoxType_KeyUp(object sender, KeyEventArgs e)
        {
            if (proxy != null)
            {
                if (proxy.State == CommunicationState.Faulted)
                {
                    HandleProxy();
                }
                else
                {
                    if (chatTxtBoxType.Text.Length < 1)
                    {
                        proxy.IsWritingAsync(null);
                    }
                }
            }
        }

        void chatTxtBoxType_KeyDown(object sender, KeyEventArgs e)
        {
            if (proxy != null)
            {
                if (proxy.State == CommunicationState.Faulted)
                {
                    HandleProxy();
                }
                else
                {
                    if (e.Key == Key.Enter)
                    {
                        Send();
                    }
                    else if (chatTxtBoxType.Text.Length < 1)
                    {
                        proxy.IsWritingAsync(this.localClient);
                    }
                }
            }
        }

        void chatListBoxNames_SelectionChanged(object sender,
                              SelectionChangedEventArgs e)
        {
            
            ListBoxItem item =
                chatListBoxNames.SelectedItem as ListBoxItem;
            if (item != null)
            {
                this.receiver = this.OnlineClients[item];
            }
        }


        #endregion

        #region IChatCallback Members

        public void RefreshClients(List<Client> clients)
        {
            string group = clients.Where(x => x.Username == this.localClient.Username).FirstOrDefault().Group;
            chatListBoxNames.Items.Clear();
            OnlineClients.Clear();

            foreach (Client c in clients)
            {
                if(c.Group == group)
                {
                    ListBoxItem item = MakeItem(c.Username);
                    chatListBoxNames.Items.Add(item);
                    OnlineClients.Add(item, c);
                }   

            }
        }

        public void Receive(Message msg)
        {
            foreach (Client c in this.OnlineClients.Values)
            {
                if (c.Username == msg.Sender && msg.MessageSenderGroup == c.Group)
                {
                    if (msg.MessageSenderGroup == "Chatters")
                    {
                        string converted = "";
                        for (int i = 0; i < msg.Msg.Length; i++)
                        {
                            if (i % 2 == 0)
                            {
                                converted += msg.Msg[i];
                            }
                            else
                            {
                                if (msg.Msg[i] == '0')
                                    converted += '1';
                                else
                                    converted += '0';
                            }
                        }
                        var data = GetBytesFromBinaryString(converted);
                        var text = Encoding.UTF8.GetString(data);
                        ListBoxItem item = MakeItem(msg.Sender + " at " + msg.Time.ToShortTimeString() + " : " + text);
                        chatListBoxMsgs.Items.Add(item);
                    }
                    else if (msg.MessageSenderGroup == "Admins") 
                    {
                        char[] buffer = (msg.Msg).ToCharArray();
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            // Letter.
                            char letter = buffer[i];
                            // Add shift to all.
                            if (Char.IsLetter(letter))
                            {
                                letter = (char)(letter + 23);
                                // Subtract 26 on overflow.
                                // Add 26 on underflow.
                                if (letter > 'z')
                                {
                                    letter = (char)(letter - 26);
                                } else if (letter < 'z')
                                {
                                    letter = (char)(letter - 26);

                                }
                                
                                buffer[i] = letter;
                            } else
                            {
                                continue;
                            }
                            
                        }
                        string text = new string(buffer);
                        ListBoxItem item = MakeItem(msg.Sender + " at " + msg.Time.ToShortTimeString() + " : " + text);
                        chatListBoxMsgs.Items.Add(item);
                    }else if (msg.MessageSenderGroup == "VIPs") 
                    {
                        char xorKey = 'P';

                        // Define String to store encrypted/decrypted String 
                        string outputString = "";

                        // calculate length of input string 
                        int len = msg.Msg.Length;

                        // perform XOR operation of key 
                        // with every caracter in string 
                        for (int i = 0; i < len; i++)
                        {
                            outputString = outputString + char.ToString((char)(msg.Msg[i] ^ xorKey));
                        }
                        
                        ListBoxItem item = MakeItem(msg.Sender + " at " + msg.Time.ToShortTimeString() + " : " + outputString);
                        chatListBoxMsgs.Items.Add(item);
                    }
                    else
                    {
                        ListBoxItem item = MakeItem(msg.Sender + " at " + msg.Time.ToShortTimeString() + " : " + msg.Msg);
                        chatListBoxMsgs.Items.Add(item);
                    }
                    
                }
            }
            ScrollViewer sv = FindVisualChild(chatListBoxMsgs);
            sv.LineDown();
        }



        public void IsWritingCallback(Client client)
        {
            if (client == null)
            {
                chatLabelWritingMsg.Content = "";
            }
            else
            {
                if(client.Username != this.localClient.Username)
                    chatLabelWritingMsg.Content += client.Username +
                      " is writing a message.., ";
            }
        }



        public void UserJoin(Client client)
        {
            
            ListBoxItem item = MakeItem(client.Username + " has joined the chat --- " + DateTime.Now.ToShortTimeString());
            chatListBoxMsgs.Items.Add(item);
            
            ScrollViewer sv = FindVisualChild(chatListBoxMsgs);
            sv.LineDown();
        }

        public void UserLeave(Client client)
        {
            ListBoxItem item = MakeItem(client.Username + " has left the chat --- " + DateTime.Now.ToShortTimeString());
            chatListBoxMsgs.Items.Add(item);
            ScrollViewer sv = FindVisualChild(chatListBoxMsgs);
            sv.LineDown();
        }

        public IAsyncResult BeginRefreshClients(List<Client> clients, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndRefreshClients(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceive(Message msg, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndReceive(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginIsWritingCallback(Client client, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndIsWritingCallback(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginUserJoin(Client client, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndUserJoin(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginUserLeave(Client client, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndUserLeave(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public Byte[] GetBytesFromBinaryString(String binary)
        {
            var list = new List<Byte>();

            for (int i = 0; i < binary.Length; i += 8)
            {
                String t = binary.Substring(i, 8);

                list.Add(Convert.ToByte(t, 2));
            }

            return list.ToArray();
        }



        #endregion

        private void loginTxtBoxPort_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}