using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SeoAnalyzerWebApp.Startup))]
namespace SeoAnalyzerWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
        }
    }
}
