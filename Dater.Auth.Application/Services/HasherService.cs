using Dater.Auth.Application.ServicesContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.Services
{
    public class HasherService : IHasherService
    {
        private readonly ILogger<HasherService> _logger;
        public HasherService(ILogger<HasherService> logger)
        {
            _logger = logger;
        }
        public string HashPassword(string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            
            _logger.LogInformation("Password hashed successfully.");

            return hashedPassword;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            bool verify = BCrypt.Net.BCrypt.Verify(password, hashedPassword);

            if(verify)
            {
                _logger.LogInformation("Password verification successful.");
            }
            else
            {
                _logger.LogWarning("Password verification failed.");
            }

            return verify;
        }
    }
}
