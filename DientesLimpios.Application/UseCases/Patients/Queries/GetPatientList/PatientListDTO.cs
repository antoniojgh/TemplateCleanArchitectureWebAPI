using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public class PatientListDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
