using DiarioPersonalApi.Models;
using FluentValidation;

namespace DiarioPersonalApi.Validators
{
    public class RegisterRequestDTOValidator : AbstractValidator<RegisterRequestDTO> 
    {
        public RegisterRequestDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress().WithMessage("Debe proporcionar un email válido.");

            RuleFor(x => x.Contraseña)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");

            // NombreUsuario opcional: Si se quiere limitar.
            RuleFor(x => x.NombreUsuario)
                .MaximumLength(50).WithMessage("El nombre de usuario no puede ser tan largo.");
        }

    }
}
