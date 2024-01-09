using System.Diagnostics;
using System.Net;
using IdentityService.Domain;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.WebAPI.Controllers.Login;
[Route("[controller]/[action]")]
[ApiController]
public class LoginController:ControllerBase
{
    private readonly IIdRepository _repository;
    private readonly IdDomainService _idService;
    public LoginController(IdDomainService idService, IIdRepository repository)
    {
      this._idService = idService;
      this._repository = repository;
    }
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CreateWorld()
    {
      if (await _repository.FindByNameAsync("admin") != null)
      {
        return StatusCode((int)HttpStatusCode.Conflict, "已经初始化过了");
      }
      User user = new User("admin");
      var r = await _repository.CreateAsync(user, "123456");
      Debug.Assert(r.Succeeded);
      var token = await _repository.GenerateChangePhoneNumberTokenAsync(user, "18918999999");
      var cr = await _repository.ChangePhoneNumAsync(user.Id, "18918999999", token);
      Debug.Assert(cr.Succeeded);
      r = await _repository.AddToRoleAsync(user, "User");
      Debug.Assert(r.Succeeded);
      r = await _repository.AddToRoleAsync(user, "Admin");
      Debug.Assert(r.Succeeded);
      return Ok();
    }
  

}