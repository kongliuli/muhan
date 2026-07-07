using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernBoxes.Core.Interfaces
{
    public interface IPersistenceProvider
    {
        Task SaveAsync<T>(string entityName, IEnumerable<T> data);
        Task<IEnumerable<T>> LoadAsync<T>(string entityName);
    }
}
