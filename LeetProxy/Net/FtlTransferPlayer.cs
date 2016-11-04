using System.Net;
using MiNET.Net;

namespace LeetProxy.Server.Net
{
	public partial class FtlTransferPlayer : Package<FtlTransferPlayer>
	{
		public string username; // = null;
		public UUID clientuuid; // = null;
		public IPEndPoint serverAddress; // = null;
		public long clientId; // = null;
		public Skin skin; // = null;
		public string extraData;

		public FtlTransferPlayer()
		{
			Id = 0x02;
		}

		protected override void EncodePackage()
		{
			base.EncodePackage();

			BeforeEncode();

			Write(username);
			Write(clientuuid);
			Write(serverAddress);
			Write(clientId);
			Write(skin);
			Write(extraData);

			AfterEncode();
		}

		partial void BeforeEncode();
		partial void AfterEncode();

		protected override void DecodePackage()
		{
			base.DecodePackage();

			BeforeDecode();

			username = ReadString();
			clientuuid = ReadUUID();
			serverAddress = ReadIPEndPoint();
			clientId = ReadLong();
			skin = ReadSkin();
			extraData = ReadString();

			AfterDecode();
		}

		partial void BeforeDecode();
		partial void AfterDecode();
	}
}
