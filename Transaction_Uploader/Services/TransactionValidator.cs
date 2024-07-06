using FluentValidation;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Services
{
    public class TransactionValidator : AbstractValidator<Transaction>
    {
        public TransactionValidator()
        {
            RuleFor(x => x.TransactionId).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
            RuleFor(x => x.TransactionDate).NotEmpty();
            RuleFor(x => x.Status).NotEmpty().Must(BeAValidStatus).WithMessage("Status must be A, R, or D");
        }

        private bool BeAValidStatus(string status)
        {
            return status == "A" || status == "R" || status == "D";
        }
    }
}
