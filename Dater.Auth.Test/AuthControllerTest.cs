using Dater.Auth.Application;
using Dater.Auth.Application.DTOs;
using Dater.Auth.Application.ServicesContracts;
using Dater.Auth.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            result.Should().BeOfType<OkObjectResult>();

            result.Value.Should().NotBeNull();

            result.Value.Email.Should().Be(requestDTO.Email);
            result.Value.AccessToken.Should().NotBeNullOrEmpty();
            result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldReturnNotFoundError_UserNotFound()
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

            result.Should().BeOfType<NotFoundObjectResult>();
            result.Value.Should().Be("User not found");
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

            result.Should().BeOfType<UnauthorizedObjectResult>();
            result.Value.Should().Be("Incorrect password");
        }

        [Fact]
        public async Task Register_ShouldReturnCorrectResponse()
        {

        }
    }
}
