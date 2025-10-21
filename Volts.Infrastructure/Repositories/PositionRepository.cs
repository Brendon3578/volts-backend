using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;
using Volts.Domain.Interfaces;
using Volts.Infrastructure.Data;

namespace Volts.Infrastructure.Repositories
{
    public class PositionRepository : Repository<Position>, IPositionRepository
    {
        public PositionRepository(VoltsDbContext context) : base(context) { }

        public async Task<IEnumerable<Position>> GetByGroupIdAsync(string groupId)
        {
            return await _dbSet
                .Where(p => p.GroupId == groupId)
                .ToListAsync();
        }
    }
}
