using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UtilityClient
{
	public enum Methods {Simple = 3, Data}

	public class TcpCl
	{
		TcpClient session;
		NetworkStream stream;

		public TcpCl(IPEndPoint ip)
		{
			session = new TcpClient();
			session.Connect(ip);
			stream = session.GetStream();
		}


		public string[] GetMethods(Methods m) 
		{
			byte[] data = new byte[2];
			data[0] = 1;
			data[1] = (byte)m;
			stream.Write(data, 0, data.Length);
			data = new byte[256];
			int length = stream.Read(data, 0, data.Length);
			string text = Encoding.ASCII.GetString(data, 0, length);
			string[] rows = text.Split('\n');
			return rows;

		}

		public void ExecuteSimpleMethod(byte num)
		{
			byte[] data = { 3, num };
			stream.Write(data, 0, data.Length);
			data = new byte[256];
			stream.Read(data, 0, data.Length);
			//Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));
		}

		public void ExecuteDataMethod(byte num, byte[] data)
		{
			byte[] length = BitConverter.GetBytes(data.Length);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(length);
			//4 type for methods with data 1- method number
			byte[] req = { 4, num, 0, 0, 0, 0};

			Array.Copy(length, 0, req, 2, 4);
			stream.Write(req,0,6);
			stream.ReadByte();
			stream.Write(data,0,data.Length);

			byte[] answer = new byte[100];
			int len = stream.Read(answer,0,answer.Length);
			//Console.WriteLine("sended");
			//Console.WriteLine(Encoding.ASCII.GetString(answer,0,len));
		}

		public void Disconnect()
		{
			if (session.Connected)
			{
				stream.WriteByte(255);
				stream.Close();
			}
		}

		public bool Connected { get {return session.Connected;} }

	}

}
