using Timeenator.Interfaces;

namespace Timeenator.Impl.Grouping
{
    public interface IExecutableGroup<T> where T : struct
    {
        INullableQuerySerie<T> ExecuteGrouping();
    }
}