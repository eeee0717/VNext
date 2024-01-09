using VNext.CommonInitializer;
using IdentityService.Domain;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// 可以优化成通用初始化
builder.Services.AddDbContextFactory<IdDbContext>(options =>
{
  options.UseMySql("server=127.0.0.1;port=3306;user=root;password=123456;database=IdentityService;",
    ServerVersion.Parse("8.2.0-mysql"));
});

// builder.ConfigureExtraServices(new InitializerOptions
// {
//   EventBusQueueName = "IdentityService.WebAPI",
//   LogFilePath = "./log/IdentityService.log"
// });

builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new() { Title = "IdentityService.WebAPI", Version = "v1" });
  //c.AddAuthenticationHeader();
});
builder.Services.AddDataProtection();
IdentityBuilder idBuilder = builder.Services.AddIdentityCore<User>(options =>
{
  options.Password.RequireDigit = false;
  options.Password.RequireLowercase = false;
  options.Password.RequireNonAlphanumeric = false;
  options.Password.RequireUppercase = false;
  options.Password.RequiredLength = 6;
  //不能设定RequireUniqueEmail，否则不允许邮箱为空
  //options.User.RequireUniqueEmail = true;
  //以下两行，把GenerateEmailConfirmationTokenAsync验证码缩短
  options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
  options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
});
idBuilder = new IdentityBuilder(idBuilder.UserType, typeof(Role), builder.Services);
idBuilder.AddEntityFrameworkStores<IdDbContext>()
  .AddDefaultTokenProviders()
  .AddRoleManager<RoleManager<Role>>()
  .AddUserManager<IdUserManager>();


if (builder.Environment.IsDevelopment())
{
  builder.Services.AddScoped<IEmailSender, MockEmailSender>();
  builder.Services.AddScoped<ISmsSender, MockSmsSender>();
}
// else
// {
//   builder.Services.AddScoped<IEmailSender, SendCloudEmailSender>();
//   builder.Services.AddScoped<ISmsSender, SendCloudSmsSender>();
// }

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseSwagger();
  app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityService.WebAPI v1"));
}

// app.UseDefault();

app.MapControllers();

app.Run();