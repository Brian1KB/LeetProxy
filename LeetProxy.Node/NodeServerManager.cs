using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MiNET;
using MiNET.Net;

namespace LeetProxy.Node
{
	internal class NodeServerManager : IServerManager
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(NodeServerManager));

		private readonly MiNetServer _server;
		private readonly TcpListener _listener;

		public NodeServerManager(MiNetServer server, int port)
		{
			_server = server;
			
			_listener = new TcpListener(IPAddress.Parse("192.168.0.12"), port);
			_listener.Start();

			_listener.BeginAcceptTcpClient(AcceptTcpClient, _listener);
		}

		private void AcceptTcpClient(IAsyncResult ar)
		{
			var client = _listener.EndAcceptTcpClient(ar);

			client.NoDelay = true;

			ThreadPool.QueueUserWorkItem(thread =>
			{
				try
				{
					var stream = client.GetStream();
					var reader = new BinaryReader(stream);

					var length = reader.ReadInt32();
					var buffer = reader.ReadBytes(length);

					var ftlCreatePlayer = (FtlCreatePlayer)PackageFactory.CreatePackage(buffer[0], buffer, "ftl");

					var player = _server.PlayerFactory.CreatePlayer(_server, (IPEndPoint) client.Client.RemoteEndPoint);
					var networkHandler = new NodeNetworkHandler(_server, player, client);

					player.UseCreativeInventory = false;
					player.NetworkHandler = networkHandler;
					player.CertificateData = null;
					player.Username = ftlCreatePlayer.username;
					player.ClientUuid = ftlCreatePlayer.clientuuid;
					player.ServerAddress = ftlCreatePlayer.serverAddress;
					player.ClientId = ftlCreatePlayer.clientId;
					player.Skin = ftlCreatePlayer.skin;

					ftlCreatePlayer.PutPool();

					var writer = new BinaryWriter(stream);

					writer.Write(0);
					writer.Flush();
				}
				catch (Exception e)
				{
					Log.Info("Failed to create player " + e);

					try
					{
						client.Close();
					}
					catch (Exception)
					{
					}
				}
			});

			_listener.BeginAcceptTcpClient(AcceptTcpClient, _listener);
		}

		public IServer GetServer()
		{
			return null;
		}
	}
}