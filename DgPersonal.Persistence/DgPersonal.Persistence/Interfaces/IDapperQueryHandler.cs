using System.Collections.Generic;
using System.Threading.Tasks;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IDapperQueryHandler
    {
        Task<TOutput> GetFirstOrDefault<TOutput>(object sqlParameters = null, string storedProcedureName = null);
        Task<List<TOutput>> GetList<TOutput>(object sqlParameters = null, string storedProcedureName = null);
    }
}