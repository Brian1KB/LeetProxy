using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using log4net;
using MiNET;
using MiNET.Net;

namespace LeetProxy.Server
{
	internal class ProxyNodeConnection : IServer
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ProxyNodeConnection));
		private static readonly NewtonsoftMapper JsonMapper = new NewtonsoftMapper();
		private readonly IPEndPoint _ipEndPoint;

		public ProxyNodeConnection(IPEndPoint ipEndPoint)
		{
			_ipEndPoint = ipEndPoint;
		}

		public IMcpeMessageHandler CreatePlayer(INetworkHandler session, PlayerInfo playerInfo)
		{
			try
			{
				var client = new TcpClient {NoDelay = true};

				client.Connect(_ipEndPoint);

				var stream = client.GetStream();

				var writer = new BinaryWriter(stream);
				var reader = new BinaryReader(stream);

				var ftlCreatePlayer = new Net.FtlCreatePlayer
				{
					username = playerInfo.Username,
					clientuuid = playerInfo.ClientUuid,
					serverAddress = ParseIpEndpoint(playerInfo.ServerAddress),
					clientId = playerInfo.ClientId,
					skin = playerInfo.Skin,
					certificateData = JsonMapper.Serialize(playerInfo.CertificateData)
				}.Encode();

				writer.Write(ftlCreatePlayer.Length);
				writer.Write(ftlCreatePlayer);
				writer.Flush();

				reader.ReadInt32();

				return new ProxyMessageHandler(client, session, playerInfo);
			}
			catch (Exception e)
			{
				Log.Error("Failed to initiate connection with " + _ipEndPoint.Address + ": " + e);
			}

			return null;
		}

		public static IPEndPoint ParseIpEndpoint(string endPoint)
		{
			var split = endPoint.Split(':');

			return new IPEndPoint(IPAddress.Parse(split[0]), int.Parse(split[1]));
		}
	}
}