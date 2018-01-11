using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ServerSide.Startup))]

namespace ServerSide
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}