using System.Numerics;
using Accord.Math;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;
using NAudio.Wave;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Options;

namespace VoiceAuth.Services;

public class VoiceFeaturesService
{
     public float[] ExtractVoiceFeatures(string audioFilePath)
        {
            // Чтение аудиофайла
            using (var reader = new AudioFileReader(audioFilePath))
            {
                // Извлечение сэмплов из аудиофайла
                int sampleCount =
                    (int)(reader.Length /
                          (reader.WaveFormat.BitsPerSample / 8)); // Правильный расчет количества сэмплов
                float[]  samples= new float[sampleCount];

                
                
                reader.Read(samples, 0, samples.Length);
                var formatedSamples = TrimSilence(samples);
                // Инициализация MfccExtractor
                int sampleRate = reader.WaveFormat.SampleRate;
                var mfccOptions = new MfccOptions
                {
                    SamplingRate = sampleRate,
                    FeatureCount = 40,
                    FrameDuration = 0.025f/*sec*/,
                    HopDuration = 0.010f/*sec*/,
                    FilterBankSize = 50,
                    PreEmphasis = 0.87,
                    //...unspecified parameters will have default values 
                };
                var mfcc = new MfccExtractor(mfccOptions); // Предположим, что 13 коэффициентов MFCC — стандартное значение

                // Применение метода для вычисления признаков с использованием частоты дискретизации
                var mfccFeatures = mfcc.ComputeFrom(formatedSamples);
                


                // Проверяем, что есть хотя бы один фрейм с MFCC
                if (mfccFeatures != null && mfccFeatures.Count > 0)
                {
                    /*var (mean, variance) = CalculateAggregatedFeatures(mfccFeatures);
                    var spectralCentroidFeatures = ExtractSpectralCentroid(formatedSamples, sampleRate);
                    var spectralRolloffFeatures = AnalyzeAudioSpectralSlope(formatedSamples);
                    var features = mean.Concat(variance).Concat(spectralCentroidFeatures.ToFloats()).Concat(spectralRolloffFeatures.ToFloats()).ToArray();
                    //var aggregatedFeature = ConcatenateFeatures(mean, variance, spectralCentroidFeatures, spectralRolloffFeatures);*/
                    int frameLimit = Math.Min(200, mfccFeatures.Count); // Если меньше 20 фреймов, взять все доступные
                    var limitedFrames = mfccFeatures.Take(frameLimit);
                    var features  = limitedFrames.SelectMany(arr => arr).ToArray();
                    return features;
                }
                else
                {
                    // Возвращаем пустой массив, если коэффициенты не были извлечены
                    return new float[0];
                }
            }
        }

    
   
    public static float[] TrimSilence(float[] samples, float threshold = 0.001f)
    {
        List<float> nonSilenceIndexes = new List<float>();

        // Заполняем список индексами всех сэмплов, которые превышают порог
        for (int i = 0; i < samples.Length; i++)
        {
            if (Math.Abs(samples[i]) > threshold)
            {
                nonSilenceIndexes.Add(samples[i]);
            }
        }

        // Если все сэмплы тишина, возвращаем пустой массив
        if (nonSilenceIndexes.Count == 0)
        {
            return new float[0];
        }

        // Начальный и конечный индексы
        //int startIndex = nonSilenceIndexes.First();
        //int endIndex = nonSilenceIndexes.Last();

        // Возвращаем только те сэмплы, которые находятся между первым и последним индексами
        return nonSilenceIndexes.ToArray();
    }
     static (float[] mean, float[] variance)  CalculateAggregatedFeatures(List<float[]> features)
        {
            int frameLimit = Math.Min(20, features.Count); // Если меньше 20 фреймов, взять все доступные
            var limitedFrames = features.Take(frameLimit);
    
            // Результаты: массивы для средних и стандартных отклонений
            List<float> meanValues = new List<float>();
            List<float> stdDevValues = new List<float>();
    
            // Рассчитать среднее и стандартное отклонение для первых 20 фреймов
            foreach (var frame in limitedFrames)
            {
                // Среднее
                float mean = frame.Average();
                meanValues.Add(mean);
    
                // Стандартное отклонение
                float stdDev = (float)Math.Sqrt(frame.Average(v => Math.Pow(v - mean, 2)));
                stdDevValues.Add(stdDev);
            }
    
            return (meanValues.ToArray(), stdDevValues.ToArray());
        }
    
 
    
