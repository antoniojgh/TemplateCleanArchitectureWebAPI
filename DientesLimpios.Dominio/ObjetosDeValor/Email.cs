using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;

namespace DientesLimpios.Dominio.ObjetosDeValor
{
    public record Email
    {
        public string Valor { get; } = null!;

        private Email() { }   // EF Core

        private Email(string valor) => Valor = valor;

        public static Result<Email> Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Result.Failure<Email>(DomainErrors.Email.Vacio);

            if (!valor.Contains('@', StringComparison.Ordinal))
                return Result.Failure<Email>(DomainErrors.Email.FormatoInvalido);

            return Result.Success(new Email(valor));
        }
    }

}
