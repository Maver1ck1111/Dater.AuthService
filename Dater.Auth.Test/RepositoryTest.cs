using Castle.Core.Logging;
using Dater.Auth.Application;
using Dater.Auth.Domain;
using Dater.Auth.Infrastracture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Test
{
    public class RepositoryTest
    {
        private readonly Mock<ILogger<AccountRepository>> _mockLogger = new Mock<ILogger<AccountRepository>>();

        private AccountDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AccountDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddAccount()
        {
            var repository = new AccountRepository(GetInMemoryDb(), _mockLogger.Object);

            Account account = new Account()
            {
                Email = "example@mail.com",
                HashedPassword = "hashedPassword",
                RefreshToken = "refreshToken",
            };

            Result<Guid> addResult = await repository.AddAsync(account);

            addResult.StatusCode.Should().Be(200);
            addResult.Value.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAccount()
        {
            var repository = new AccountRepository(GetInMemoryDb(), _mockLogger.Object);

            Account account = new Account()
            {
                Email = "example@mail.com",
                HashedPassword = "hashedPassword",
                RefreshToken = "refreshToken",
            };

            await repository.AddAsync(account);

            account.Email = "anotherEmail@mail.com";

            Result<bool> updateResult = await repository.UpdateAsync(account);

            updateResult.StatusCode.Should().Be(200);
            updateResult.Value.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturn404_WhenAccountNotFound()
        {
            var repository = new AccountRepository(GetInMemoryDb(), _mockLogger.Object);

            Account account = new Account()
            {
                AccountID = Guid.NewGuid(),
                Email = "example@mail.com",
                HashedPassword = "hashedPassword",
                RefreshToken = "refreshToken",
            };

            Result<bool> updateResult = await repository.UpdateAsync(account);

            updateResult.StatusCode.Should().Be(404);
            updateResult.Value.Should().BeFalse();
        }


        [Fact]
        public async Task DeleteAsync_ShouldDeleteAccount()
        {
            var repository = new AccountRepository(GetInMemoryDb(), _mockLogger.Object);

            Account account = new Account()
            {
                Email = "example@mail.com",
                HashedPassword = "hashedPassword",
                RefreshToken = "refreshToken",
            };

            Result<Guid> addResult = await repository.AddAsync(account);

            Result<bool> updateResult = await repository.DeleteAsync(addResult.Value);

            updateResult.StatusCode.Should().Be(200);
            updateResult.Value.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn404_WhenAccountNotFound()
        {
            var repository = new AccountRepository(GetInMemoryDb(), _mockLogger.Object);

            Result<bool> deleteResult = await repository.DeleteAsync(Guid.NewGuid());

            deleteResult.StatusCode.Should().Be(404);
            deleteResult.Value.Should().BeFalse();
        }

        [Fact]
        public async Task GetAccountByEmail_ShouldNotBeNull()
        {
            var repository = new AccountRepository(GetInMemoryDb(), _mockLogger.Object);

            Account account = new Account()
            {
                Email = "example@mail.com",
                HashedPassword = "hashedPassword",
                RefreshToken = "refreshToken",
            };

            await repository.AddAsync(account);

            Result<Account> getResult = await repository.GetByEmailAsync(account.Email);

            getResult.StatusCode.Should().Be(200);
            getResult.Value?.Email.Should().Be("example@mail.com");
        }

        [Fact]
        public async Task GetAccountByEmail_ShouldBeNull_WhenAccountNotFound()
        {
            var repository = new AccountRepository(GetInMemoryDb(), _mockLogger.Object);

            Result<Account> getResult = await repository.GetByEmailAsync("someEmail@mail.com");

            getResult.StatusCode.Should().Be(404);
            getResult.Value.Should().BeNull();
        }

    }
}
