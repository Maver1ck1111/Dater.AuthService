using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Dater.Auth.Application.RepositoriesContracts;

namespace Dater.Auth.Infrastracture
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            string connectionString = $"Host={Environment.GetEnvironmentVariable("AuthDbHost")}; " +
                $"Port={Environment.GetEnvironmentVariable("AuthDbPort")}; " +
                $"Database={Environment.GetEnvironmentVariable("AuthDbName")}; " +
                $"Username={Environment.GetEnvironmentVariable("AuthDbUsername")};" +
                $"Password={Environment.GetEnvironmentVariable("AuthDbPassword")};";

            services.AddDbContext<AccountDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            services.AddScoped<IAccountRepository, AccountRepository>();

            return services;
        }
    }
}
