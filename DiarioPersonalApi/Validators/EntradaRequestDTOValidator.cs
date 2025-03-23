using DiarioPersonalApi.Models;
using FluentValidation;

namespace DiarioPersonalApi.Validators
{
    public class EntradaRequestDTOValidator: AbstractValidator<EntradaRequestDTO>
    {
        public EntradaRequestDTOValidator()
        {
            RuleFor(e => e.Contenido)
                .NotEmpty().WithMessage("El texto no puede estar vacío.")
                .MaximumLength(5000).WithMessage("El texto no puede superar los 5000 caracteres.");

            RuleFor(e => e.Fecha)
                .NotEmpty().WithMessage("La fecha es obligatoria.");
        }
    }
}
