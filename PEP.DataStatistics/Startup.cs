using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PEP.DataStatistics.Startup))]
namespace PEP.DataStatistics
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
