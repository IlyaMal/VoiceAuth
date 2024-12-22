namespace VoiceAuth;

public interface IVoiceBLL
{
    void TrainAndSaveModel();
    List<double> VerifyVoiceByVoice(string audioFilePath);

}