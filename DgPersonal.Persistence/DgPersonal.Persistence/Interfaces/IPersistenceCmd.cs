using System.Collections.Generic;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IPersistenceCmd<T> where T : class
    {
        IReadOnlyList<string> NavigationIncludes() 
            => new List<string>();
    }
}