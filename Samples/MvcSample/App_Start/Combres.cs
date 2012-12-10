[assembly: WebActivator.PreApplicationStartMethod(typeof(MvcSample.App_Start.Combres), "PreStart")]
namespace MvcSample.App_Start {
	using System.Web.Routing;
	using global::Combres;
	
    public static class Combres {
        public static void PreStart() {
            RouteTable.Routes.AddCombresRoute("Combres");
        }
    }
}