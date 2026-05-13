using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Patients
{
    public class GetPatientListUseCaseTests
    {
        private readonly IPatientRepository _repositorio;
        private readonly ILogger<GetPatientListHandler> _logger;
        private readonly GetPatientListHandler _handler;

        public GetPatientListUseCaseTests()
        {
            _repositorio = Substitute.For<IPatientRepository>();
            _logger = Substitute.For<ILogger<GetPatientListHandler>>();

            _handler = new GetPatientListHandler(_repositorio, _logger);
        }

        [Fact]
        public async Task Handle_CuandoHayPatients_RetornaPagedConDTOsCorrectos()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 2;

            var paciente1 = Patient.Create("Felipe", "felipe@ejemplo.com").Value;
            var paciente2 = Patient.Create("Claudia", "claudia@ejemplo.com").Value;

            var patients = new List<Patient> { paciente1, paciente2 };

            _repositorio.GetFiltered(Arg.Any<PatientFilterDTO>()).Returns(patients);

            _repositorio.GetTotalRecordCount().Returns(10);

            var request = new GetPatientListQuery
            {
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(10);
            result.Value.Elementos.Count.Should().Be(2);
            result.Value.Elementos[0].Name.Should().Be("Felipe");
            result.Value.Elementos[0].Email.Should().Be("felipe@ejemplo.com");
            result.Value.Elementos[1].Name.Should().Be("Claudia");
            result.Value.Elementos[1].Email.Should().Be("claudia@ejemplo.com");
            await _repositorio.Received(1).GetFiltered(Arg.Any<PatientFilterDTO>());
            await _repositorio.Received(1).GetTotalRecordCount();

        }

        [Fact]
        public async Task Handle_CuandoNoHayPatients_RetornaListaVaciaYTotalCero()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 5;

            var patientFilterDTO = new PatientFilterDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            IEnumerable<Patient> patients = new List<Patient>();

            _repositorio.GetFiltered(Arg.Any<PatientFilterDTO>()).Returns(patients);

            _repositorio.GetTotalRecordCount().Returns(0);

            var request = new GetPatientListQuery
            {
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(0);
            result.Value.Elementos.Count.Should().Be(0);
        }

    }
}
