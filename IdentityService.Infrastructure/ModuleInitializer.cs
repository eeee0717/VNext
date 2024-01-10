using IdentityService.Domain;
using Microsoft.Extensions.DependencyInjection;
using VNext.Commons;

namespace IdentityService.Infrastructure;

public class ModuleInitializer : IModuleInitializer
{
  public void Initialize(IServiceCollection services)
  {
    services.AddScoped<IdDomainService>();
    services.AddScoped<IIdRepository, IdRepository>();
  }
}