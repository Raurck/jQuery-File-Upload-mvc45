using System.Web;
using System.Web.Mvc;

namespace jQuery_File_Upload_mvc45
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
