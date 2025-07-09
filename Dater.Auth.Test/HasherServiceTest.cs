using Castle.Core.Logging;
using Dater.Auth.Application.Services;
using Dater.Auth.Application.ServicesContracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Test
{
    public class HasherServiceTest
    {
        private readonly Mock<ILogger<HasherService>> _loggerMock = new Mock<ILogger<HasherService>>();

        [Fact]
        public void HashPassword_ShouldReturnHashedPassword()
        {
            string hashedPassword = new HasherService(_loggerMock.Object).HashPassword("myPassword1234");

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().HaveLength(60);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
        {
            string password = "myPassword1234";
            string hashedPassword = new HasherService(_loggerMock.Object).HashPassword(password);

            bool isVerified = new HasherService(_loggerMock.Object).VerifyPassword(password, hashedPassword);

            isVerified.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalseForCorrectPassword()
        {
            string password = "myPassword1234";
            string hashedPassword = new HasherService(_loggerMock.Object).HashPassword(password);

            bool isVerified = new HasherService(_loggerMock.Object).VerifyPassword("anotherPassword", hashedPassword);

            isVerified.Should().BeFalse();
        }
    }
}
