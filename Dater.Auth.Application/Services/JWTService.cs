using Dater.Auth.Application.ServicesContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.Services
{
    public class JWTService : IJWTService
    {
        private readonly IConfiguration
        public JWTService()
        {

        }
        public string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }

        public string GenerateToken(string email)
        {
            throw new NotImplementedException();
        }
    }
}
