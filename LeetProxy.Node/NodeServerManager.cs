using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MiNET;
using LeetProxy.Server.Net;

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

					var ftlPackage = ProxyPackageFactory.CreatePackage(buffer[0], buffer);

					if (ftlPackage.GetType() == typeof(FtlCreatePlayer))
					{
						var player = _server.PlayerFactory.CreatePlayer(_server, (IPEndPoint)client.Client.RemoteEndPoint);
						var networkHandler = new NodeNetworkHandler(_server, player, client);

						player.UseCreativeInventory = false;
						player.NetworkHandler = networkHandler;
						player.CertificateData = null;
						player.Username = ((FtlCreatePlayer) ftlPackage).username;
						player.ClientUuid = ((FtlCreatePlayer)ftlPackage).clientuuid;
						player.ServerAddress = ((FtlCreatePlayer)ftlPackage).serverAddress.Address.ToString();
						player.ClientId = ((FtlCreatePlayer)ftlPackage).clientId;
						player.Skin = ((FtlCreatePlayer)ftlPackage).skin;

						ftlPackage.PutPool();
					}
					else if (ftlPackage.GetType() == typeof(FtlTransferPlayer))
					{
						var player = _server.PlayerFactory.CreatePlayer(_server, (IPEndPoint)client.Client.RemoteEndPoint);
						var networkHandler = new NodeNetworkHandler(_server, player, client);

						player.UseCreativeInventory = false;
						player.NetworkHandler = networkHandler;
						player.CertificateData = null;
						player.Username = ((FtlTransferPlayer)ftlPackage).username;
						player.ClientUuid = ((FtlTransferPlayer)ftlPackage).clientuuid;
						player.ServerAddress = ((FtlTransferPlayer)ftlPackage).serverAddress.Address.ToString();
						player.ClientId = ((FtlTransferPlayer)ftlPackage).clientId;
						player.Skin = ((FtlTransferPlayer)ftlPackage).skin;

						ftlPackage.PutPool();
					}

					var writer = new BinaryWriter(stream);

					writer.Write(0);
					writer.Flush();
				}
				catch (Exception e)
				{
					Log.Error("Failed to create player " + e);

					try
					{
						client.Close();
					}
					catch (Exception)
					{
						Log.Error("Error while closing connection " + e);
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