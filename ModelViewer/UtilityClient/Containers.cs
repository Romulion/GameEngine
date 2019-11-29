using System;
using System.Net;

namespace UtilityClient
{
	public class HostContainer
	{
		IPEndPoint ip;
		string hostname;

		public HostContainer(string hostname, IPEndPoint ip)
		{
			this.ip = ip;
			this.hostname = hostname;
		}

		public string GetHostname { get { return hostname; } }
		public IPEndPoint GetIP { get { return ip; } }

		public override string ToString()
		{
			return string.Format("{0} {1}", GetHostname, GetIP);
		}
	}
}
