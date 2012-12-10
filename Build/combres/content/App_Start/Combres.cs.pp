[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.Combres), "PreStart")]
namespace $rootnamespace$.App_Start {
	using System.Web.Routing;
	using global::Combres;
	
    public static class Combres {
        public static void PreStart() {
            RouteTable.Routes.AddCombresRoute("Combres");
        }
    }
}