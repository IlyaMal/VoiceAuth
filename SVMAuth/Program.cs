using Accord.IO;
using Accord.MachineLearning.VectorMachines;
using Accord.Statistics.Kernels;
using VoiceAuth;

namespace SVMAuth;

class Program
{
    public static void Main(string[] args)
    {
        Test test = new Test();
        string modelFilePath;
        string[] userFiles = { "user1_record1.wav", "user1_record2.wav",   "user1_record3.wav", "user1_record4.wav"};
        string[] otherFiles = { "other_record.wav", "other1_record1.wav", "other2_record2.wav", "other3_record3.wav", "other4_record4.wav","other5_record5.wav", "other6_record6.wav","other7_record7.wav", "other8_record8.wav", "other9_record9.wav", "other10_record10.wav" };
        modelFilePath = "user1_svm_model.bin";

        // Обучаем и сохраняем модель
        //test.TrainAndSaveModel(userFiles, otherFiles, modelFilePath);
        
        
        
        modelFilePath = "user1_svm_model.bin";
        string testAudioFile = "user1_record1.wav";

        bool accessGranted = test.VerifyVoice(testAudioFile, modelFilePath);

        Console.WriteLine(accessGranted ? "Access Granted" : "Access Denied");
    }
}

class Test
{
    public void SaveModel(SupportVectorMachine<Gaussian> svm, string modelFilePath)
    {
        // Сохранение модели в файл
        svm.Save(modelFilePath);
    }
    public void TrainAndSaveModel(string[] userFiles, string[] otherFiles, string modelFilePath)
    {
        var voiceAuth = new VoiceAuthenticationSystem();

        // Обучаем модель для пользователя
        var svm = voiceAuth.TrainUserModel(userFiles, otherFiles);

        // Сохраняем модель на диск
        SaveModel(svm, modelFilePath);
    }
    public SupportVectorMachine<Gaussian> LoadModel(string modelFilePath)
    {
        // Загрузка модели из файла
        return Serializer.Load<SupportVectorMachine<Gaussian>>(modelFilePath);
    }
    public bool VerifyVoice(string audioFilePath, string modelFilePath)
    {
        var voiceAuth = new VoiceAuthenticationSystem();

        // Загрузить модель из файла
        var svm = LoadModel(modelFilePath);

        // Извлечь MFCC признаки из аудиофайла
        var voiceFeatures = voiceAuth.ExtractVoiceFeatures(audioFilePath);

        // Проверить, принадлежит ли голос пользователю
        bool prediction = svm.Decide(voiceFeatures);

        return prediction; // 1 - голос пользователя, 0 - не пользователь
    }
}