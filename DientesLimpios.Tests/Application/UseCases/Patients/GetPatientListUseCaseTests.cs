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
        private readonly IPatientRepository _repository;
        private readonly ILogger<GetPatientListHandler> _logger;
        private readonly GetPatientListHandler _handler;

        public GetPatientListUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _repository = Substitute.For<IPatientRepository>();
            _logger = Substitute.For<ILogger<GetPatientListHandler>>();

            _handler = new GetPatientListHandler(_repository, _logger);
        }

        [Fact]
        public async Task Handle_PatientsExist_ReturnsPagedDTOsCorrectly()
        {
            // Arrange
            const int total = 7;   // deliberately != dentists.Count
            var page = 1;
            var pageSize = 2;

            var patient1 = Patient.Create("Felipe", "felipe@ejemplo.com").Value;
            var patient2 = Patient.Create("Claudia", "claudia@ejemplo.com").Value;

            var patients = new List<Patient> { patient1, patient2 };

            _repository.GetFiltered(Arg.Any<PatientFilterDTO>(), Arg.Any<CancellationToken>()).Returns((patients, total));

            var request = new GetPatientListQuery
            {
                Page = page,
                RecordsPerPage = pageSize
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(total);
            result.Value.Elements.Count.Should().Be(2);
            result.Value.Elements[0].Name.Should().Be("Felipe");
            result.Value.Elements[0].Email.Should().Be("felipe@ejemplo.com");
            result.Value.Elements[1].Name.Should().Be("Claudia");
            result.Value.Elements[1].Email.Should().Be("claudia@ejemplo.com");
            await _repository.Received(1).GetFiltered(Arg.Any<PatientFilterDTO>(), Arg.Any<CancellationToken>());

        }

        [Fact]
        public async Task Handle_NoPatientsExist_ReturnsEmptyListAndTotalZero()
        {
            // Arrange
            var page = 1;
            var pageSize = 5;

            var filterDto = new PatientFilterDTO { Page = page, RecordsPerPage = pageSize };

            IEnumerable<Patient> patients = new List<Patient>();

            _repository.GetFiltered(Arg.Any<PatientFilterDTO>(), Arg.Any<CancellationToken>()).Returns((patients, patients.Count()));

            var dbSet = new List<Patient>().BuildMockDbSet();
            _db.Patients.Returns(dbSet);

            var request = new GetPatientListQuery
            {
                Page = page,
                RecordsPerPage = pageSize
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(0);
            result.Value.Elements.Count.Should().Be(0);
        }

    }
}
