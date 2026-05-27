using OpenCBT.Domain.Entities;

namespace OpenCBT.Domain.Interfaces;

public interface IExamRepository : IGenericRepository<Exam>
{
    Task<IEnumerable<Exam>> GetActiveExamsAsync();
}
