using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using VNext.ASPNETCore;
using VNext.Commons;
using VNext.Commons.JsonConverters;
using VNext.JWT;

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
    //开始:Authentication,Authorization
    //只要需要校验Authentication报文头的地方（非IdentityService.WebAPI项目）也需要启用这些
    //IdentityService项目还需要启用AddIdentityCore
    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication();
    JWTOptions jwtOpt = configuration.GetSection("JWT").Get<JWTOptions>();
    builder.Services.AddJwtAuthentication(jwtOpt);
    //启用Swagger中的【Authorize】按钮。这样就不用每个项目的AddSwaggerGen中单独配置了
    builder.Services.Configure<SwaggerGenOptions>(c =>
    {
      c.AddAuthenticationHeader();
    });
    //结束:Authentication,Authorization
    // 进程内进行消息传递
    services.AddMediatR(assemblies);
    services.Configure<MvcOptions>(options =>
    {
      options.Filters.Add<UnitOfWorkFilter>();
    });
    services.Configure<JsonOptions>(options =>
    {
      //设置时间格式。而非“2008-08-08T08:08:08”这样的格式
      options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
    });
  }
}