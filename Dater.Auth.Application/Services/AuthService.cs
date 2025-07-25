﻿using Dater.Auth.Application.DTOs;
using Dater.Auth.Application.RepositoriesContracts;
using Dater.Auth.Application.ServicesContracts;
using Dater.Auth.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHasherService _hasherService;
        private readonly IJWTService _jwtService;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IHasherService hasherService, IJWTService jwtService, IAccountRepository accountRepository, ILogger<AuthService> logger)
        {
            _hasherService = hasherService;
            _jwtService = jwtService;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<Result<AccountResponseDTO>> Login(AccountRequestDTO accountRequestDTO)
        {
            if (accountRequestDTO == null)
            {
                _logger.LogError("Login request is null.");
                return Result<AccountResponseDTO>.OnError(400, "Invalid login request.");
            }

            if (string.IsNullOrEmpty(accountRequestDTO.Email) || string.IsNullOrEmpty(accountRequestDTO.Password))
            {
                _logger.LogError("Email or password is empty.");
                return Result<AccountResponseDTO>.OnError(400, "Email and password cannot be empty.");
            }

            Result<Account> resultAccount = await _accountRepository.GetByEmailAsync(accountRequestDTO.Email);

            if (resultAccount.StatusCode == 404) 
            {
                _logger.LogError("Account with email {Email} not found.", accountRequestDTO.Email);
                return Result<AccountResponseDTO>.OnError(404, "Account not found.");
            }

            bool isCorrectPassword = _hasherService.VerifyPassword(accountRequestDTO.Password, resultAccount.Value.HashedPassword);

            if (!isCorrectPassword)
            {
                _logger.LogError("Incorrect password for email {Email}.", accountRequestDTO.Email);
                return Result<AccountResponseDTO>.OnError(401, "Incorrect password.");
            }

            string refreshToken = _jwtService.GenerateRefreshToken();
            string jwtToken = _jwtService.GenerateToken(resultAccount.Value.Email, resultAccount.Value.AccountID);

            Account updatedAccount = resultAccount.Value;
            updatedAccount.RefreshToken = refreshToken;
            updatedAccount.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            Result<bool> updated = await _accountRepository.UpdateAsync(updatedAccount);

            if(updated.StatusCode != 200) 
            {
                _logger.LogError("Failed to update account with email {Email}.", accountRequestDTO.Email);
                return Result<AccountResponseDTO>.OnError(updated.StatusCode, updated.ErrorMessage);
            }

            AccountResponseDTO accountResponseDTO = new AccountResponseDTO
            {
                Email = resultAccount.Value.Email,
                RefreshToken = refreshToken,
                AccessToken = jwtToken
            };

            _logger.LogInformation("User {Email} logged in successfully.", accountRequestDTO.Email);

            return Result<AccountResponseDTO>.OnSuccess(accountResponseDTO);
        }

        public async Task<Result<AccountResponseDTO>> RefreshTokens(string email, string refreshToken)
        {
            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogError("Email or refresh token is empty");
                return Result<AccountResponseDTO>.OnError(400, "Email and refresh token cannot be empty.");
            }

            Result<Account> resultAccount = await _accountRepository.GetByEmailAsync(email);

            if (resultAccount.StatusCode == 404)
            {
                _logger.LogError("Account with email {Email} not found.", email);
                return Result<AccountResponseDTO>.OnError(404, "Account not found.");
            }

            if(resultAccount.Value.RefreshToken != refreshToken)
            {
                _logger.LogError("Invalid refresh token for email {Email}.", email);
                return Result<AccountResponseDTO>.OnError(401, "Invalid refresh token.");
            }

            if(resultAccount.Value.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                _logger.LogError("Refresh token for email {Email} has expired.", email);
                return Result<AccountResponseDTO>.OnError(401, "Refresh token has expired.");
            }

            _logger.LogInformation("Refreshing tokens for user {Email}.", email);

            string newRefreshToken = _jwtService.GenerateRefreshToken();
            string jwtToken = _jwtService.GenerateToken(email, resultAccount.Value.AccountID);

            Account updatedAccount = resultAccount.Value;
            updatedAccount.RefreshToken = refreshToken;
            updatedAccount.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            Result<bool> updated = await _accountRepository.UpdateAsync(updatedAccount);

            if (updated.StatusCode != 200)
            {
                _logger.LogError("Failed to update account with email {Email}.", email);
                return Result<AccountResponseDTO>.OnError(updated.StatusCode, updated.ErrorMessage);
            }

            return Result<AccountResponseDTO>.OnSuccess(new AccountResponseDTO
            {
                Email = email,
                AccessToken = jwtToken,
                RefreshToken = newRefreshToken
            }, 200);
        }

        public async Task<Result<AccountResponseDTO>> Register(AccountRequestDTO accountRequestDTO)
        {
            if (accountRequestDTO == null) 
            {
                _logger.LogError("Registration request is null.");
                return Result<AccountResponseDTO>.OnError(400, "Invalid registration request.");
            }

            if (string.IsNullOrEmpty(accountRequestDTO.Email) || string.IsNullOrEmpty(accountRequestDTO.Password)) 
            {
                _logger.LogError("Email or password is empty.");
                return Result<AccountResponseDTO>.OnError(400, "Email and password cannot be empty.");
            }

            Result<Account> existingAccount = await _accountRepository.GetByEmailAsync(accountRequestDTO.Email);

            if (existingAccount.StatusCode == 200) 
            {
                _logger.LogError("Account with email {Email} already exists.", accountRequestDTO.Email);
                return Result<AccountResponseDTO>.OnError(409, "Account with this email already exists.");
            }

            Account account = new Account
            {
                Email = accountRequestDTO.Email,
                HashedPassword = _hasherService.HashPassword(accountRequestDTO.Password),
                RefreshToken = _jwtService.GenerateRefreshToken(),
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
            };

            Result<Guid> result = await _accountRepository.AddAsync(account);

            if(result.StatusCode != 201) 
            {
                _logger.LogError("Failed to register account with email {Email}.", accountRequestDTO.Email);
                return Result<AccountResponseDTO>.OnError(result.StatusCode, result.ErrorMessage);
            }

            string jwtToken = _jwtService.GenerateToken(account.Email, account.AccountID);

            return Result<AccountResponseDTO>.OnSuccess(new AccountResponseDTO
            {
                Email = account.Email,
                AccessToken = jwtToken,
                RefreshToken = account.RefreshToken
            }, 201);
        }
    }
}
