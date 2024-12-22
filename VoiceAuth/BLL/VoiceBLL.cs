using VoiceAuth.Services;

namespace VoiceAuth;

public class VoiceBLL: IVoiceBLL
{
    private SvmService _svmService = new SvmService();
    private string modelFilePath = "user1_svm_model2.bin";
    private string[] userFiles = {"recording3.wav","recording4.wav","recording5.wav","work.wav", "workpls.wav", "working.wav", "fuck.wav"};//"recording.wav","recording1.wav", "recording2.wav",
    private string[] otherFiles = { "bad.wav","bad2.wav", "bad3.wav","1.wav","2.wav","3.wav" };//"us3.wav",   "us4.wav", "us5.wav", "us6.wav",   "us7.wav",   "us8.wav","us9.wav","us10.wav", 

    
    public void TrainAndSaveModel()
    {
        // Обучаем модель для пользователя
         _svmService.TrainAndSaveModel(userFiles, otherFiles, modelFilePath);
    }
    public List<double> VerifyVoiceByVoice(string audioFilePath)
    { 
        return _svmService.VerifyVoiceByVoice(audioFilePath, this.modelFilePath);
    }
}