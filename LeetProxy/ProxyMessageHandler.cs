using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MiNET;
using MiNET.Net;

namespace LeetProxy.Server
{
	internal class ProxyMessageHandler : IMcpeMessageHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ProxyMessageHandler));

		private TcpClient _client;
		private readonly INetworkHandler _session;

		private readonly object _writerLock = new object();
		private BinaryWriter _writer;

		public ProxyMessageHandler(TcpClient client, INetworkHandler session)
		{
			_client = client;
			_session = session;

			var stream = client.GetStream();

			var reader = new BinaryReader(stream);
			_writer = new BinaryWriter(stream);

			new Thread(() =>
			{
				while (_client != null)
				{
					try
					{
						var length = reader.ReadInt32();

						if (length == -1)
						{
							Close();
							break;
						}

						var bytes = reader.ReadBytes(length);

						ThreadPool.QueueUserWorkItem(thread =>
						{
							var package = PackageFactory.CreatePackage(bytes[0], bytes, "mcpe");

							_session.SendPackage(package);
						});
					}
					catch (Exception e)
					{
						Log.Error("Failed to read from " + _client.Client.RemoteEndPoint + ": " + e);
						Close();
					}
				}
			})
			{
				IsBackground = true
			}.Start();
		}

		public void Disconnect(string reason, bool sendDisconnect = true)
		{
			var disconnect = McpeDisconnect.CreateObject();
			disconnect.message = reason;

			WriteBytes(disconnect.Encode());

			disconnect.PutPool();
		}

		public void HandleMcpeLogin(McpeLogin message)
		{
			WritePackage(message);
		}

		public void HandleMcpeClientMagic(McpeClientMagic message)
		{
			if (message == null)
			{
				message = new McpeClientMagic();

				WriteBytes(message.Encode());

				return;
			}

			WritePackage(message);
		}

		public void HandleMcpeResourcePackClientResponse(McpeResourcePackClientResponse message)
		{
			WritePackage(message);
		}

		public void HandleMcpeText(McpeText message)
		{
			WritePackage(message);
		}

		public void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			WritePackage(message);
		}

		public void HandleMcpeRemoveBlock(McpeRemoveBlock message)
		{
			WritePackage(message);
		}

		public void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			WritePackage(message);
		}

		public void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			WritePackage(message);
		}

		public void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
		{
			WritePackage(message);
		}

		public void HandleMcpeInteract(McpeInteract message)
		{
			WritePackage(message);
		}

		public void HandleMcpeUseItem(McpeUseItem message)
		{
			WritePackage(message);
		}

		public void HandleMcpePlayerAction(McpePlayerAction message)
		{
			WritePackage(message);
		}

		public void HandleMcpeAnimate(McpeAnimate message)
		{
			WritePackage(message);
		}

		public void HandleMcpeRespawn(McpeRespawn message)
		{
			WritePackage(message);
		}

		public void HandleMcpeDropItem(McpeDropItem message)
		{
			WritePackage(message);
		}

		public void HandleMcpeContainerClose(McpeContainerClose message)
		{
			WritePackage(message);
		}

		public void HandleMcpeContainerSetSlot(McpeContainerSetSlot message)
		{
			WritePackage(message);
		}

		public void HandleMcpeCraftingEvent(McpeCraftingEvent message)
		{
			WritePackage(message);
		}

		public void HandleMcpeBlockEntityData(McpeBlockEntityData message)
		{
			WritePackage(message);
		}

		public void HandleMcpePlayerInput(McpePlayerInput message)
		{
			WritePackage(message);
		}

		public void HandleMcpeMapInfoRequest(McpeMapInfoRequest message)
		{
			WritePackage(message);
		}

		public void HandleMcpeRequestChunkRadius(McpeRequestChunkRadius message)
		{
			WritePackage(message);
		}

		public void HandleMcpeItemFramDropItem(McpeItemFramDropItem message)
		{
			WritePackage(message);
		}

		public void HandleMcpeResourcePackChunkRequest(McpeResourcePackChunkRequest message)
		{
			WritePackage(message);
		}

		public void HandleMcpeCommandStep(McpeCommandStep message)
		{
			WritePackage(message);
		}

		private void Close()
		{
			lock (_writerLock)
			{
				try
				{
					if (_writer != null)
					{
						_writer.Flush();
						_writer.Close();
						_writer = null;
					}
				}
				catch (Exception)
				{
				}

				if (_client != null)
				{
					_client.Close();
					_client = null;
				}

				_session.Close();
			}
		}

		private void WritePackage(Package package)
		{
			WriteBytes(package.Bytes);
		}

		private void WriteBytes(byte[] package)
		{
			lock (_writerLock)
			{
				try
				{
					_writer.Write(package.Length);
					_writer.Write(package);
					_writer.Flush();
				}
				catch (Exception e)
				{
					Log.Error("Failed to write to " + _client.Client.RemoteEndPoint + ": " + e);
					Close();
				}
			}
		}
	}
}