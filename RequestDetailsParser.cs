using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Structura.Ptb.Mvc.Instrumentation
{
    public static class RequestDetailsParser
    {
        public static string GetRequestProperties(HttpRequestBase request)
        {
            return string.Format("{0} IP: {1}, User: {2}" // , Referrer: {3}
                                 , request.Url.Query
                                 , request.UserHostAddress
                                 , HttpContext.Current.User.Identity.Name);
        }

        public static string Get(IDictionary<string, object> routeValues, string actionName, string controllerName,
                                 string requestProperties, HttpRequestBase request)
        {
            var routeDataString = new StringBuilder();
            foreach (var r in routeValues)
            {
                if (r.Key == "action" || r.Key == "controller")
                    continue;
                if (routeDataString.Length > 0)
                    routeDataString.Append(", ");

                routeDataString.Append(r.Key);
                routeDataString.Append("=");
                routeDataString.Append(r.Value);
            }

            // if route data is empty, we use the form values
            foreach (object key in request.Form.Keys)
            {
                if (key == null || key.ToString() == "__RequestVerificationToken")
                    continue;

                if (routeDataString.Length > 0)
                    routeDataString.Append(", ");

                routeDataString.Append(key);
                routeDataString.Append("=");
                routeDataString.Append(request.Form[key.ToString()]);
            }

            return string.Format("{0} {1}/{2} {3}, RouteData: {4}"
                                 , request.HttpMethod
                                 , controllerName
                                 , actionName
                                 , requestProperties
                                 , routeDataString);
        }
    }
}