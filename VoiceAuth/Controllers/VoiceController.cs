using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;

namespace VoiceAuth;
[ApiController]
[Route("api/[controller]")]
public class VoiceController(IVoiceBLL _voiceBll): ControllerBase
{

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserVoice()
        {
            _voiceBll.TrainAndSaveModel();
            return Ok("Шаблон голоса успешно сохранён.");
        }

        [HttpPost("auth-voice")]
        public async Task<IActionResult> AuthenticateUserVoice([FromForm] IFormFile voiceFile)
        {
            List<double> accessGranted = new List<double>();
            
            
            if (voiceFile == null || voiceFile.Length == 0)
            {
                return BadRequest("Неверный аудиофайл.");
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "user_voice.wav");
            

            // Сохраняем файл
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await voiceFile.CopyToAsync(stream);
            }
            if (!VoiceDetector.ContainsVoice(filePath))
            {
                return BadRequest();
            }

            var thread = new Thread(() =>
            {
                accessGranted = _voiceBll.VerifyVoiceByVoice(filePath);
            },100000000);
            thread.Start();
            thread.Join();
            if (accessGranted[0] > 0.55)
            {
                return Ok(accessGranted);
            }

            return Unauthorized();

        }
}
public class VoiceDetector
    {
        public static bool ContainsVoice(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File {filePath} not found");

            // Load audio file
            using (var audioFileReader = new AudioFileReader(filePath))
            {
                float[] buffer = new float[audioFileReader.WaveFormat.SampleRate];
                int read;
                while ((read = audioFileReader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Check energy level to detect if there's sound
                    float energy = 0;
                    for (int i = 0; i < read; i++)
                    {
                        energy += buffer[i] * buffer[i];
                    }

                    energy /= read;
                    Console.WriteLine(energy);

                    // Threshold to determine if sound contains voice-like features
                    if (energy > 0.001f) // Adjust threshold as needed
                    {
                        
                        return true; // Voice detected
                    }
                }
            }

            return false; // No voice detected
        }
    }
