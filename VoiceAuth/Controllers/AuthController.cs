using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using VoiceAuth.QueryModels;

namespace VoiceAuth;


[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthBLL _authBll): ControllerBase
{
    
    
    [HttpPost("auth")]
    public async Task<IActionResult> Authorize([FromBody] QueryAuthModel model)
    {
        if (string.IsNullOrWhiteSpace(model.username) || string.IsNullOrWhiteSpace(model.password))
            return BadRequest();
        if (_authBll.VerifyUserByPassword(model))
        {
            return Ok();
        }
        return Unauthorized();
    }

    


   
}