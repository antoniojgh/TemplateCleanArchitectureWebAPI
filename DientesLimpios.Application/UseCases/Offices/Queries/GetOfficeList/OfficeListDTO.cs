using System;
using System.Collections.Generic;
using System.Text;

namespace DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeList
{
    public class OfficeListDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }
}
