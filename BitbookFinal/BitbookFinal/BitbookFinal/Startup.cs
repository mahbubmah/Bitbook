using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BitbookFinal.Startup))]
namespace BitbookFinal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
