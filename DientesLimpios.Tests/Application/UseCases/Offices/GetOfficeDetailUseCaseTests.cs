using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeDetail;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class GetOfficeDetailUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<GetOfficeDetailHandler> _logger;
        private readonly GetOfficeDetailHandler _handler;

        public GetOfficeDetailUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<GetOfficeDetailHandler>>();

            _handler = new GetOfficeDetailHandler(_db, _logger);
        }


        [Fact]
        public async Task Handle_OfficeExiste_RetornaDTO()
        {
            // Arrange
            var officeResult = Office.Create("Office A");
            var office = officeResult.Value;

            var id = office.Id;
            var query = new GetOfficeDetailQuery { Id = id };

            var dbSet = new List<Office> { office }.BuildMockDbSet();
            _db.Offices.Returns(dbSet);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(id);
            result.Value.Name.Should().Be("Office A");
        }

        [Fact]
        public async Task Handle_OfficeNoExiste_RetornaFailureNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var query = new GetOfficeDetailQuery { Id = id };

            var dbSet = new List<Office>().BuildMockDbSet();
            _db.Offices.Returns(dbSet);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NotFound);
        }

    }
}
