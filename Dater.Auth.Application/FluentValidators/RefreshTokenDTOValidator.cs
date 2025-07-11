using Dater.Auth.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dater.Auth.Application.FluentValidators
{
    public class RefreshTokenDTOValidator : AbstractValidator<RefreshTokenDTO>
    {
        public RefreshTokenDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Token is required");
        }
    }
}
