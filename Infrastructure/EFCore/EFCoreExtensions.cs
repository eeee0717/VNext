using System.Linq;
using System.Linq.Expressions;
using DomainCommons.Models;

namespace Microsoft.EntityFrameworkCore;

public static class EFCoreExtensions
{
  /// <summary>
  /// set global 'IsDeleted=false' query-filter for every entity
  /// </summary>
  /// <param name="modelBuilder"></param>
  public static void EnableSoftDeletionGlobalFilter(this ModelBuilder modelBuilder)
  {
    // 筛选出所有实现了 ISoftDelete 接口的实体
    var entityTypesHasSoftDeletion = modelBuilder.Model.GetEntityTypes()
      .Where(e => e.ClrType.IsAssignableTo(typeof(ISoftDelete)));
    
    // 为每个实体设置查询过滤器
    foreach (var entityType in entityTypesHasSoftDeletion)
    {
      // 寻找实体中的 IsDeleted 属性
      var isDeletedProperty = entityType.FindProperty(nameof(ISoftDelete.IsDeleted));
      // 设置查询过滤器
      var parameter = Expression.Parameter(entityType.ClrType, "p");
      // 生成表达式树
      var filter = Expression.Lambda(Expression.Not(Expression.Property(parameter, isDeletedProperty!.PropertyInfo!)),
        parameter);
      entityType.SetQueryFilter(filter);
    }
  }
}