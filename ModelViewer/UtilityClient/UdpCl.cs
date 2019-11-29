using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;

namespace UtilityClient
{
	internal static class UdpSearchServers
	{

		public static HostContainer[] HostLookup(ushort port, string keycode)
		{
			UdpClient Udp = new UdpClient();
            Udp.Client.ReceiveTimeout = 1000;

			List<HostContainer> hosts = new List<HostContainer>(5);
			IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Broadcast,port);

			try
			{
				//broadcast hullo packet
				byte[] data = Encoding.UTF8.GetBytes(keycode);
				Udp.Send(data, data.Length, RemoteIpEndPoint);

				//read responses
				while (true)
				{
					byte[] receiveBytes = Udp.Receive(ref RemoteIpEndPoint);
					hosts.Add(new HostContainer(Encoding.UTF8.GetString(receiveBytes),RemoteIpEndPoint));
				}


			}
			catch (SocketException se)
			{
				if (se.ErrorCode != 10060)
					throw se;
			}
			catch (Exception ex)
			{
				//Console.WriteLine("Возникло исключение: " + ex.Message);
			}
			finally
			{
				Udp.Close();
			}

			if (hosts.Count == 0)
				return null;

			return hosts.ToArray();
				
		}
	}
}