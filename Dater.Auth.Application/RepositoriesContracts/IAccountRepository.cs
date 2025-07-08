using Dater.Auth.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.RepositoriesContracts
{
    public interface IAccountRepository
    {
        Task<Result<Guid>> AddAsync(Account account);
        Task<Result<bool>> UpdateAsync(Account account);
        Task<Result<Account>> GetByEmailAsync(string email);
        Task<Result<bool>> DeleteAsync(Guid accountId);
    }
}
