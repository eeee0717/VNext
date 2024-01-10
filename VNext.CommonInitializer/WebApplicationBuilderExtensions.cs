using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VNext.Commons;

namespace VNext.CommonInitializer;

public static class WebApplicationBuilderExtensions
{
  public static void ConfigureExtraServices(this WebApplicationBuilder builder, InitializerOptions initOptions)
  {
    IServiceCollection services = builder.Services;
    IConfiguration configuration = builder.Configuration;
    var assemblies = ReflectionHelper.GetAllReferencedAssemblies();
    services.RunModuleInitializers(assemblies);
    services.AddAllDbContexts(ctx =>
    {
      ctx.UseMySql("server=127.0.0.1;port=3306;user=root;password=123456;database=IdentityService;",
        ServerVersion.Parse("8.2.0-mysql"));
    }, assemblies);

  }
}