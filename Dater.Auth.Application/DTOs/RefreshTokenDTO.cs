using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.DTOs
{
    public class RefreshTokenDTO
    {
        public string Email { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
