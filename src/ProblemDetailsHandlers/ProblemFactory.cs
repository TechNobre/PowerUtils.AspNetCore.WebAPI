using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using PowerUtils.Net.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers
{
    public class ProblemFactory : ProblemDetailsFactory
    {
        #region CONTAINS
        private static readonly ActionDescriptor EMPTY_ACTION_DESCRIPTOR = new();
        private static readonly RouteData EMPTY_ROUTE_DATA = new();

        private static readonly HashSet<string> AllowedHeaderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            HeaderNames.AccessControlAllowCredentials,
            HeaderNames.AccessControlAllowHeaders,
            HeaderNames.AccessControlAllowMethods,
            HeaderNames.AccessControlAllowOrigin,
            HeaderNames.AccessControlExposeHeaders,
            HeaderNames.AccessControlMaxAge,

            HeaderNames.StrictTransportSecurity,

            HeaderNames.WWWAuthenticate,
        };
        #endregion


        #region PRIVATE PROPERTIES
        private IActionResultExecutor<ObjectResult> _executor { get; }
        #endregion


        #region CONSTRUCTOR
        public ProblemFactory(IActionResultExecutor<ObjectResult> executor)
        {
            _executor = executor ?? throw new ArgumentNullException($"{typeof(ProblemFactory).Namespace} > {nameof(IActionResultExecutor<ObjectResult>)}");
        }
        #endregion


        #region OVERRIDE PROBLEM DETAILS FACTORY
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null
        )
        {
            int status = statusCode ?? httpContext.Response.StatusCode;

            // It feels weird to mutate the response inside this method, but it's the
            // only way to pass the status code to MapStatusCode and it will be set
            // on the response when writing the problem details response later anyway.
            httpContext.Response.StatusCode = status;

            ProblemDetails result = ProblemDetailsResponse
                .Create(httpContext)
                .ToBaseProblemDetails();

            _setProblemDetailsDefault(
                result,
                status,
                title,
                type,
                detail,
                instance
            );

            return result;
        }

        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null
        )
        {
            ValidationProblemDetails result = new ValidationProblemDetails(modelStateDictionary);

            _setProblemDetailsDefault(
                result,
                statusCode ?? StatusCodes.Status422UnprocessableEntity,
                title,
                type,
                detail,
                instance
            );

            return result;
        }
        #endregion


        #region PUBLIC STATIC METHOD
        public void ClearResponse(HttpContext context, int statusCode)
        { // DONE
            // Make sure problem responses are never cached.
            HeaderDictionary headers = new HeaderDictionary();
            headers.Append(HeaderNames.CacheControl, "no-cache, no-store, must-revalidate");
            headers.Append(HeaderNames.Pragma, "no-cache");
            headers.Append(HeaderNames.Expires, "0");

            foreach (KeyValuePair<string, StringValues> header in context.Response.Headers)
            {
                // Because the CORS middleware adds all the headers early in the pipeline,
                // we want to copy over the existing Access-Control-* headers after resetting the response.
                if (AllowedHeaderNames.Contains(header.Key))
                {
                    headers.Add(header);
                }
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;

            foreach (KeyValuePair<string, StringValues> header in headers)
            {
                context.Response.Headers.Add(header);
            }
        }

        public async Task WriteProblemDetails(HttpContext context, ProblemDetailsResponse problemDetails)
        {
            RouteData routeData = context.GetRouteData() ?? EMPTY_ROUTE_DATA;

            ActionContext actionContext = new ActionContext(context, routeData, EMPTY_ACTION_DESCRIPTOR);

            ObjectResult result = new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes = new MediaTypeCollection()
                {
                   ExtendedMediaTypeNames.ProblemApplication.JSON
                },
            };

            await _executor.ExecuteAsync(actionContext, result);

            await context.Response.CompleteAsync();
        }
        #endregion


        #region PRIVATE METHODS
        private static void _setProblemDetailsDefault(
            ProblemDetails result,
            int statusCode,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            result.Status = statusCode;
            result.Title = title ?? result.Title;
            result.Type = type ?? result.Type ?? result.Status.GetStatusCodeLink();
            result.Detail = detail ?? result.Detail;
            result.Instance = instance ?? result.Instance;
        }
        #endregion
    }
}