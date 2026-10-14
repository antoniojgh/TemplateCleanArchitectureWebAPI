using DientesLimpios.Application.Interfaces.Persistence;
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
        private readonly ILogger<GetPatientListHandler> _logger;
        private readonly GetPatientListHandler _handler;

        public GetPatientListUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<GetPatientListHandler>>();

            _handler = new GetPatientListHandler(_db, _logger);
        }

        [Fact]
        public async Task Handle_FirstPage_ReturnsPageOrderedByNameAndTotalOfAllPatients()
        {
            // Arrange
            var patients = new List<Patient>
            {
                Patient.Create("Felipe", "felipe@example.com").Value,
                Patient.Create("Claudia", "claudia@example.com").Value,
                Patient.Create("Mario", "mario@example.com").Value
            };

            var dbSet = patients.BuildMockDbSet();
            _db.Patients.Returns(dbSet);

            var request = new GetPatientListQuery { Page = 1, RecordsPerPage = 2 };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(3);
            result.Value.Items.Select(p => p.Name).Should().Equal("Claudia", "Felipe");
            result.Value.Items[0].Email.Should().Be("claudia@example.com");
        }

        [Fact]
        public async Task Handle_LastPage_ReturnsRemainingPatientsAndTotalOfAllPatients()
        {
            // Arrange
            var patients = new List<Patient>
            {
                Patient.Create("Felipe", "felipe@example.com").Value,
                Patient.Create("Claudia", "claudia@example.com").Value,
                Patient.Create("Mario", "mario@example.com").Value
            };

            var dbSet = patients.BuildMockDbSet();
            _db.Patients.Returns(dbSet);

            var request = new GetPatientListQuery { Page = 2, RecordsPerPage = 2 };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(3);
            result.Value.Items.Select(p => p.Name).Should().Equal("Mario");
        }

        [Fact]
        public async Task Handle_EmailFilter_ReturnsOnlyMatchingPatientsAndFilteredTotal()
        {
            // Arrange
            var patients = new List<Patient>
            {
                Patient.Create("Felipe", "felipe@example.com").Value,
                Patient.Create("Claudia", "claudia@example.com").Value
            };

            var dbSet = patients.BuildMockDbSet();
            _db.Patients.Returns(dbSet);

            var request = new GetPatientListQuery { Page = 1, RecordsPerPage = 10, Email = "felipe@" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(1);
            result.Value.Items.Select(p => p.Name).Should().Equal("Felipe");
        }

        [Fact]
        public async Task Handle_NoPatientsExist_ReturnsEmptyListAndTotalZero()
        {
            // Arrange
            var dbSet = new List<Patient>().BuildMockDbSet();
            _db.Patients.Returns(dbSet);

            var request = new GetPatientListQuery { Page = 1, RecordsPerPage = 5 };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(0);
            result.Value.Items.Should().BeEmpty();
        }
    }
}
