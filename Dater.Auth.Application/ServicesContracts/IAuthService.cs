using Dater.Auth.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.ServicesContracts
{
    public interface IAuthService
    {
        Task<Result<AccountResponseDTO>> Login(AccountRequestDTO accountRequestDTO);
        Task<Result<AccountResponseDTO>> Register(AccountRequestDTO accountRequestDTO);
        Task<Result<AccountResponseDTO>> RefreshTokens(string email, string refreshToken);
    }
}
