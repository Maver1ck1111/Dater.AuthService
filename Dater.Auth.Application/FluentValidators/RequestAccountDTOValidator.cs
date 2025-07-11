using Dater.Auth.Application.DTOs;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.FluentValidators
{
    public class RequestAccountDTOValidator : AbstractValidator<AccountRequestDTO>
    {
        public RequestAccountDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .Must(password => password.Count(char.IsDigit) >= 3)
                .WithMessage("Password must contain at least 3 digits.");
        }
    }
}
