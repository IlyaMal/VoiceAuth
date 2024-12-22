using VoiceAuth.QueryModels;
using VoiceAuth.Services;

namespace VoiceAuth;

public class AuthBLL: IAuthBLL
{
    private IConfiguration appConfig;
    
    public AuthBLL(IConfiguration configuration)
    {
        appConfig = configuration;
    }
    
    public bool VerifyUserByPassword(QueryAuthModel model)
    {
        var root = appConfig["Login"];
        var password = appConfig["Password"];
        var salt = appConfig["Salt"];
        if (model.username == root)
        {
            if (password == EncryptService.HashPassword(model.password, salt))
            {
                return true;
            }
        }

        return false;
    }
}