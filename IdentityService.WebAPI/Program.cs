using IdentityService.Domain;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContextFactory<IdDbContext>(options =>
{
  options.UseMySql("server=127.0.0.1;port=3306;user=root;password=123456;database=IdentityService;",
    ServerVersion.Parse("8.2.0-mysql"));
});

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();