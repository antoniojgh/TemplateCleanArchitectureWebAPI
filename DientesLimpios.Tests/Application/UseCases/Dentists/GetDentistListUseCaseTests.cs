using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Dentists
{
    public class GetDentistListUseCaseTests
    {
        private readonly IDentistRepository _repositorio;
        private readonly ILogger<GetDentistListHandler> _logger;
        private readonly GetDentistListHandler _handler;

        public GetDentistListUseCaseTests()
        {
            _repositorio = Substitute.For<IDentistRepository>();
            _logger = Substitute.For<ILogger<GetDentistListHandler>>();

            _handler = new GetDentistListHandler(_repositorio, _logger);

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

            _repositorio.GetFiltered(Arg.Any<DentistFilterDTO>()).Returns(dentists);

            _repositorio.GetTotalRecordCount().Returns(10);

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
            await _repositorio.Received(1).GetFiltered(Arg.Any<DentistFilterDTO>());
            await _repositorio.Received(1).GetTotalRecordCount();
        }

        [Fact]
        public async Task Handle_CuandoNoHayDentists_RetornaListaVaciaYTotalCero()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 5;

            var patientFilterDTO = new DentistFilterDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            IEnumerable<Dentist> dentists = new List<Dentist>();

            _repositorio.GetFiltered(Arg.Any<DentistFilterDTO>()).Returns(dentists);

            _repositorio.GetTotalRecordCount().Returns(0);

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
