using System.Web;
using System.Web.Compilation;
using System.Web.Routing;

public class WebFormRouteHandler<T> : IRouteHandler where T : IHttpHandler, new()
{
    public string VirtualPath { get; set; }

    public WebFormRouteHandler(string virtualPath)
    {
        VirtualPath = virtualPath;
    }

    #region IRouteHandler Members

    public IHttpHandler GetHttpHandler(RequestContext requestContext)
    {
        return (VirtualPath != null)
            ? (IHttpHandler)BuildManager.CreateInstanceFromVirtualPath(VirtualPath, typeof(T))
            : new T();
    }

    #endregion
}