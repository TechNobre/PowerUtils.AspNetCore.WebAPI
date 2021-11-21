using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using PowerUtils.AspNetCore.WebAPI.ProblemDetailsHandlers;
using PowerUtils.Net.Constants;
using PowerUtils.Validations;
using System.Threading.Tasks;

namespace PowerUtils.AspNetCore.WebAPI.Validations
{
    public class ValidationNotificationsFilter : IAsyncResultFilter
    { // DONE
        private readonly IValidationNotificationsPipeline _validations;

        public ValidationNotificationsFilter(IValidationNotificationsPipeline validationNotificationsPipeline)
        {
            _validations = validationNotificationsPipeline;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (_validations.Invalid)
            {
                context.HttpContext.Response.StatusCode = (int)_validations.StatusCode;
                context.HttpContext.Response.ContentType = ExtendedMediaTypeNames.ProblemApplication.JSON;

                var problemDetailsResponse = ProblemDetailsResponse.Create(
                    context,
                    _validations
                );

                await context.HttpContext.Response.WriteAsync(problemDetailsResponse.ToString());

                return;
            }

            await next();
        }
    }
}