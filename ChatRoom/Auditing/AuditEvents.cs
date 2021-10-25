using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Auditing
{
	public enum AuditEventTypes
	{
		AuthenticationSuccess = 0,
		AuthenticationFailed = 1,
		SecondaryServiceAuthenticationSuccess = 2,
		SecondaryServiceAuthenticationFailed = 3,
		MessageSentToSecondaryService = 4,
		BackUpServiceAuthenticationSuccess = 5,
		BackUpServiceAuthenticationFailed = 6
	}

	public class AuditEvents
	{
		private static ResourceManager resourceManager = null;
		private static object resourceLock = new object();

		private static ResourceManager ResourceMgr
		{
			get
			{
				lock (resourceLock)
				{
					if (resourceManager == null)
					{
						resourceManager = new ResourceManager
							(typeof(AuditEventFile).ToString(),
							Assembly.GetExecutingAssembly());
					}
					return resourceManager;
				}
			}
		}

		public static string AuthenticationSuccess
		{
			get
			{
				return ResourceMgr.GetString(AuditEventTypes.AuthenticationSuccess.ToString());
			}
		}

		

		public static string AuthenticationFailed
		{
			get
			{
				return ResourceMgr.GetString(AuditEventTypes.AuthenticationFailed.ToString());
			}
		}

		public static string SecondaryServiceAuthenticationSuccess 
		{
            get
            {
				return ResourceMgr.GetString(AuditEventTypes.SecondaryServiceAuthenticationSuccess.ToString());
			}
		}

		public static string SecondaryServiceAuthenticationFailed
		{
			get
			{
				return ResourceMgr.GetString(AuditEventTypes.SecondaryServiceAuthenticationFailed.ToString());
			}
		}

		public static string MessageSentToSecondaryService
		{
			get
			{
				return ResourceMgr.GetString(AuditEventTypes.MessageSentToSecondaryService.ToString());
			}
		}

		public static string BackUpServiceAuthenticationSuccess
		{
			get
			{
				return ResourceMgr.GetString(AuditEventTypes.BackUpServiceAuthenticationSuccess.ToString());
			}
		}

		public static string BackUpServiceAuthenticationFailed
		{
			get
			{
				return ResourceMgr.GetString(AuditEventTypes.BackUpServiceAuthenticationFailed.ToString());
			}
		}

	}
}
