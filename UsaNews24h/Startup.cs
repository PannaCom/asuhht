using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(UsaNews24h.Startup))]
namespace UsaNews24h
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
