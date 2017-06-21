using System;
using System.Net;
using MiNET.Net;

namespace LeetProxy.Server.Net
{
	public partial class FtlCreatePlayer : Package<FtlCreatePlayer>
	{
		public string username; // = null;
		public UUID clientuuid; // = null;
		public IPEndPoint serverAddress; // = null;
		public long clientId; // = null;
		public Skin skin; // = null;
		public string certificateData;

		public FtlCreatePlayer()
		{
			Id = 0x01;
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
			Write(certificateData);

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
			certificateData = ReadString();

			AfterDecode();
		}

		partial void BeforeDecode();
		partial void AfterDecode();
	}
}
