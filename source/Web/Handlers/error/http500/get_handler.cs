using System;
using Dovetail.SDK.Fubu.Actions;

namespace Bootstrap.Web.Handlers.error.http500
{
    public class get_handler
    {
        public Error500Model Execute(Error500Request request)
        {
            return new Error500Model { ErrorMessage = request.Exception != null ? request.Exception.Message : "" };
        }
    }

    public class Error500Request : IServerErrorRequest
    {
        public Exception Exception { get; set; }
    }

    public class Error500Model 
    {
        public string ErrorMessage { get; set; }
    }
}