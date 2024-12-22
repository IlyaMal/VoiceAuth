using Accord.IO;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Kernels;

namespace VoiceAuth.Services;

public class SvmService
{
    private VoiceFeaturesService VoiceFeaturesService = new VoiceFeaturesService();
    public void SaveModel(SupportVectorMachine<Sigmoid> svm, string modelFilePath)
        {
            // Сохранение модели в файл
            svm.Save(modelFilePath);
        }
        public void TrainAndSaveModel(string[] userFiles, string[] otherFiles, string modelFilePath)
        {


            // Обучаем модель для пользователя
            var (svm, accuracy, confusionMatrix) = TrainUserModel(userFiles, otherFiles);
            Console.WriteLine(accuracy);
            Console.WriteLine(confusionMatrix);

            // Сохраняем модель на диск
            SaveModel(svm, modelFilePath);
        }
        
        public (SupportVectorMachine<Sigmoid> Svm, double Accuracy, ConfusionMatrix ConfusionMatrix) TrainUserModel(string[] userFiles, string[] otherFiles)
    {
        // Извлечение MFCC признаков для пользователя и других
        List<float[]> userFeatures = new List<float[]>();
        List<float[]> otherFeatures = new List<float[]>();
        List<float[]> trainingData = new List<float[]>();
        List<int> labels = new List<int>();
        foreach (var userFile in userFiles)
        {
            float[] feature = VoiceFeaturesService.ExtractVoiceFeatures(userFile);
            userFeatures.Add(feature);
             
        }
        
        foreach (var otherFile in otherFiles)
        {
             
            float[] feature = VoiceFeaturesService.ExtractVoiceFeatures(otherFile);
            otherFeatures.Add(feature);
            
        }
        // Создание данных для обучения
        

        foreach (var feature in userFeatures)
        {
            trainingData.Add(feature);
            labels.Add(1); // Метка 1 - пользователь
        }

        foreach (var feature in otherFeatures)
        {
            trainingData.Add(feature);
            labels.Add(0); // Метка 0 - другие
        }
        
        var kernel = new Sigmoid(1.0,0.0); 
        
        // Снижение размерности данных до 2D с помощью PCA
       

        // Инициализация SVM с Гауссовым ядром
        var svm = new SupportVectorMachine<Sigmoid>(inputs: trainingData[0].Length, kernel);

        // Обучение модели SVM с помощью метода минимальной оптимизации
        var teacher = new SequentialMinimalOptimization<Sigmoid>()
        {
            Complexity = 1 // Параметр C
        };

        svm = teacher.Learn(trainingData.ToArray().ToDouble(), labels.ToArray());
        
        
        //VisualizeSvm(svm, transformedInputs, labels.ToArray(), "svm_visualization.png");
        
        var predictedLabels = svm.Decide(trainingData.ToArray().ToDouble());
        double accuracy = Accuracy.Calculate(labels.ToArray(), predictedLabels.ToInt32());

        // Создание матрицы ошибок
        var confusionMatrix = new ConfusionMatrix(labels.ToArray(), predictedLabels.ToInt32());

        return (svm, accuracy, confusionMatrix);

    }
        
    public SupportVectorMachine<Sigmoid> LoadModel(string modelFilePath)
    {
        // Загрузка модели из файла
        return Serializer.Load<SupportVectorMachine<Sigmoid>>(modelFilePath);
    }

    public List<double> VerifyVoiceByVoice(string audioFilePath, string modelFilePath)
    {

        // Загрузить модель из файла
        var svm = LoadModel(modelFilePath);

        // Извлечь MFCC признаки из аудиофайла
        var voiceFeatures = VoiceFeaturesService.ExtractVoiceFeatures(audioFilePath);
        if (voiceFeatures.Length < 8000)
        {
            return new List<double>(){0};
        }
        List<double> prediction = new List<double>();

        prediction.Add(svm.Probability(voiceFeatures.ToDouble()));
        var res = svm.Decide(voiceFeatures.ToDouble());
        Console.WriteLine(res);
        double rawOutput = svm.Compute(voiceFeatures.ToDouble());

        // Применяем сигмоидальную функцию или другое преобразование для получения вероятности
        double probability = Sigmoid(rawOutput); // Сигмоид преобразует значение в диапазон [0, 1]
        prediction.Add(probability);
        
            
        
        // Проверить, принадлежит ли голос пользователю
        

        return prediction; // 1 - голос пользователя, 0 - не пользователь
    }
    private double Sigmoid(double value)
    {
        return 1.0 / (1.0 + Math.Exp(-value)); // Сигмоидальная функция
    }
}
public class Accuracy
{
    public static double Calculate(int[] trueLabels, int[] predictedLabels)
    {
        int correct = 0;
        for (int i = 0; i < trueLabels.Length; i++)
        {
            if (trueLabels[i] == predictedLabels[i])
            {
                correct++;
            }
        }
        return (double)correct / trueLabels.Length;
    }
}

public class ConfusionMatrix
{
    public int[,] Matrix { get; private set; }

    public ConfusionMatrix(int[] trueLabels, int[] predictedLabels)
    {
        Matrix = new int[2, 2]; // For binary classification: [[TP, FP], [FN, TN]]
        for (int i = 0; i < trueLabels.Length; i++)
        {
            int actual = trueLabels[i];
            int predicted = predictedLabels[i];
            Matrix[actual, predicted]++;
        }
    }

    public override string ToString()
    {
        return $"TP: {Matrix[1, 1]}, FP: {Matrix[0, 1]}, FN: {Matrix[1, 0]}, TN: {Matrix[0, 0]}";
    }
}
