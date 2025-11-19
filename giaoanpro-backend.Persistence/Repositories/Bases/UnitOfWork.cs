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

        public ILessonPlanRepository LessonPlans
        {
            get
            {
                var key = typeof(ILessonPlanRepository);
                if (!_repositories.TryGetValue(key, out var repo))
                {
                    repo = new LessonPlanRepository(_context);
                    _repositories[key] = repo!;
                }
                return (ILessonPlanRepository)repo!;
            }
        }

        public IActivityRepository Activities
        {
            get
            {
                var key = typeof(IActivityRepository);
                if (!_repositories.TryGetValue(key, out var repo))
                {
                    repo = new ActivityRepository(_context);
                    _repositories[key] = repo!;
                }
                return (IActivityRepository)repo!;
            }
        }

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

		public IGradeRepository Grades
		{
			get
			{
				var key = typeof(IGradeRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new GradeRepository(_context);
					_repositories[key] = repo!;
				}
				return (IGradeRepository)repo!;
			}
		}

		public ISemesterRepository Semesters
		{
			get
			{
				var key = typeof(ISemesterRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new SemesterRepository(_context);
					_repositories[key] = repo!;
				}
				return (ISemesterRepository)repo!;
			}
		}

		public ISubjectRepository Subjects
		{
			get
			{
				var key = typeof(ISubjectRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new SubjectRepository(_context);
					_repositories[key] = repo!;
				}
				return (ISubjectRepository)repo!;
			}
		}

		public ISyllabusRepository Syllabuses
		{
			get
			{
				var key = typeof(ISyllabusRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new SyllabusRepository(_context);
					_repositories[key] = repo!;
				}
				return (ISyllabusRepository)repo!;
			}
		}

		public IClassRepository Classes
		{
			get
			{
				var key = typeof(IClassRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new ClassRepository(_context);
					_repositories[key] = repo!;
				}
				return (IClassRepository)repo!;
			}
		}

		public IUserRepository Users
		{
			get
			{
				var key = typeof(IUserRepository);
				if (!_repositories.TryGetValue(key, out var repo))
				{
					repo = new UserRepository(_context);
					_repositories[key] = repo!;
				}
				return (IUserRepository)repo!;
			}
		}
        public IExamRepository Exams
        {
            get
            {
                var key = typeof(IExamRepository);
                if (!_repositories.TryGetValue(key, out var repo))
                {
                    repo = new ExamRepository(_context);
                    _repositories[key] = repo!;
                }
                return (IExamRepository)repo!;
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
