using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using Structura.SharedComponents.Utilities;

namespace Structura.SharedComponents.WebUtilities
{
    public interface IReportController
    {
        HttpResponseBase Response { get; }
        HttpRequestBase Request { get; }
        ModelStateDictionary ModelState { get; }
        ControllerContext ControllerContext { get; }
        void AddTempMessage(bool success, string message);
        string CurrentUserFullName { get; }
        IFormatLogger FormatLogger { get; }
    }
}
