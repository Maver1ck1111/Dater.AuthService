using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.ServicesContracts
{
    public interface IJWTService
    {
        string GenerateToken(string email);
        string GenerateRefreshToken();
    }
}
