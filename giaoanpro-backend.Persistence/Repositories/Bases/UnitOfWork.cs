using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace giaoanpro_backend.Persistence.Repositories.Bases
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly GiaoanproDBContext _context;
		private readonly Dictionary<Type, object> _repositories = new();
		private IDbContextTransaction? _transaction;

		public ISubscriptionRepository Subscriptions
		{
			get
			{
				var key = typeof(ISubscriptionRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new SubscriptionRepository(_context);
					_repositories[key] = repo!;
				}
				return (ISubscriptionRepository)repo!;
			}
		}

		public ISubscriptionPlanRepository SubscriptionPlans
		{
			get
			{
				var key = typeof(ISubscriptionPlanRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new SubscriptionPlanRepository(_context);
					_repositories[key] = repo!;
				}
				return (ISubscriptionPlanRepository)repo!;
			}
		}

		public IPaymentRepository Payments
		{
			get
			{
				var key = typeof(IPaymentRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new PaymentRepository(_context);
					_repositories[key] = repo!;
				}
				return (IPaymentRepository)repo!;
			}
		}

		public UnitOfWork(GiaoanproDBContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
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
