using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
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
        private readonly IDentistRepository _repository;
        private readonly ILogger<GetDentistListHandler> _logger;
        private readonly GetDentistListHandler _handler;

        public GetDentistListUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _repository = Substitute.For<IDentistRepository>();
            _logger = Substitute.For<ILogger<GetDentistListHandler>>();

            _handler = new GetDentistListHandler(_repository, _db, _logger);

        }

        [Fact]
        public async Task Handle_DentistsExist_ReturnsPagedDTOsCorrectly()
        {
            // Arrange
            var page = 1;
            var pageSize = 2;

            var dentist1 = Dentist.Create("Felipe", "felipe@ejemplo.com").Value;
            var dentist2 = Dentist.Create("Claudia", "claudia@ejemplo.com").Value;

            var dentists = new List<Dentist> { dentist1, dentist2 };

            _repository.GetFiltered(Arg.Any<DentistFilterDTO>(), Arg.Any<CancellationToken>()).Returns(dentists);

            var allDentists = Enumerable.Range(0, 10).Select(i => Dentist.Create($"Name{i}", $"email{i}@test.com").Value).ToList();
            var dbSet = allDentists.BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            var request = new GetDentistListQuery
            {
                Page = page,
                RecordsPerPage = pageSize
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
            await _repository.Received(1).GetFiltered(Arg.Any<DentistFilterDTO>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_NoDentistsExist_ReturnsEmptyListAndTotalZero()
        {
            // Arrange
            var page = 1;
            var pageSize = 5;

            var filterDto = new DentistFilterDTO { Page = page, RecordsPerPage = pageSize };

            IEnumerable<Dentist> dentists = new List<Dentist>();

            _repository.GetFiltered(Arg.Any<DentistFilterDTO>(), Arg.Any<CancellationToken>()).Returns(dentists);

            var dbSet = new List<Dentist>().BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            var request = new GetDentistListQuery
            {
                Page = page,
                RecordsPerPage = pageSize
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
