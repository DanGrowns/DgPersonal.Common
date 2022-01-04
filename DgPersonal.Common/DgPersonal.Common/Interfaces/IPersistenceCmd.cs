using System.Collections.Generic;

namespace DgPersonal.Common.Interfaces
{
    public interface IPersistenceCmd<T> where T : class
    {
        IReadOnlyList<string> NavigationIncludes() 
            => new List<string>();
    }
}