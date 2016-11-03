using MiNET;
using MiNET.Utils;

namespace LeetProxy.Server
{
	internal class ProxyMotdProvider : MotdProvider
	{
		public ProxyMotdProvider()
		{
			Motd = $"{ChatColors.Red}LEET{ChatColors.DarkGray}Proxy{ChatColors.Gray} - MiNET Proxy";
			SecondLine = $"{ChatColors.Red}LEET{ChatColors.DarkGray}Proxy{ChatColors.Green}";
		}
	}
}