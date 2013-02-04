using System;
using System.Net;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Fubu.Configuration;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;

namespace Dovetail.SDK.Fubu.Actions
{
	/// <summary>
    /// The view you create to display friendly server error pages needs to implement this interface.
    /// </summary>
    public interface IServerErrorRequest
    {
        Exception Exception { get; set; }
    }

    /// <summary>
    /// Action wrapper which catches exceptions thrown by the wrapped behavior and transfers the output to a 
    /// </summary>
    /// <typeparam name="T">Type of the input model of the view which will be rendered after an exception.</typeparam>
    public class ActionExceptionWrapper<T> : IActionBehavior
        where T : class, IServerErrorRequest,  new()
    {
        private readonly IFubuRequest _request;
	    private readonly IFubuPartialService _fubuPartialService;
	    private readonly IOutputWriter _writer;
	    private readonly ILogger _logger;

		public ActionExceptionWrapper(IFubuRequest request, IFubuPartialService fubuPartialService, IOutputWriter writer, ILogger logger)
        {
            _request = request;
			_fubuPartialService = fubuPartialService;
			_writer = writer;
			_logger = logger;
        }

        public IActionBehavior InsideBehavior { get; set; }

        public void Invoke()
        {
            exceptionHandledBehavior(b => b.Invoke());
        }

        public void InvokePartial()
        {
            exceptionHandledBehavior(b => b.InvokePartial());
        }

        public void exceptionHandledBehavior(Action<IActionBehavior> behaviorAction)
        {
            if (InsideBehavior == null) return;

            try
            {
                behaviorAction(InsideBehavior);
            }
            catch (Exception exception)
            {
                _logger.LogError("An API action threw an exception.", exception);

                if (!AspNetSettings.IsCustomErrorsEnabled)
                    throw;

                var request = new T {Exception = exception};
                _request.Set(request);

	            _fubuPartialService.Invoke(typeof (T));

                _writer.WriteResponseCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}