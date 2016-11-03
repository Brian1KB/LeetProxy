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
	internal class NodeNetworkHandler : INetworkHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(NodeNetworkHandler));

		private TcpClient _client;

		private readonly Player _player;
		private readonly MiNetServer _server;

		private BinaryWriter _writer;

		private readonly object _writerLock = new object();

		public NodeNetworkHandler(MiNetServer server, Player player, TcpClient client)
		{
			_server = server;
			_player = player;
			_client = client;

			new Thread(() =>
			{
				try
				{
					_server.ServerInfo.PlayerSessions.TryAdd((IPEndPoint) _client.Client.RemoteEndPoint, null);

					var stream = client.GetStream();
					var reader = new BinaryReader(stream);
					_writer = new BinaryWriter(stream);

					while (_client != null)
					{
						var length = reader.ReadInt32();
						var buffer = reader.ReadBytes(length);
						var package = PackageFactory.CreatePackage(buffer[0], buffer, "mcpe");

						if (_server != null)
							Interlocked.Increment(ref _server.ServerInfo.NumberOfPacketsInPerSecond);
						HandlePackage(package);
						package.PutPool();
					}
				}
				catch (Exception)
				{
					try
					{
						_player.Disconnect("Lost connection", false);
					}
					catch (Exception)
					{
						_client = null;
					}
				}
			})
			{
				IsBackground = true
			}.Start();
		}

		public void Close()
		{
			PlayerNetworkSession playerNetworkSession;
			_server.ServerInfo.PlayerSessions.TryRemove((IPEndPoint)_client.Client.RemoteEndPoint, out playerNetworkSession);

			lock (_writerLock)
			{
				if (_writer != null)
				{
					try
					{
						_writer.Write(-1);
						_writer.Flush();
						_writer.Close();
					}
					catch (Exception)
					{
					}
				}
			}

			_client?.Close();
		}

		public IPEndPoint GetClientEndPoint()
		{
			return null;
		}

		public void SendDirectPackage(Package package)
		{
			SendPackage(package);
		}

		public void SendPackage(Package package)
		{
			if (!_player.IsConnected) return;
			if (_server != null)
				Interlocked.Increment(ref _server.ServerInfo.NumberOfPacketsOutPerSecond);

			lock (_writerLock)
			{
				if (_writer == null) return;

				try
				{
					var buffer = package.Encode();

					_writer.Write(buffer.Length);
					_writer.Write(buffer);
					_writer.Flush();
				}
				catch (Exception)
				{
					Close();
				}
			}

			package.PutPool();
		}

		private void HandlePackage(Package package)
		{
			var player = _player;

			if (typeof(McpeDisconnect) == package.GetType())
			{
				var mcpeDisconnect = (McpeDisconnect)package;
				Log.Warn("Got disconnect in node: " + mcpeDisconnect.message);
				player.Disconnect(mcpeDisconnect.message, false);
			}

			else if (typeof(McpeClientMagic) == package.GetType())
			{
				player.HandleMcpeClientMagic((McpeClientMagic)package);
			}

			else if (typeof(McpeResourcePackClientResponse) == package.GetType())
			{
				player.HandleMcpeResourcePackClientResponse((McpeResourcePackClientResponse)package);
			}

			else if (typeof(McpeResourcePackChunkRequest) == package.GetType())
			{
				player.HandleMcpeResourcePackChunkRequest((McpeResourcePackChunkRequest)package);
			}

			else if (typeof(McpeUpdateBlock) == package.GetType())
			{
				// DO NOT USE. Will dissapear from MCPE any release. 
				// It is a bug that it leaks these packages.
			}

			else if (typeof(McpeRemoveBlock) == package.GetType())
			{
				player.HandleMcpeRemoveBlock((McpeRemoveBlock)package);
			}

			else if (typeof(McpeAnimate) == package.GetType())
			{
				player.HandleMcpeAnimate((McpeAnimate)package);
			}

			else if (typeof(McpeUseItem) == package.GetType())
			{
				player.HandleMcpeUseItem((McpeUseItem)package);
			}

			else if (typeof(McpeEntityEvent) == package.GetType())
			{
				player.HandleMcpeEntityEvent((McpeEntityEvent)package);
			}

			else if (typeof(McpeText) == package.GetType())
			{
				player.HandleMcpeText((McpeText)package);
			}

			else if (typeof(McpeRemoveEntity) == package.GetType())
			{
				// Do nothing right now, but should clear out the entities and stuff
				// from this players internal structure.
			}

			else if (typeof(McpeLogin) == package.GetType())
			{
				player.HandleMcpeLogin((McpeLogin)package);
			}

			else if (typeof(McpeMovePlayer) == package.GetType())
			{
				player.HandleMcpeMovePlayer((McpeMovePlayer)package);
			}

			else if (typeof(McpeCommandStep) == package.GetType())
			{
				player.HandleMcpeCommandStep((McpeCommandStep)package);
			}

			else if (typeof(McpeInteract) == package.GetType())
			{
				player.HandleMcpeInteract((McpeInteract)package);
			}

			else if (typeof(McpeRespawn) == package.GetType())
			{
				player.HandleMcpeRespawn((McpeRespawn)package);
			}

			else if (typeof(McpeBlockEntityData) == package.GetType())
			{
				player.HandleMcpeBlockEntityData((McpeBlockEntityData)package);
			}

			else if (typeof(McpePlayerAction) == package.GetType())
			{
				player.HandleMcpePlayerAction((McpePlayerAction)package);
			}

			else if (typeof(McpeDropItem) == package.GetType())
			{
				player.HandleMcpeDropItem((McpeDropItem)package);
			}

			else if (typeof(McpeContainerSetSlot) == package.GetType())
			{
				player.HandleMcpeContainerSetSlot((McpeContainerSetSlot)package);
			}

			else if (typeof(McpeContainerClose) == package.GetType())
			{
				player.HandleMcpeContainerClose((McpeContainerClose)package);
			}

			else if (typeof(McpeMobEquipment) == package.GetType())
			{
				player.HandleMcpeMobEquipment((McpeMobEquipment)package);
			}

			else if (typeof(McpeMobArmorEquipment) == package.GetType())
			{
				player.HandleMcpeMobArmorEquipment((McpeMobArmorEquipment)package);
			}

			else if (typeof(McpeCraftingEvent) == package.GetType())
			{
				player.HandleMcpeCraftingEvent((McpeCraftingEvent)package);
			}

			else if (typeof(McpeRequestChunkRadius) == package.GetType())
			{
				player.HandleMcpeRequestChunkRadius((McpeRequestChunkRadius)package);
			}

			else if (typeof(McpeMapInfoRequest) == package.GetType())
			{
				player.HandleMcpeMapInfoRequest((McpeMapInfoRequest)package);
			}

			else if (typeof(McpeItemFramDropItem) == package.GetType())
			{
				player.HandleMcpeItemFramDropItem((McpeItemFramDropItem)package);
			}

			else if (typeof(McpeItemFramDropItem) == package.GetType())
			{
				player.HandleMcpePlayerInput((McpePlayerInput)package);
			}

			else
			{
				Log.Error($"Unhandled package: {package.GetType().Name} 0x{package.Id:X2} for user: {_player.Username}");
				if (Log.IsDebugEnabled) Log.Warn($"Unknown package 0x{package.Id:X2}\n{Package.HexDump(package.Bytes)}");
			}
		}
	}
}