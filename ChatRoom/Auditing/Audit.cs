using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing
{
    public class Audit : IDisposable
    {
        private static EventLog customLog = null;
        const string SourceName = "Auditing.Audit";
        const string LogName = "SecTest";

        static Audit()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                }
                customLog = new EventLog(LogName,
                    Environment.MachineName, SourceName);
            }
            catch (Exception e)
            {
                customLog = null;
                Console.WriteLine("Error while trying to create log handle. Error = {0}", e.Message);
            }
        }

        public static void AuthenticationSuccess(string userName)
        {

            if (customLog != null)
            {
                string AuthenticationSuccess =
                    AuditEvents.AuthenticationSuccess;
                string message = String.Format(AuthenticationSuccess,
                    userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.AuthenticationSuccess));
            }
        }

        public static void AuthenticationFailed(string userName)
        {
            if (customLog != null)
            {
                string AuthenticationFailed =
                    AuditEvents.AuthenticationFailed;
                string message = String.Format(AuthenticationFailed,
                    userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.AuthenticationFailed));
            }
        }

        public static void SecondaryServiceAuthenticationSuccess(string userName)
        {
            if (customLog != null)
            {
                string SecondaryServiceAuthenticationFailed =
                    AuditEvents.SecondaryServiceAuthenticationSuccess;
                string message = String.Format(SecondaryServiceAuthenticationFailed,
                    userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.SecondaryServiceAuthenticationFailed));
            }
        }

        public static void SecondaryServiceAuthenticationFailed(string userName)
        {
            if (customLog != null)
            {
                string SecondaryServiceAuthenticationSuccess =
                    AuditEvents.SecondaryServiceAuthenticationFailed;
                string message = String.Format(SecondaryServiceAuthenticationSuccess,
                    userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.SecondaryServiceAuthenticationFailed));
            }
        }

        public static void MessageSentToSecondaryService(string userName)
        {
            if (customLog != null)
            {
                string MessageSentToSecondaryService =
                    AuditEvents.MessageSentToSecondaryService;
                string message = String.Format(MessageSentToSecondaryService,
                    userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.MessageSentToSecondaryService));
            }
        }

        public static void BackUpServiceAuthenticationSuccess(string userName)
        {
            if (customLog != null)
            {
                string BackUpServiceAuthenticationSuccess =
                    AuditEvents.BackUpServiceAuthenticationSuccess;
                string message = String.Format(BackUpServiceAuthenticationSuccess,
                    userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.BackUpServiceAuthenticationSuccess));
            }
        }

        public static void BackUpServiceAuthenticationFailed(string userName)
        {
            if (customLog != null)
            {
                string BackUpServiceAuthenticationFailed =
                    AuditEvents.MessageSentToSecondaryService;
                string message = String.Format(BackUpServiceAuthenticationFailed,
                    userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventTypes.BackUpServiceAuthenticationFailed));
            }
        }




        public void Dispose()
        {
            if (customLog != null)
            {
                customLog.Dispose();
                customLog = null;
            }
        }
    }
}
