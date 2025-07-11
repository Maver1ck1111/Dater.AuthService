using Dater.Auth.Application.Services;
using Dater.Auth.Application.ServicesContracts;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dater.Auth.Application.FluentValidators;
using FluentValidation.AspNetCore;

namespace Dater.Auth.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IJWTService, JWTService>();
            services.AddScoped<IHasherService, HasherService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddValidatorsFromAssemblyContaining<RequestAccountDTOValidator>();
            services.AddFluentValidationAutoValidation();

            return services;
        }
    }
}
