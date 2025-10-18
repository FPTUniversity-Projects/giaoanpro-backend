namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IUnitOfWork : IDisposable, IAsyncDisposable
	{
		/// <summary>
		/// Returns a generic repository for the given entity type. Instances are cached per UnitOfWork.
		/// </summary>
		IGenericRepository<T> Repository<T>() where T : class;

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
	}
}
