using System.Web;
using System.Web.Http.Filters;
using Elmah;
using Structura.Ptb.Mvc.Instrumentation;
using Structura.SharedComponents.Utilities;

namespace Structura.SharedComponents.WebUtilities.Elmah
{
    public class HandleWebApiErrorElmahAttribute : ExceptionFilterAttribute
    {
        private readonly IFormatLogger _logger;

        public HandleWebApiErrorElmahAttribute(IFormatLogger logger)
        {
            _logger = logger;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)
            {
                HttpRequestBase request =
                    (actionExecutedContext.Request.Properties["MS_HttpContext"] as HttpContextWrapper).Request;
                _logger.Error(actionExecutedContext.Exception,
                              "Unhandled api exception in application. {0}. Error message: ",
                              RequestDetailsParser.Get(
                                  actionExecutedContext.ActionContext.ControllerContext.RouteData.Values
                                  , actionExecutedContext.ActionContext.ActionDescriptor.ActionName
                                  , actionExecutedContext.ActionContext.ControllerContext.Controller.GetType().Name
                                  , RequestDetailsParser.GetRequestProperties(request)
                                  , request));

                ErrorSignal.FromCurrentContext().Raise(actionExecutedContext.Exception);
            }

            base.OnException(actionExecutedContext);
        }
    }
}