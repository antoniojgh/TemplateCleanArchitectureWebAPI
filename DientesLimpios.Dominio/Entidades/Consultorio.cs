using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Dominio.Comunes;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using DientesLimpios.Dominio.Excepciones;

namespace DientesLimpios.Dominio.Entidades
{
    public class Consultorio : EntidadAuditable
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; } = null!;

        private Consultorio() { }   // EF Core

        private Consultorio(string nombre)
        {
            Id = Guid.CreateVersion7();
            Nombre = nombre;
        }

        public static Result<Consultorio> Crear(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Failure<Consultorio>(DomainErrors.Consultorio.NombreObligatorio);

            return Result.Success(new Consultorio(nombre));
        }

        public Result ActualizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Failure(DomainErrors.Consultorio.NombreObligatorio);

            Nombre = nombre;
            return Result.Success();
        }

    }
}
