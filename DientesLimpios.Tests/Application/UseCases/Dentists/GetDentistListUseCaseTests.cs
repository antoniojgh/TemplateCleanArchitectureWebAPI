using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Dentists
{
    public class GetDentistListUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<GetDentistListHandler> _logger;
        private readonly GetDentistListHandler _handler;

        public GetDentistListUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<GetDentistListHandler>>();

            _handler = new GetDentistListHandler(_db, _logger);
        }

        [Fact]
        public async Task Handle_FirstPage_ReturnsPageOrderedByNameAndTotalOfAllDentists()
        {
            // Arrange
            var dentists = new List<Dentist>
            {
                Dentist.Create("Felipe", "felipe@example.com").Value,
                Dentist.Create("Claudia", "claudia@example.com").Value,
                Dentist.Create("Mario", "mario@example.com").Value
            };

            var dbSet = dentists.BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            var request = new GetDentistListQuery { Page = 1, RecordsPerPage = 2 };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(3);
            result.Value.Items.Select(d => d.Name).Should().Equal("Claudia", "Felipe");
            result.Value.Items[0].Email.Should().Be("claudia@example.com");
        }

        [Fact]
        public async Task Handle_LastPage_ReturnsRemainingDentistsAndTotalOfAllDentists()
        {
            // Arrange
            var dentists = new List<Dentist>
            {
                Dentist.Create("Felipe", "felipe@example.com").Value,
                Dentist.Create("Claudia", "claudia@example.com").Value,
                Dentist.Create("Mario", "mario@example.com").Value
            };

            var dbSet = dentists.BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            var request = new GetDentistListQuery { Page = 2, RecordsPerPage = 2 };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(3);
            result.Value.Items.Select(d => d.Name).Should().Equal("Mario");
        }

        [Fact]
        public async Task Handle_NameFilter_ReturnsOnlyMatchingDentistsAndFilteredTotal()
        {
            // Arrange
            var dentists = new List<Dentist>
            {
                Dentist.Create("Felipe", "felipe@example.com").Value,
                Dentist.Create("Claudia", "claudia@example.com").Value
            };

            var dbSet = dentists.BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            var request = new GetDentistListQuery { Page = 1, RecordsPerPage = 10, Name = "Clau" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(1);
            result.Value.Items.Select(d => d.Name).Should().Equal("Claudia");
        }

        [Fact]
        public async Task Handle_NoDentistsExist_ReturnsEmptyListAndTotalZero()
        {
            // Arrange
            var dbSet = new List<Dentist>().BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            var request = new GetDentistListQuery { Page = 1, RecordsPerPage = 5 };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(0);
            result.Value.Items.Should().BeEmpty();
        }
    }
}
