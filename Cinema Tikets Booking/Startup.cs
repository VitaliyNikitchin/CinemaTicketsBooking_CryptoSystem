using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Cinema_Security.Startup))]
namespace Cinema_Security
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
