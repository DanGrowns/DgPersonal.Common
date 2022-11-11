using Mapster;

namespace DgPersonal.Persistence.Interfaces
{
    public interface IStateChange<TDto>
    {
        void SetStateFromDto(TDto dto) 
            => dto.Adapt(this);
    }
}