    List<float[]> SelectFeaturesByVariance(List<float[]> data, float threshold = 0.9f)
    {
        int featureCount = data[0].Length;
        float[] variances = new float[featureCount];

        // Рассчёт среднего значения каждого признака
        float[] means = new float[featureCount];
        foreach (var row in data)
        {
            for (int i = 0; i < featureCount; i++)
            {
                means[i] += row[i];
            }
        }
        for (int i = 0; i < featureCount; i++)
        {
            means[i] /= data.Count;
        }

        // Рассчёт дисперсии
        foreach (var row in data)
        {
            for (int i = 0; i < featureCount; i++)
            {
                variances[i] += (row[i] - means[i]) * (row[i] - means[i]);
            }
        }
        for (int i = 0; i < featureCount; i++)
        {
            variances[i] /= data.Count;
        }

        // Отбор признаков
        List<float[]> selectedFeatures = new List<float[]>();
        for (int i = 0; i < featureCount; i++)
        {
            if (variances[i] >= threshold)
            {
                selectedFeatures.Add(data[i]);
            }
        }

        return selectedFeatures;
    }
    
        private static double[] ExtractSpectralCentroid(float[] audioData, int sampleRate)
    {
        int frameSize = 1024;
        int hopSize = 512;
        var centroids = new List<double>();

        var frames = GetFrames(audioData, frameSize, hopSize);
        foreach (var frame in frames)
        {
            var fft = new Complex[frame.Length];
            for (int i = 0; i < frame.Length; i++)
                fft[i] = new Complex(frame[i], 0);
            Fourier.Forward(fft, FourierOptions.Matlab);

            var magnitudes = fft.Select(c => c.Magnitude).ToArray();
            var freqs = Enumerable.Range(0, frameSize / 2).Select(i => i * sampleRate / frameSize).ToArray();

            double centroid = freqs.Zip(magnitudes, (f, m) => f * m).Sum() / magnitudes.Sum();
            centroids.Add(centroid);
        }

        double[] spec = new double[] { centroids.Mean(), centroids.StandardDeviation(), centroids.Skewness() };

        return spec;
    }

    public static double[] AnalyzeAudioSpectralSlope(float[] audioSamples)
    {
        // Загружаем аудио

        // Вычисляем спектр
        // Используем MathNet.Numerics для FFT
        int fftSize = 1024;
        int fftCount = audioSamples.Length / fftSize;
        List<double[]> spectralMagnitudes = new List<double[]>();
        double sampleRate = 44100; // Пример: замените на частоту дискретизации файла

        for (int i = 0; i < fftCount; i++)
        {
            var segment = audioSamples.Skip(i * fftSize).Take(fftSize).Select(x => (double)x).ToArray();
            Complex[] fft = segment.Select(x => new Complex(x, 0)).ToArray();
            Fourier.Forward(fft, FourierOptions.Matlab);
            spectralMagnitudes.Add(fft.Select(c => c.Magnitude).ToArray());
        }

        // Среднее значение спектра
        double[] averageSpectrum = new double[fftSize / 2];
        for (int i = 0; i < fftSize / 2; i++)
        {
            averageSpectrum[i] = spectralMagnitudes.Average(spectrum => spectrum[i]);
        }

        // Частоты (до Nyquist частоты)
        double[] frequencies = Enumerable.Range(0, fftSize / 2)
                                         .Select(bin => bin * sampleRate / fftSize)
                                         .ToArray();

        // Рассчитываем спектральный спад
        var (mean, stdDev) = CalculateSpectralSlopeStatistics(frequencies, averageSpectrum);
        return new double[]{mean, stdDev};
    }

    public static (double Mean, double StandardDeviation) CalculateSpectralSlopeStatistics(double[] frequencies, double[] magnitudes)
    {
        if (frequencies == null || magnitudes == null)
            throw new ArgumentNullException("Frequencies and magnitudes cannot be null.");

        if (frequencies.Length != magnitudes.Length || frequencies.Length == 0)
            throw new ArgumentException("Frequencies and magnitudes must have the same non-zero length.");

        double[] slopes = new double[frequencies.Length - 1];
        for (int i = 0; i < frequencies.Length - 1; i++)
        {
            double deltaFreq = frequencies[i + 1] - frequencies[i];
            double deltaMag = magnitudes[i + 1] - magnitudes[i];

            if (Math.Abs(deltaFreq) < 1e-10)
                throw new InvalidOperationException("Frequency values must be distinct.");

            slopes[i] = deltaMag / deltaFreq;
        }

        double mean = slopes.Average();
        double variance = slopes.Select(slope => Math.Pow(slope - mean, 2)).Average();
        double standardDeviation = Math.Sqrt(variance);

        return (mean, standardDeviation);
    }

    private static List<float[]> GetFrames(float[] audioData, int frameSize, int hopSize)
    {
        var frames = new List<float[]>();
        for (int i = 0; i < audioData.Length - frameSize; i += hopSize)
        {
            frames.Add(audioData.Skip(i).Take(frameSize).ToArray());
        }
        return frames;
    }

}