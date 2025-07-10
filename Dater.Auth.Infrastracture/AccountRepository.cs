using Dater.Auth.Application;
using Dater.Auth.Application.RepositoriesContracts;
using Dater.Auth.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Infrastracture
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDbContext _context;
        private readonly ILogger<AccountRepository> _logger;
        public AccountRepository(AccountDbContext context, ILogger<AccountRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Result<Guid>> AddAsync(Account account)
        {
            if(account == null)
            {
               _logger.LogError("Attempted to add a null account.");
               return Result<Guid>.OnError(400, "Account cannot be null");
            }

            account.AccountID = Guid.NewGuid();

            await _context.Accounts.AddAsync(account);

            int affectedRows = await _context.SaveChangesAsync();

            if (affectedRows > 0)
            {
                _logger.LogInformation("Account with ID {AccountId} and email {Email} added successfully.", account.AccountID, account.Email);
                return Result<Guid>.OnSuccess(account.AccountID, 201);
            }

            _logger.LogError("Failed to add account with email {Email}. No rows affected.", account.Email);
            return Result<Guid>.OnError(500, "Error while adding to table");
        }

        public async Task<Result<bool>> DeleteAsync(Guid accountID)
        {
            if (accountID == Guid.Empty) 
            {
                _logger.LogError("Attempted to delete an account with an empty ID.");
                return Result<bool>.OnError(400, "Account ID cannot be empty");
            }

            Account? account = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountID == accountID);

            if(account == null)
            {
                _logger.LogError("Account with ID {AccountId} not found.", accountID);
                return Result<bool>.OnError(404, "Account not found");
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Account with ID {AccountId} deleted successfully.", accountID);
            return Result<bool>.OnSuccess(true);
        }

        public async Task<Result<Account>> GetByEmailAsync(string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                _logger.LogError("Attemted to find account with empty email");
                return Result<Account>.OnError(400, "Email cannot be empty");
            }

            Account? account = await _context.Accounts.FirstOrDefaultAsync(x => x.Email == email);

            if(account == null)
            {
                _logger.LogError("Account with email {Email} not found.", email);
                return Result<Account>.OnError(404, "Account not found");
            }

            _logger.LogInformation("Account with email {Email} found.", email);
            return Result<Account>.OnSuccess(account);
        }

        public async Task<Result<bool>> UpdateAsync(Account account)
        {
            if(account == null)
            {
                _logger.LogError("Attemted to update an empty account");
                return Result<bool>.OnError(400, "Account cannot be null");
            }

            if (account.AccountID == Guid.Empty)
            {
                _logger.LogError("Attempted to update an account with an empty ID.");
                return Result<bool>.OnError(400, "Account ID cannot be empty");
            }

            Account? existingAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountID == account.AccountID);

            if (existingAccount == null)
            {
                _logger.LogError("Account with ID {AccountId} not found for update.", account.AccountID);
                return Result<bool>.OnError(404, "Account not found");
            }

            _context.Accounts.Update(existingAccount);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Account with ID {AccountId} updated successfully.", account.AccountID);
            return Result<bool>.OnSuccess(true);
        }
    }
}
