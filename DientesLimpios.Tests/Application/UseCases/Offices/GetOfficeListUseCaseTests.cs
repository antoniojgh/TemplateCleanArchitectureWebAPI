using DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeList;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class GetOfficeListUseCaseTests
    {
        private readonly IOfficeRepository _repositorio;
        private readonly ILogger<GetOfficeListHandler> _logger;
        private readonly GetOfficeListHandler _handler;

        public GetOfficeListUseCaseTests()
        {
            _repositorio = Substitute.For<IOfficeRepository>();
            _logger = Substitute.For<ILogger<GetOfficeListHandler>>();

            _handler = new GetOfficeListHandler(_repositorio, _logger);
        }


        [Fact]
        public async Task Handle_CuandoHayOffices_RetornaListaDeOfficeListDTO()
        {
            // Arrange
            var offices = new List<Office>
                {
                    Office.Create("Office A").Value,
                    Office.Create("Office B").Value,
                };

            _repositorio.GetAll().Returns(offices);

            // Act
            var result = await _handler.Handle(new GetOfficeListQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Count.Should().Be(offices.Count);

            for (int i = 0; i < offices.Count; i++)
            {
                result.Value[i].Id.Should().Be(offices[i].Id);
                result.Value[i].Name.Should().Be(offices[i].Name);
            }
        }

        [Fact]
        public async Task Handle_CuandoNoHayOffices_RetornaListaVacia()
        {
            // Arrange
            _repositorio.GetAll().Returns(new List<Office>());

            // Act
            var result = await _handler.Handle(new GetOfficeListQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Count.Should().Be(0);
            await _repositorio.Received(1).GetAll();
        }
    }
}
