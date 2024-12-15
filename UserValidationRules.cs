using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace WebServer
{
    public class UserValidationRules:AbstractValidator<UserLoginModel>
    {
        public UserValidationRules() 
        {
            RuleFor(user=>user.Login)
                .NotEmpty()
                .Matches("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")
                .WithMessage("Invalid email format");
            RuleFor(user => user.Password)
                .NotEmpty()
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("Password must be between 8 and 20 characters, at least one digit, special symbol, and upper case letter.");
        }
    }
}
