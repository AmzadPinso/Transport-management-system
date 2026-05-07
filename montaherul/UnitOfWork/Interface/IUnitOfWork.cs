using montaherul.Repository.Interface;

namespace montaherul.UnitOfWork.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepo    Student { get; }
        ICourseRepo Course { get; }

        // Add more repository properties as needed

        Task<int> SaveChangesAsync();
    }
}
