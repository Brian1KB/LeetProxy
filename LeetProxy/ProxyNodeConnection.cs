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

				var ftlCreatePlayer = new FtlCreatePlayer()
				{
					username = playerInfo.Username,
					clientuuid = playerInfo.ClientUuid,
					serverAddress = playerInfo.ServerAddress,
					clientId = playerInfo.ClientId,
					skin = playerInfo.Skin
				}.Encode();

				writer.Write(ftlCreatePlayer.Length);
				writer.Write(ftlCreatePlayer);
				writer.Flush();

				reader.ReadInt32();

				Log.Info("Successfully initiated connection with " + _ipEndPoint);

				return new ProxyMessageHandler(client, session);
			}
			catch (Exception e)
			{
				Log.Error("Failed to initiate connection with " + _ipEndPoint.Address + ": " + e);
			}

			return null;
		}
	}
}