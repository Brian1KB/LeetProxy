using MiNET.Net;

namespace LeetProxy.Server.Net
{
	public class ProxyPackageFactory
	{
		public static Package CreatePackage(byte messageId, byte[] buffer)
		{
			Package package;

			switch (messageId)
			{
				case 0x01:
					package = FtlCreatePlayer.CreateObject();

					package.Decode(buffer);

					return package;
				case 0x02:
					package = FtlTransferPlayer.CreateObject();

					package.Decode(buffer);

					return package;
				case 0x03:
					package = FtlRequestPlayerTransfer.CreateObject();

					package.Decode(buffer);

					return package;
				default:
					return null;
			}
		}
	}
}
