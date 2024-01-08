using IdentityService.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure;

public class IdDbContext : IdentityDbContext<User, Role, Guid>
{
    public IdDbContext(DbContextOptions<IdDbContext> options)
        : base(options)
    {
    }
    // 没有这行迁移会报错
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;user=root;password=123456;database=IdentityService;", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.2.0-mysql"));
    //
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        modelBuilder.EnableSoftDeletionGlobalFilter();
    }
}
