using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Structura.SharedComponents.WebUtilities
{
    public class TransferResult : RedirectResult
    {
        public TransferResult(string url)
            : base(url)
        {
        }

        public TransferResult(object routeValues)
            : base(GetRouteUrl(routeValues))
        {
        }

        public TransferResult(string action, string controller) :
            base(GetRouteUrl(new {controller, action}))
        {
        }

        private static string GetRouteUrl(object routeValues)
        {
            var url = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()),
                                    RouteTable.Routes);
            return url.RouteUrl(routeValues);
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var url = Url; // urlHelper.RouteUrl(this.RouteName, this.RouteValues);

            var httpContext = HttpContext.Current;

            // MVC 3 running on IIS 7+ 
            if (HttpRuntime.UsingIntegratedPipeline)
            {
                httpContext.Server.TransferRequest(url, true);
            }
            else
            {
                // Pre MVC 3 
                httpContext.RewritePath(url, false);

                IHttpHandler httpHandler = new MvcHttpHandler();
                httpHandler.ProcessRequest(httpContext);
            }
        }
    }
}