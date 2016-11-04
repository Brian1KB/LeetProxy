using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using LeetProxy.Server.Net;
using MiNET;
using MiNET.Net;

namespace LeetProxy.Server
{
	internal class ProxyMessageHandler : IMcpeMessageHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ProxyMessageHandler));

		private TcpClient _client;
		private readonly INetworkHandler _session;
		private readonly PlayerInfo _playerInfo;

		private readonly object _writerLock = new object();
		private BinaryWriter _writer;

		public ProxyMessageHandler(TcpClient client, INetworkHandler session, PlayerInfo playerInfo)
		{
			_client = client;
			_session = session;
			_playerInfo = playerInfo;

			var stream = _client.GetStream();

			var reader = new BinaryReader(stream);
			_writer = new BinaryWriter(stream);

			StartListener(reader);
		}

		private Thread _listenerThread;

		public void StartListener(BinaryReader reader)
		{
			_listenerThread?.Abort();

			_listenerThread = new Thread(() =>
			{
				while (_client != null)
				{
					try
					{
						var length = reader.ReadInt32();

						if (length == -1)
						{
							Close(true);
							break;
						}

						var type = reader.ReadByte() == 0 ? "mcpe" : "ftl";
						var bytes = reader.ReadBytes(length);

						ThreadPool.QueueUserWorkItem(thread =>
						{
							if (type == "mcpe")
							{
								var package = PackageFactory.CreatePackage(bytes[0], bytes, "mcpe");

								_session.SendPackage(package);
							}
							else
							{
								var package = ProxyPackageFactory.CreatePackage(bytes[0], bytes) as FtlRequestPlayerTransfer;

								Transfer(package?.targetEndpoint);
							}
						});
					}
					catch (Exception e)
					{
						Log.Error("Failed to read from " + _client.Client.RemoteEndPoint + ": " + e);
						break;
					}
				}
			})
			{
				IsBackground = true
			};

			_listenerThread.Start();
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

		private void Transfer(IPEndPoint targetEndPoint)
		{
			try
			{
				Close(false);

				lock (_writerLock)
				{
					_listenerThread.Abort();

					_client = new TcpClient { NoDelay = true };

					_client.Connect(targetEndPoint);

					var stream = _client.GetStream();

					_writer = new BinaryWriter(stream);
					var reader = new BinaryReader(stream);

					var ftlTransferPlayer = new FtlTransferPlayer
					{
						username = _playerInfo.Username,
						clientuuid = _playerInfo.ClientUuid,
						serverAddress = ParseIpEndpoint(_playerInfo.ServerAddress),
						clientId = _playerInfo.ClientId,
						skin = _playerInfo.Skin
					}.Encode();

					_writer.Write(ftlTransferPlayer.Length);
					_writer.Write(ftlTransferPlayer);
					_writer.Flush();

					reader.ReadInt32();

					Log.Info("Successfuly initiated connection transfer to " + targetEndPoint);

					StartListener(reader);
				}
			}
			catch (Exception e)
			{
				Log.Error("Failed to intiate connection transfer: " + e);
				Close(true);
			}
		}

		private void Close(bool closeSession)
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
				catch (Exception e)
				{
					Log.Error("Failed to close writer: " + e);
				}

				if (_client != null)
				{
					_client.Close();
					_client = null;
				}

				if (!closeSession) return;

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
					Close(true);
				}
			}
		}

		public static IPEndPoint ParseIpEndpoint(string endPoint)
		{
			var split = endPoint.Split(':');

			return new IPEndPoint(IPAddress.Parse(split[0]), int.Parse(split[1]));
		}
	}
}