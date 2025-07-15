using Dater.Auth.Application;
using Dater.Auth.Application.DTOs;
using Dater.Auth.Application.ServicesContracts;
using Dater.Auth.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dater.Auth.Test
{
    public class AuthControllerTest
    {
        private readonly Mock<ILogger<AuthController>> _logger = new Mock<ILogger<AuthController>>();
        private readonly Mock<IAuthService> _authService = new Mock<IAuthService>();

        [Fact]
        public async Task Login_ShouldReturnCorrectResponse() 
        {
            AccountRequestDTO requestDTO = new AccountRequestDTO() 
            {
                Email = "someEmail@mail.com",
                Password = "password1234"
            };

            _authService
                .Setup(x => x.Login(It.IsAny<AccountRequestDTO>()))
                .ReturnsAsync(Result<AccountResponseDTO>.OnSuccess(new AccountResponseDTO
                {
                    AccessToken = "jwtToken",
                    RefreshToken = "refreshToken",
                    Email = requestDTO.Email
                }));

            AuthController authController = new AuthController(_authService.Object, _logger.Object);

            var result = await authController.Login(requestDTO);

            var value = result.Value as AccountResponseDTO;

            value.Should().NotBeNull();

            value.Email.Should().Be(requestDTO.Email);
            value.AccessToken.Should().NotBeNullOrEmpty();
            value.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldReturn404NotFoundError_UserNotFound()
        {
            AccountRequestDTO requestDTO = new AccountRequestDTO()
            {
                Email = "someEmail@mail.com",
                Password = "password1234"
            };

            _authService
                .Setup(x => x.Login(It.IsAny<AccountRequestDTO>()))
                .ReturnsAsync(Result<AccountResponseDTO>.OnError(404, "User not found"));

            AuthController authController = new AuthController(_authService.Object, _logger.Object);

            var result = await authController.Login(requestDTO);

            result.Result.Should().BeOfType<NotFoundObjectResult>();

            var notFoundResult = result.Result as NotFoundObjectResult;

            notFoundResult.Value.Should().Be("User not found.");
        }

        [Fact]
        public async Task Login_ShouldReturn401Unauthorized_IncorrectPassword()
        {
            AccountRequestDTO requestDTO = new AccountRequestDTO()
            {
                Email = "someEmail@mail.com",
                Password = "password1234"
            };

            _authService
                .Setup(x => x.Login(It.IsAny<AccountRequestDTO>()))
                .ReturnsAsync(Result<AccountResponseDTO>.OnError(401, "Incorrect password"));
            
            AuthController authController = new AuthController(_authService.Object, _logger.Object);
            var result = await authController.Login(requestDTO);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();

            var unauthorizedResult = result.Result as UnauthorizedObjectResult;

            unauthorizedResult.Value.Should().Be("Incorrect password.");
        }

        [Fact]
        public async Task Register_ShouldReturnCorrectResponse()
        {
            AccountRequestDTO requestDTO = new AccountRequestDTO()
            {
                Email = "someEmail@mail.com",
                Password = "password1234"
            };

            _authService
                .Setup(x => x.Register(It.IsAny<AccountRequestDTO>()))
                .ReturnsAsync(Result<AccountResponseDTO>.OnSuccess(new AccountResponseDTO
                {
                    Email = requestDTO.Email,
                    AccessToken = "jwtToken",
                    RefreshToken = "refreshToken"
                }));

            AuthController authController = new AuthController(_authService.Object, _logger.Object);

            var result = await authController.Register(requestDTO);
            
            var value = result.Value as AccountResponseDTO;

            value.Should().NotBeNull();
            value.Email.Should().Be(requestDTO.Email);
            value.AccessToken.Should().NotBeNullOrEmpty();
            value.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_ShouldReturn409Error_UserAlreadyExist()
        {
            AccountRequestDTO requestDTO = new AccountRequestDTO()
            {
                Email = "someEmail@mail.com",
                Password = "password1234"
            };

            _authService
                .Setup(x => x.Register(It.IsAny<AccountRequestDTO>()))
                .ReturnsAsync(Result<AccountResponseDTO>.OnError(409, "User already exist"));

            AuthController authController = new AuthController(_authService.Object, _logger.Object);

            var result = await authController.Register(requestDTO);

            result.Result.Should().BeOfType<ConflictObjectResult>();

            var value = (result.Result as ConflictObjectResult)?.Value;

            value.Should().NotBeNull();
            value.Should().Be("User with this email already exists.");
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnCorrectResponse()
        {
            RefreshTokenDTO refreshTokenDTO = new RefreshTokenDTO
            {
                Email = "someEmail@mail.com",
                RefreshToken = "refreshToken1234"
            };

            _authService
                .Setup(x => x.RefreshTokens(refreshTokenDTO.Email, refreshTokenDTO.RefreshToken))
                .ReturnsAsync(Result<AccountResponseDTO>.OnSuccess(new AccountResponseDTO
                {
                    Email = refreshTokenDTO.Email,
                    AccessToken = "newJwtToken",
                    RefreshToken = "newRefreshToken"
                }));

            AuthController authController = new AuthController(_authService.Object, _logger.Object);

            var result = await authController.Refresh(refreshTokenDTO);

            var value = result.Value as AccountResponseDTO;

            value.Should().NotBeNull();
            value.Email.Should().Be(refreshTokenDTO.Email);
            value.AccessToken.Should().NotBeNullOrEmpty();
            value.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RefreshToken_ShouldReturn401Unathorized_InvalidRefreshToken()
        {
            RefreshTokenDTO refreshTokenDTO = new RefreshTokenDTO
            {
                Email = "someEmail@mail.com",
                RefreshToken = "invalidToken1234"
            };

            _authService
               .Setup(x => x.RefreshTokens(refreshTokenDTO.Email, refreshTokenDTO.RefreshToken))
               .ReturnsAsync(Result<AccountResponseDTO>.OnError(401, "Refresh token isnt match"));

            AuthController authController = new AuthController(_authService.Object, _logger.Object);

            var result = await authController.Refresh(refreshTokenDTO);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();

            var value = result.Result as UnauthorizedObjectResult;

            value.Value.Should().Be("Incorrect refresh token or expire time.");
        }
    }
}
