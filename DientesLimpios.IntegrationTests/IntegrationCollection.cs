namespace DientesLimpios.IntegrationTests
{
    [CollectionDefinition(Name)]
    public sealed class IntegrationCollection : ICollectionFixture<IntegrationTestFactory>
    {
        public const string Name = "Integration";
    }

}
