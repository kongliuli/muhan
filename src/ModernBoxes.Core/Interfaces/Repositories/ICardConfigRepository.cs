using ModernBoxes.Core.Models;
using System.Collections.Generic;

namespace ModernBoxes.Core.Interfaces.Repositories
{
    public interface ICardConfigRepository
    {
        void SyncCardConfigs(IEnumerable<CardContentModel> cards);
    }
}
