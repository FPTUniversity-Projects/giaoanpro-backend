using giaoanpro_backend.Application.DTOs.Responses.Users;
using giaoanpro_backend.Domain.Entities;
using Mapster;

namespace giaoanpro_backend.Application.Mappings
{
	public class UserRegister : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<User, GetUserLookupResponse>()
				.Map(dest => dest.Id, src => src.Id)
				.Map(dest => dest.Email, src => src.Email)
				.Map(dest => dest.FullName, src => src.FullName)
				.Map(dest => dest.Role, src => src.Role);
		}
	}
}
