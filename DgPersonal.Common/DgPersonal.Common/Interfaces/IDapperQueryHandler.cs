using System.Collections.Generic;
using System.Threading.Tasks;

namespace DgPersonal.Common.Interfaces
{
    public interface IDapperQueryHandler
    {
        Task<TOutput> GetFirstOrDefault<TOutput>(object sqlParameters = null);
        Task<List<TOutput>> GetList<TOutput>(object sqlParameters = null);
    }
}