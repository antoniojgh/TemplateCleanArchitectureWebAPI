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
        private readonly IDentistRepository _repositorio;
        private readonly ILogger<GetDentistListHandler> _logger;
        private readonly GetDentistListHandler _handler;

        public GetDentistListUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _repositorio = Substitute.For<IDentistRepository>();
            _logger = Substitute.For<ILogger<GetDentistListHandler>>();

            _handler = new GetDentistListHandler(_repositorio, _db, _logger);

        }

        [Fact]
        public async Task Handle_CuandoHayDentists_RetornaPagedConDTOsCorrectos()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 2;

            var dentista1 = Dentist.Create("Felipe", "felipe@ejemplo.com").Value;
            var dentista2 = Dentist.Create("Claudia", "claudia@ejemplo.com").Value;

            var dentists = new List<Dentist> { dentista1, dentista2 };

            _repositorio.GetFiltered(Arg.Any<DentistFilterDTO>(), Arg.Any<CancellationToken>()).Returns(dentists);

            var allDentists = Enumerable.Range(0, 10).Select(i => Dentist.Create($"Name{i}", $"email{i}@test.com").Value).ToList();
            var dbSet = allDentists.BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            var request = new GetDentistListQuery
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
            await _repositorio.Received(1).GetFiltered(Arg.Any<DentistFilterDTO>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_CuandoNoHayDentists_RetornaListaVaciaYTotalCero()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 5;

            var patientFilterDTO = new DentistFilterDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            IEnumerable<Dentist> dentists = new List<Dentist>();

            _repositorio.GetFiltered(Arg.Any<DentistFilterDTO>(), Arg.Any<CancellationToken>()).Returns(dentists);

            var dbSet = new List<Dentist>().BuildMockDbSet();
            _db.Dentists.Returns(dbSet);

            var request = new GetDentistListQuery
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
