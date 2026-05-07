using montaherul.Repository.Interface;
using montaherul.UnitOfWork.Interface;

namespace montaherul.UnitOfWork.Application
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDBContext _context;

        public IStudentRepo Student { get; }
        public ICourseRepo Course { get; }

        public UnitOfWork(
            MyDBContext context,
            IStudentRepo studentRepo,
            ICourseRepo courseRepo)
        {
            _context = context;
            Student = studentRepo;
            Course = courseRepo;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}