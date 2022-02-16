using DgPersonal.Persistence.Interfaces;
using Mapster;

namespace DgPersonal.Persistence.Base
{
    public abstract class IntEntity : IEntity, IIntIdentifier
    {
        [AdaptIgnore]
        public int Id { get; protected internal set; } 
        
        public object GetPrimaryKey() => Id;
        public bool Exists() => Id > 0;
    }
}