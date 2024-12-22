using Accord.Math;
using NAudio.Wave;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Options;

namespace VoiceAuth;

using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using System.Collections.Generic;
using System.IO;

public class VoiceAuthenticationSystem
{
    // Метод для обучения модели SVM для одного пользователя
    public SupportVectorMachine<Gaussian> TrainUserModel(string[] userFiles, string[] otherFiles)
    {
        // Извлечение MFCC признаков для пользователя и других
        List<Double[]> userFeatures = new List<double[]>();
        List<Double[]> otherFeatures = new List<double[]>();
        foreach (var userFile in userFiles)
        {
            userFeatures.Add(ExtractVoiceFeatures(userFile)) ; 
        }
        
        foreach (var otherFile in otherFiles)
        {
            otherFeatures.Add(ExtractVoiceFeatures(otherFile)) ; 
        }
        
       
        


        // Создание данных для обучения
        List<double[]> trainingData = new List<double[]>();
        List<int> labels = new List<int>();

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
        
        var kernel = new Gaussian(); 

        // Инициализация SVM с Гауссовым ядром
        var svm = new SupportVectorMachine<Gaussian>(inputs: trainingData[0].Length, kernel);

        // Обучение модели SVM с помощью метода минимальной оптимизации
        var teacher = new SequentialMinimalOptimization<Gaussian>()
        {
            Complexity = 100 // Параметр C
        };

        svm = teacher.Learn(trainingData.ToArray(), labels.ToArray());

        return svm; // Вернуть обученную модель
    }

    // Вспомогательный метод для извлечения MFCC признаков (предполагается, что он реализован)
    public double[] ExtractVoiceFeatures(string audioFilePath)
        {
            // Чтение аудиофайла
            using (var reader = new AudioFileReader(audioFilePath))
            {
                // Извлечение сэмплов из аудиофайла
                int sampleCount =
                    (int)(reader.Length /
                          (reader.WaveFormat.BitsPerSample / 8)); // Правильный расчет количества сэмплов
                float[] samples = new float[sampleCount];
                reader.Read(samples, 0, samples.Length);
                

                // Инициализация MfccExtractor
                int sampleRate = reader.WaveFormat.SampleRate;
                
                
                var mfccOptions = new MfccOptions
                {
                    SamplingRate = sampleRate,
                    FeatureCount = 300,
                    FrameDuration = 0.001/*sec*/,
                    HopDuration = 0.0005/*sec*/,
                    FilterBankSize = 500,
                    PreEmphasis = 0.87,
                    //...unspecified parameters will have default values 
                };
                
                
                
                var mfcc = new MfccExtractor(mfccOptions); // Предположим, что 13 коэффициентов MFCC — стандартное значение

                // Применение метода для вычисления признаков с использованием частоты дискретизации
                var mfccFeatures = mfcc.ComputeFrom(samples);

                // Проверяем, что есть хотя бы один фрейм с MFCC
                if (mfccFeatures != null && mfccFeatures.Count > 0)
                {
                    //foreach (var VARIABLE in mfccFeatures[0].ToDouble())
                    //{
                    //    Console.Write(VARIABLE);
                    //}
                    // Возвращаем первый набор коэффициентов MFCC
                    return mfccFeatures[0].ToDouble();
                }
                else
                {
                    // Возвращаем пустой массив, если коэффициенты не были извлечены
                    return new double[0];
                }
            }
        }
}
