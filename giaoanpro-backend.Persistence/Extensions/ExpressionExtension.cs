using System.Linq.Expressions;

namespace giaoanpro_backend.Persistence.Extensions
{
	public static class ExpressionExtension
	{
		public static Expression<Func<T, bool>> AndAlso<T>(
			this Expression<Func<T, bool>> expr1,
			Expression<Func<T, bool>> expr2)
		{
			var parameter = Expression.Parameter(typeof(T));

			var body = Expression.AndAlso(
				Expression.Invoke(expr1, parameter),
				Expression.Invoke(expr2, parameter)
			);

			return Expression.Lambda<Func<T, bool>>(body, parameter);
		}
	}
}
