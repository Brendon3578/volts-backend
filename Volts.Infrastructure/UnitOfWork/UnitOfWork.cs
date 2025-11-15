using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Interfaces;
using Volts.Infrastructure.Data;
using Volts.Infrastructure.Repositories;

namespace Volts.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VoltsDbContext _context;
        private IDbContextTransaction? _transaction;

        public IUserRepository Users { get; }
        public IOrganizationRepository Organizations { get; }
        public IOrganizationMemberRepository OrganizationMembers { get; }
        public IGroupRepository Groups { get; }
        public IPositionRepository Positions { get; }
        public IShiftRepository Shifts { get; }
        public IShiftPositionRepository ShiftPositions { get; }
        public IShiftPositionAssignmentRepository ShiftPositionAssignment { get; }

        public UnitOfWork(VoltsDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Organizations = new OrganizationRepository(_context);
            OrganizationMembers = new OrganizationMemberRepository(_context);
            Groups = new GroupRepository(_context);
            Positions = new PositionRepository(_context);
            Shifts = new ShiftRepository(_context);
            ShiftPositions = new ShiftPositionRepository(_context);
            ShiftPositionAssignment = new ShiftPositionAssignmentRepository(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

    }
}
