using DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeDetail;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class GetOfficeDetailUseCaseTests
    {
        private readonly IOfficeRepository _repositorio;
        private readonly ILogger<GetOfficeDetailHandler> _logger;
        private readonly GetOfficeDetailHandler _handler;

        public GetOfficeDetailUseCaseTests()
        {
            _repositorio = Substitute.For<IOfficeRepository>();
            _logger = Substitute.For<ILogger<GetOfficeDetailHandler>>();

            _handler = new GetOfficeDetailHandler(_repositorio, _logger);
        }


        [Fact]
        public async Task Handle_OfficeExiste_RetornaDTO()
        {
            // Arrange
            var officeResult = Office.Create("Office A");
            var office = officeResult.Value;

            var id = office.Id;
            var query = new GetOfficeDetailQuery { Id = id };

            _repositorio.GetById(id).Returns(office);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(id);
            result.Value.Name.Should().Be("Office A");
            await _repositorio.Received(1).GetById(id);
        }

        [Fact]
        public async Task Handle_OfficeNoExiste_RetornaFailureNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var query = new GetOfficeDetailQuery { Id = id };
            _repositorio.GetById(id).ReturnsNull();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NotFound);
            await _repositorio.Received(1).GetById(id);
        }

    }
}
