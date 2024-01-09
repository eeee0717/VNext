using IdentityService.Domain;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services;

public class MockSmsSender: ISmsSender
{
  private readonly ILogger<MockSmsSender> _logger;
  public MockSmsSender(ILogger<MockSmsSender> logger)
  {
    this._logger = logger;
  }

  public Task SendAsync(string phoneNum, params string[] args)
  {
    this._logger.LogInformation("Send Sms to {0},args:{1}", phoneNum,
      string.Join(",", args));
    return Task.CompletedTask;  }
}