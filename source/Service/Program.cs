using Topshelf;

namespace Service
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			return (int) HostFactory.Run(c =>
				{
					c.SetServiceName("dtbootstrap-casemonitor");
					c.SetDisplayName("Dovetail Bootstrap Case Monitor");
					c.SetDescription("This is an example of how to implement a windows service that uses the Dovetail Bootstrap library facilitating development of Dovetail SDK applications");
					c.RunAsNetworkService();
					c.EnablePauseAndContinue();

					c.Service(settings => new BootstrapService(settings));

					c.UseLog4Net("bootstrap.log4net");

					//There is a lot more you can do here to configure your service using Topshelf
					// http://docs.topshelf-project.com/en/latest/configuration/index.html
				});
		}
	}
}