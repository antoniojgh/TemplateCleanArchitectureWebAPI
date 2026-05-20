using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeList;
using DientesLimpios.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class GetOfficeListUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly ILogger<GetOfficeListHandler> _logger;
        private readonly GetOfficeListHandler _handler;

        public GetOfficeListUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<GetOfficeListHandler>>();

            _handler = new GetOfficeListHandler(_db, _logger);
        }


        [Fact]
        public async Task Handle_OfficesExist_ReturnsOfficeListDTOs()
        {
            // Arrange
            var offices = new List<Office>
                {
                    Office.Create("Office A").Value,
                    Office.Create("Office B").Value,
                };

            var dbSet = offices.BuildMockDbSet();
            _db.Offices.Returns(dbSet);

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
        public async Task Handle_NoOfficesExist_ReturnsEmptyList()
        {
            // Arrange
            var dbSet = new List<Office>().BuildMockDbSet();
            _db.Offices.Returns(dbSet);

            // Act
            var result = await _handler.Handle(new GetOfficeListQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Count.Should().Be(0);
        }
    }
}
