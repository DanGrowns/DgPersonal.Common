namespace DgPersonal.Common.Interfaces
{
    public interface IStateChange<TDto>
    {
        void SetStateFromDto(TDto dto);
    }
}