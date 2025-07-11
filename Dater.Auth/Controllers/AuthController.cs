using Dater.Auth.Application.DTOs;
using Dater.Auth.Application.ServicesContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dater.Auth.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AccountResponseDTO>> Login(AccountRequestDTO requestDTO)
        {
            return Ok();
        }

        [HttpPost("register")]
        public async Task<ActionResult<AccountResponseDTO>> Register(AccountRequestDTO requestDTO)
        {
            return Ok();
        }
    }
}
