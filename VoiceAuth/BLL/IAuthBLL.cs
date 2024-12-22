using VoiceAuth.QueryModels;

namespace VoiceAuth;

public interface IAuthBLL
{
    bool VerifyUserByPassword(QueryAuthModel model);
}