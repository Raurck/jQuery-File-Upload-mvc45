using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(jQuery_File_Upload_mvc45.Startup))]
namespace jQuery_File_Upload_mvc45
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
