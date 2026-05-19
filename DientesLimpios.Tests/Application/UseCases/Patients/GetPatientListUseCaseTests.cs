using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList;
using DientesLimpios.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Patients
{
    public class GetPatientListUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly IPatientRepository _repositorio;
        private readonly ILogger<GetPatientListHandler> _logger;
        private readonly GetPatientListHandler _handler;

        public GetPatientListUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _repositorio = Substitute.For<IPatientRepository>();
            _logger = Substitute.For<ILogger<GetPatientListHandler>>();

            _handler = new GetPatientListHandler(_repositorio, _db, _logger);
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

            _repositorio.GetFiltered(Arg.Any<PatientFilterDTO>(), Arg.Any<CancellationToken>()).Returns(patients);

            var allPatients = Enumerable.Range(0, 10).Select(i => Patient.Create($"Name{i}", $"email{i}@test.com").Value).ToList();
            var dbSet = allPatients.BuildMockDbSet();
            _db.Patients.Returns(dbSet);

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
            await _repositorio.Received(1).GetFiltered(Arg.Any<PatientFilterDTO>(), Arg.Any<CancellationToken>());

        }

        [Fact]
        public async Task Handle_CuandoNoHayPatients_RetornaListaVaciaYTotalCero()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 5;

            var patientFilterDTO = new PatientFilterDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            IEnumerable<Patient> patients = new List<Patient>();

            _repositorio.GetFiltered(Arg.Any<PatientFilterDTO>(), Arg.Any<CancellationToken>()).Returns(patients);

            var dbSet = new List<Patient>().BuildMockDbSet();
            _db.Patients.Returns(dbSet);

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
