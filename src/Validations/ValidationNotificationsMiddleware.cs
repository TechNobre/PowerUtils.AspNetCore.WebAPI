using Microsoft.Extensions.DependencyInjection;
using PowerUtils.Validations;

namespace PowerUtils.AspNetCore.WebAPI.Validations
{
    public static class ValidationNotificationsMiddleware
    {
        public static IServiceCollection AddValidationNotifications(this IServiceCollection services)
        { // DONE
            services.AddScoped<IValidationNotificationsPipeline, ValidationNotificationsPipeline>();
            services.AddMvc(options => options.Filters.Add<ValidationNotificationsFilter>());

            return services;
        }
    }
}