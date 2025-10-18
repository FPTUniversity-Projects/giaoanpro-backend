using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly GiaoanproDBContext _context;
		private readonly Dictionary<Type, object> _repositories = new();
		private IDbContextTransaction? _transaction;

		public UnitOfWork(GiaoanproDBContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public IGenericRepository<T> Repository<T>() where T : class
		{
			var type = typeof(T);
			if (_repositories.TryGetValue(type, out var repo))
				return (IGenericRepository<T>)repo!;

			var repositoryInstance = new GenericRepository<T>(_context);
			_repositories[type] = repositoryInstance!;
			return repositoryInstance;
		}

		public async Task<bool> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<int> SaveChangesAndReturnCountAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public async Task BeginTransactionAsync()
		{
			if (_transaction != null) return;
			_transaction = await _context.Database.BeginTransactionAsync();
		}

		public async Task CommitTransactionAsync()
		{
			if (_transaction == null) return;

			try
			{
				await _context.SaveChangesAsync();
				await _transaction.CommitAsync();
			}
			finally
			{
				await _transaction.DisposeAsync();
				_transaction = null;
			}
		}

		public async Task RollbackTransactionAsync()
		{
			if (_transaction == null) return;

			try
			{
				await _transaction.RollbackAsync();
			}
			finally
			{
				await _transaction.DisposeAsync();
				_transaction = null;
			}
		}

		// Dispose / async dispose - does not dispose the DbContext if it is managed by DI container,
		// but will clear repository cache and ensure transaction disposed.
		public void Dispose()
		{
			// best-effort synchronous dispose for transaction
			if (_transaction != null)
			{
				_transaction.Dispose();
				_transaction = null;
			}
			_repositories.Clear();
			// Do NOT dispose _context here when context is managed by DI container.
			GC.SuppressFinalize(this);
		}

		public async ValueTask DisposeAsync()
		{
			if (_transaction != null)
			{
				await _transaction.DisposeAsync();
				_transaction = null;
			}
			_repositories.Clear();
			GC.SuppressFinalize(this);
		}
	}
}
