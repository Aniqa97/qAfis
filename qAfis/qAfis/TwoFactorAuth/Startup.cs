using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TwoFactorAuth.Startup))]
namespace TwoFactorAuth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
