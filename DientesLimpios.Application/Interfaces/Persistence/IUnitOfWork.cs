namespace DientesLimpios.Application.Interfaces.Persistence
{
    public interface IUnitOfWork
    {
        Task SaveChanges();
        Task Reversar();
    }
}
