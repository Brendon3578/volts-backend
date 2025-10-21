using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;

namespace Volts.Domain.Interfaces
{
    public interface IPositionRepository : IRepository<Position>
    {
        Task<IEnumerable<Position>> GetByGroupIdAsync(string groupId);
    }
}
