using System;
using System.Web;
using System.Web.Mvc;
using Elmah;
using Structura.Ptb.Mvc.Instrumentation;
using Structura.SharedComponents.Utilities;

namespace Structura.SharedComponents.WebUtilities.Elmah
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandleErrorElmahAttribute : HandleErrorAttribute
    {
        private const string _elmahErrorForUserSessionKey = "ElmahErrorForUser";

        public HandleErrorElmahAttribute(IFormatLogger logger)
        {
            Check.RequireNotNull(logger);
            _logger = logger;
        }

        public IFormatLogger _logger { get; set; }

        public static string ElmahErrorForUserKey(string userName)
        {
            return string.Format("{0}{1}", _elmahErrorForUserSessionKey, userName);
        }

        public override void OnException(ExceptionContext filterContext)
        {
            //base.OnException(filterContext);

            Exception e = filterContext.Exception;
            //if (filterContext.ExceptionHandled)   // if unhandled, will be logged anyhow
            //    return;

            if (!filterContext.HttpContext.Items.Contains("LoggerBinding"))
            {
                _logger.Error(e, "Unhandled api exception in application. {0}. Error message: ",
                              RequestDetailsParser.Get(
                                  filterContext.RouteData.Values
                                  , "?"
                                  , filterContext.Controller.ControllerContext.Controller.GetType().Name
                                  , RequestDetailsParser.GetRequestProperties(filterContext.HttpContext.Request)
                                  , filterContext.HttpContext.Request));
            }

            if (RaiseErrorSignal(e)) // prefer signaling, if possible
                return;
            if (IsFiltered(filterContext)) // filtered?
                return;

            Report.Error(null, e, "Unhandled exception");
        }

        private static bool RaiseErrorSignal(Exception e)
        {
            var context = HttpContext.Current;
            if (context == null)
                return false;
            ErrorSignal signal = ErrorSignal.FromContext(context);
            if (signal == null)
                return false;
            signal.Raise(e, context);
            return true;
        }

        private static bool IsFiltered(ExceptionContext context)
        {
            var config = context.HttpContext.GetSection("elmah/errorFilter")
                         as ErrorFilterConfiguration;

            if (config == null)
                return false;

            var testContext = new ErrorFilterModule.AssertionHelperContext(
                context.Exception, HttpContext.Current);

            return config.Assertion.Test(testContext);
        }
    }
}