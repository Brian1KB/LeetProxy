using System.Net;
using MiNET.Net;

namespace LeetProxy.Server.Net
{
	public partial class FtlRequestPlayerTransfer : Package<FtlRequestPlayerTransfer>
	{
		public IPEndPoint targetEndpoint;

		public FtlRequestPlayerTransfer()
		{
			Id = 0x03;
		}

		protected override void EncodePackage()
		{
			base.EncodePackage();

			BeforeEncode();

			Write(targetEndpoint);

			AfterEncode();
		}

		partial void BeforeEncode();
		partial void AfterEncode();

		protected override void DecodePackage()
		{
			base.DecodePackage();

			BeforeDecode();

			targetEndpoint = ReadIPEndPoint();

			AfterDecode();
		}

		partial void BeforeDecode();
		partial void AfterDecode();
	}
}
