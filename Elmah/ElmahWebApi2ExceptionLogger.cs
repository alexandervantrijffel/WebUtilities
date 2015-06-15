using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ExceptionHandling;
using Elmah;
using Structura.SharedComponents.Utilities;

namespace Structura.SharedComponents.WebUtilities.Elmah
{
    /// <summary>
    /// WebApi 2 ExceptionLogger that sends a formatted exception message to Elmah and IFormatLogger. 
    /// Includes request Uri, payload and client host address in the error message. 
    /// </summary>
    public class ElmahWebApi2ExceptionLogger : ExceptionLogger
    {
        public override async Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            // Retrieve the current HttpContext instance for this request.
            var httpContext = GetHttpContext(context.Request);

            if (httpContext == null)
                return;

            // Wrap the exception in an HttpUnhandledException so that ELMAH can capture the original error page.
            var exceptionToRaise = new HttpUnhandledException(await FormatLogMessageAsync(context, httpContext), context.Exception);

            FormatLoggerAccessor.Instance().Error(exceptionToRaise, string.Empty);

            // Send the exception to ELMAH (for logging, mailing, filtering, etc.).
            ErrorSignal.FromContext(httpContext).Raise(exceptionToRaise, httpContext);
        }

        //public override void Log(ExceptionLoggerContext context)
        //{
        //    // Retrieve the current HttpContext instance for this request.
        //    var httpContext = GetHttpContext(context.Request);

        //    if (httpContext == null)
        //        return;

        //    // Wrap the exception in an HttpUnhandledException so that ELMAH can capture the original error page.
        //    var exceptionToRaise = new HttpUnhandledException(FormatLogMessageAsync(context, httpContext), context.Exception);

        //    FormatLoggerAccessor.Instance().Error(exceptionToRaise, string.Empty);

        //    // Send the exception to ELMAH (for logging, mailing, filtering, etc.).
        //    ErrorSignal.FromContext(httpContext).Raise(exceptionToRaise, httpContext);
        //}

        private static async Task<string> FormatLogMessageAsync(ExceptionLoggerContext context, HttpContext httpContext)
        {
            // undocumented!
            var webRequest =
                (HttpRequestBase)context.RequestContext.GetType().GetProperty("WebRequest", typeof(HttpRequestBase)).GetValue(context.RequestContext);

            if (webRequest == null)
            {
                ErrorSignal.FromContext(httpContext)
                    .Raise(
                        new InvalidOperationException(
                            "Property 'WebRequest' of ExceptionLoggerContext.RequestContext not found. User host address cannot be determined."));
            }

            var message =
                string.Format(
                    "\r\nRequestUri: {0}\r\nRequest-Content:\r\n{1}\r\nUser host address: {2}\r\nIs local: {3}\r\nMessage: {4}\r\n",
                    context.Request.RequestUri,
                    await ReadContentAsync(context),
                    webRequest != null ? webRequest.UserHostAddress : string.Empty,
                    context.RequestContext.IsLocal,
                    context.ExceptionContext.Exception.Message);
            return message;
        }

        private static async Task<string> ReadContentAsync(ExceptionLoggerContext context)
        {
            var content = string.Empty;
            var stream = await context.Request.Content.ReadAsStreamAsync();

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var sr = new StreamReader(stream))
            {
                content = await sr.ReadToEndAsync();
            }
            return content;
        }

        private static HttpContext GetHttpContext(HttpRequestMessage request)
        {
            if (request == null)
                return null;

            object value;
            if (!request.Properties.TryGetValue("MS_HttpContext", out value))
                return null;

            HttpContextBase context = value as HttpContextBase;
            if (context == null)
                return null;

            return context.ApplicationInstance.Context;
        }
    }
}
