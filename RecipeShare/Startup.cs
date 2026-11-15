using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(RecipeShare.StartupOwin))]

namespace RecipeShare
{
    public partial class StartupOwin
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
