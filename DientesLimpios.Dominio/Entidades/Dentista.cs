using DientesLimpios.Dominio.Comunes;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;

namespace DientesLimpios.Dominio.Entidades
{
    public class Dentista : EntidadAuditable
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; } = null!;
        public Email Email { get; private set; } = null!;

        private Dentista() { } // EF Core

        private Dentista(string nombre, Email email)
        {
            Id = Guid.CreateVersion7();
            Nombre = nombre;
            Email = email;
        }


        public static Result<Dentista> Crear(string nombre, string email)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Failure<Dentista>(DomainErrors.Dentista.NombreObligatorio);

            var emailResult = Email.Crear(email);
            if (emailResult.IsFailure)
                return Result.Failure<Dentista>(emailResult.Error);

            return Result.Success(new Dentista(nombre, emailResult.Value));
        }


        public Result ActualizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Failure(DomainErrors.Dentista.NombreObligatorio);

            Nombre = nombre;
            return Result.Success();
        }

        public Result ActualizarEmail(string email)
        {
            var emailResult = Email.Crear(email);
            if (emailResult.IsFailure)
                return Result.Failure(emailResult.Error);

            Email = emailResult.Value;
            return Result.Success();
        }

    }
}
