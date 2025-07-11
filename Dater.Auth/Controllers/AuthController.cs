using Dater.Auth.Application;
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
            Result<AccountResponseDTO> result = await _authService.Login(requestDTO);

            if(result.StatusCode == 404)
            {
                _logger.LogError("User with email {Email} not found.", requestDTO.Email);
                return NotFound("User not found.");
            }

            if(result.StatusCode == 401)
            {
                _logger.LogError("Incorrect password for user with email {Email}.", requestDTO.Email);
                return Unauthorized("Incorrect password.");
            }

            return result.Value;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AccountResponseDTO>> Register(AccountRequestDTO requestDTO)
        {
            Result<AccountResponseDTO> result = await _authService.Register(requestDTO);

            if(result.StatusCode == 400)
            {
                _logger.LogError("Invalid registration request for email {Email}.", requestDTO.Email);
                return BadRequest("Invalid registration request.");
            }

            return result.Value;
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AccountResponseDTO>> Refresh(RefreshTokenDTO refreshToken)
        {
            Result<AccountResponseDTO> result = await _authService.RefreshTokens(refreshToken.Email, refreshToken.RefreshToken);

            if(result.StatusCode == 404)
            {
                _logger.LogError("User with email {Email} not found for token refresh.", refreshToken.Email);
                return NotFound("User not found.");
            }

            if(result.StatusCode == 401)
            {
                _logger.LogError("Invalid or expired refresh token for user with email {Email}.", refreshToken.Email);
                return Unauthorized("Incorrect refresh token or expire time.");
            }

            return result.Value;
        }
    }
}
