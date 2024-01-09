using Microsoft.AspNetCore.Builder;

namespace EventBus;

public static class ApplicationBuilderExtensions
{
  public static void UseEventBus(this IApplicationBuilder appBuilder)
  {
    //获得IEventBus一次，就会立即加载IEventBus，这样扫描所有的EventHandler，保证消息及时消费
    object? eventBus = appBuilder.ApplicationServices.GetService(typeof(IEventBus));
    if (eventBus == null)
    {
      throw new ApplicationException("找不到IEventBus实例");
    }
  }
}