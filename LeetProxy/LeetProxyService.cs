using log4net;
using MiNET;
using Topshelf;

namespace LeetProxy.Server
{
	public class LeetProxyService
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(LeetProxyService));
		private MiNetServer _server;

		private void Start()
		{
			Log.Info("Starting LeetProxy");

			_server = new MiNetServer
			{
				ServerRole = ServerRole.Proxy,
				ServerManager = new ProxyServerManager(),
				MotdProvider = new ProxyMotdProvider()
			};

			_server.StartServer();
		}

		private void Stop()
		{
			Log.Info("Stopping LeetProxy");

			_server.StopServer();
		}

		private static void Main()
		{
			HostFactory.Run(host =>
			{
				host.Service<LeetProxyService>(s =>
				{
					s.ConstructUsing(construct => new LeetProxyService());
					s.WhenStarted(service => service.Start());
					s.WhenStopped(service => service.Stop());
				});

				host.RunAsLocalService();
				host.SetDisplayName("LeetProxy Service");
				host.SetDescription("LeetProxy MiNET Proxy Service");
				host.SetServiceName("LeetProxy");
			});
		}
	}
}
