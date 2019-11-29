using System;
using System.Net;

namespace UtilityClient
{
	public class ClientMaster
	{
		ushort port;
		TcpCl tcp;

		public ClientMaster(IPEndPoint ip, ushort port)
		{
			tcp = new TcpCl(ip);
			this.port = port;
		}

		public static HostContainer[] ScanLocalNetwork(ushort port)
		{
			return UdpSearchServers.HostLookup(port,"hullo");
		}

		public string[] GetMethods(Methods mType) 
		{
			if (tcp != null)
				return tcp.GetMethods(mType);
			return null;
		}

		public string ExecuteMethod(Methods mType, byte num, byte[] data = null)
		{
			num++;
			string result = "";
			switch (mType)
			{
				case Methods.Simple:
					tcp.ExecuteSimpleMethod(num);
					break;
				case Methods.Data:
					if (data == null)
						return "error no data given";
					tcp.ExecuteDataMethod(num,data);
					break;

			}
			return result;
		}

		public void Disconnect()
		{
			if (tcp != null)
				tcp.Disconnect();
		}

		public bool Connected { get {return tcp.Connected;} }
	}
}
