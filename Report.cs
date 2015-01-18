using System;
using System.Diagnostics;
using Elmah;

namespace Structura.SharedComponents.WebUtilities
{
    public static class Report
    {
        public static void LogAndRaiseErrorImplementation(string errorDescription)
        {
            //WebUILogging.WebUILog(LoggingAndNotification.UnexpectedError, errorDescription);
            Trace.WriteLine(errorDescription);
            ErrorSignal.FromCurrentContext().Raise(new Exception(errorDescription));
        }

        /// <summary>
        /// Logs error to IFormatLogger and Elmah and adds a non descriptive error message to the modelstate
        /// </summary>
        public static void Error(IReportController controller, string errorDescription)
        {
            LogAndRaiseErrorImplementation(errorDescription);
            if (controller.ModelState != null)
                controller.ModelState.AddModelError(String.Empty
                                                    ,
                                                    string.Format(
                                                        "Ooops, something whent wrong. {0}. The administrator has been notified. ",
                                                        errorDescription));
        }

        /// <summary>
        /// Logs error to IFormatLogger and Elmah and adds a non descriptive error message to the modelstate
        /// </summary>
        public static void Error(IReportController controller, Exception ex, string prefix,
                                 params object[] formatStringParameters)
        {
            controller.FormatLogger.Error(ex, prefix, formatStringParameters);

            ErrorSignal.FromCurrentContext().Raise(ex);

            if (controller.ModelState != null)
            {
                var message = String.Format(prefix, formatStringParameters);
                var error = string.Format(
                    "Ooops, something whent wrong. {0}. The administrator has been notified. ",
                    message);
                controller.ModelState.AddModelError(String.Empty
                                                    , error);
                controller.AddTempMessage(false, error);
            }
        }

        private static void LogAndRaiseAuthorizationError(IReportController controller)
        {
            LogAndRaiseErrorImplementation(string.Format("Access to page '{0}' is denied for user {1})"
                                                         , controller.Request.Url, controller.CurrentUserFullName));
        }

        public static void ReportAuthorizationError(this IReportController controller)
        {
            LogAndRaiseAuthorizationError(controller);

            controller.Response.StatusCode = 403;
            controller.Response.Clear();

            new TransferResult("Unauthorized", "Error").ExecuteResult(controller.ControllerContext);

            controller.Response.End();
        }

        public static object ReturnJsonAuthorizationError(this IReportController controller)
        {
            LogAndRaiseAuthorizationError(controller);
            return
                new
                    {
                        ResultText =
                            "You are not authorized for the requested action, please contact the system administrator."
                    };
        }
    }
}