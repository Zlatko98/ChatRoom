using Auditing;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace WPFServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ChatService : IChat
    {
       
        Dictionary<Client, IChatCallback> clientsDict = new Dictionary<Client, IChatCallback>();
        List<Client> clientList = new List<Client>();
        List<Message> messages = new List<Message>();
        DataService ds = new DataService();
        

        public IChatCallback CurrentCallback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IChatCallback>();

            }
        }

        object syncObj = new object();

        private bool SearchClientsByName(string name)
        {
            foreach (Client c in clientsDict.Keys)
            {
                if (c.Username == name)
                {
                    return true;
                }
            }
            return false;
        }
        public bool Connect(Client client)
        {
            if (Thread.CurrentPrincipal.IsInRole("chatters"))
            {
                client.Group = "Chatters";

            }
            else if (Thread.CurrentPrincipal.IsInRole("admins")) 
            {
                client.Group = "Admins";
            }
            else if (Thread.CurrentPrincipal.IsInRole("vips"))
            {
                client.Group = "VIPs";
            }
            else
            {
                client.Group = "Unassigned";
            }

            if (!clientsDict.ContainsValue(CurrentCallback) && !SearchClientsByName(client.Username))
            {
                lock (syncObj)
                {
                    clientsDict.Add(client, CurrentCallback);
                    clientList.Add(client);
                    
                    foreach (Client key in clientsDict.Keys)
                    {
                        IChatCallback callback = clientsDict[key];
                        try
                        {
                            callback.UserJoin(client);
                            callback.RefreshClients(clientList);

                        }
                        catch
                        {
                            clientsDict.Remove(key);
                            return false;
                        }

                    }

                }
                
                return true;
            }
            

            return false;
        }

        public void Say(Message msg)
        {
            string mssg = "";
            string converted = "";
            if (Thread.CurrentPrincipal.IsInRole("chatters"))
            {
                msg.MessageSenderGroup = "Chatters";
                mssg = string.Join("", System.Text.Encoding.UTF8.GetBytes(msg.Msg).Select(n => Convert.ToString(n, 2).PadLeft(8, '0')));
                for (int i = 0; i < mssg.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        converted += mssg[i];
                    }
                    else
                    {
                        if (mssg[i] == '0')
                            converted += '1';
                        else
                            converted += '0';
                    }
                }
                msg.Msg = converted;

            }
            else if (Thread.CurrentPrincipal.IsInRole("admins"))
            {
                msg.MessageSenderGroup = "Admins";
                mssg = msg.Msg;

                char[] buffer = mssg.ToCharArray();
                for (int i = 0; i < buffer.Length; i++)
                {
                    // Letter.
                    char letter = buffer[i];
                    // Add shift to all.
                    if (Char.IsLetter(letter))
                    {
                        letter = (char)(letter + 3);
                        // Subtract 26 on overflow.
                        // Add 26 on underflow.
                        if (letter > 'z')
                        {
                            letter = (char)(letter - 26);
                        }
                        else if (letter < 'a' && (letter < 'A' || letter > 'Z'))
                        {
                            letter = (char)(letter + 26);
                        }
                        // Store.
                        buffer[i] = letter;
                    }
                    else
                    {
                        continue;
                    }
                    
                }
                msg.Msg = new string(buffer);
            } else if (Thread.CurrentPrincipal.IsInRole("vips"))
            {
                msg.MessageSenderGroup = "VIPs";
                mssg = msg.Msg;

                char xorKey = 'P';

                // Define String to store encrypted/decrypted String 
                string outputString = "";

                // calculate length of input string 
                int len = mssg.Length;

                // perform XOR operation of key 
                // with every caracter in string 
                for (int i = 0; i < len; i++)
                {
                    outputString = outputString +
                    char.ToString((char)(mssg[i] ^ xorKey));
                }

                msg.Msg = outputString;
            }
            else
            {
                msg.MessageSenderGroup = "Unassigned";
            }
            msg.Time = DateTime.Now;
            messages.Add(msg);
            ds.GetMessages(messages);
            lock (syncObj)
            {
                foreach(KeyValuePair<Client, IChatCallback> item in clientsDict)
                {
                    if(msg.MessageSenderGroup == item.Key.Group)
                    {
                        item.Value.Receive(msg);
                    }

                }
            }
        }

        public void IsWriting(Client client)
        {
            lock (syncObj)
            {
                foreach (IChatCallback callback in clientsDict.Values)
                {
              
                    callback.IsWritingCallback(client);
                }
            }
        }

        public void Disconnect(Client client)
        {
            foreach (Client c in clientsDict.Keys)
            {
                if (client.Username == c.Username)
                {
                    lock (syncObj)
                    {
                        this.clientsDict.Remove(c);
                        this.clientList.Remove(c);
                        foreach (IChatCallback callback in clientsDict.Values)
                        {
                            callback.RefreshClients(this.clientList);
                            callback.UserLeave(client);
                        }
                    }
                    return;
                }
            }
        }

        //public List<Message> GetData()
        //{
        //    return messages;
        //}

    }
}
