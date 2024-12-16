using Discusseddto.Auth;
using FluentValidation;
using System.Data;

namespace DiscussedApi.Validations.Encryption
{
    public class EncyrptionCredentialsValidation : AbstractValidator<EncyrptionCredentialsDto>
    {
        public EncyrptionCredentialsValidation()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id cant be null");
            RuleFor(x => x.Key).NotNull().WithMessage("Key cant be null");
            RuleFor(x => x.Iv).NotNull().WithMessage("Iv cant be null");
        }
    }
}
