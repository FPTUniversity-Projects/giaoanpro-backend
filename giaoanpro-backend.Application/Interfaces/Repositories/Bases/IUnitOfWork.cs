using giaoanpro_backend.Application.Interfaces.Repositories;

namespace giaoanpro_backend.Application.Interfaces.Repositories.Bases
{
	public interface IUnitOfWork : IDisposable, IAsyncDisposable
	{
		/// <summary>
		/// Save changes and return true if one or more rows were affected.
		/// </summary>
		Task<bool> SaveChangesAsync();

		/// <summary>
		/// Save changes and return number of affected rows.
		/// </summary>
		Task<int> SaveChangesAndReturnCountAsync();

		/// <summary>
		/// Transaction helpers.
		/// </summary>
		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();

		// Repositories
		ISubscriptionRepository Subscriptions { get; }
		ISubscriptionPlanRepository SubscriptionPlans { get; }
		IPaymentRepository Payments { get; }
		IExamRepository Exams { get; }
		IGradeRepository Grades { get; }
		ISemesterRepository Semesters { get; }
		ISubjectRepository Subjects { get; }
		ISyllabusRepository Syllabuses { get; }
		IClassRepository Classes { get; }
		IUserRepository Users { get; }
        ILessonPlanRepository LessonPlans { get; }
        IActivityRepository Activities { get; }
    }
}
