using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList
{
    public class DentistListDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
