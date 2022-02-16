namespace DgPersonal.Persistence.Interfaces
{
    public interface IEqualityComparer<TFirst, TSecond>
    {
        bool Equals(TFirst x, TSecond y);
    }
}