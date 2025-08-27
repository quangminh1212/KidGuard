using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChildGuard.Core.Detection;

namespace ChildGuard.Core.Audio;

public class AudioMonitor
{
    private Process _ffmpegProcess;
    private readonly BadWordsDetector _detector;
    private readonly string _ffmpegPath;
    private readonly string _outputPath;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isMonitoring;
    
    public event EventHandler<AudioDetectionEventArgs> OnDetection;
    public event EventHandler<SpeechDetectedEventArgs> OnSpeechDetected;
    public event EventHandler<AudioLevelEventArgs> OnLoudNoiseDetected;
    
    public AudioMonitor(string ffmpegPath = "ffmpeg.exe")
    {
        _detector = new BadWordsDetector();
        _ffmpegPath = ffmpegPath;
        _outputPath = Path.Combine(Path.GetTempPath(), "ChildGuard", "audio");
        Directory.CreateDirectory(_outputPath);
    }
    
    public async Task StartMonitoringAsync()
    {
        if (_isMonitoring)
            return;
            
        _isMonitoring = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        await Task.Run(() => MonitorAudioLoop(_cancellationTokenSource.Token));
    }
    
    public void Start()
    {
        _ = StartMonitoringAsync();
    }
    
    public void Stop()
    {
        StopMonitoring();
    }
    
    public void StopMonitoring()
    {
        _isMonitoring = false;
        _cancellationTokenSource?.Cancel();
        
        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
        {
            _ffmpegProcess.Kill();
            _ffmpegProcess.Dispose();
        }
    }
    
    public void Dispose()
    {
        StopMonitoring();
    }
    
    private void MonitorAudioLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Capture audio for 30 seconds chunks
                var audioFile = CaptureAudio(30);
                
                if (File.Exists(audioFile))
                {
                    // Convert audio to text using FFmpeg + speech recognition
                    var text = ConvertAudioToText(audioFile);
                    
                    if (!string.IsNullOrEmpty(text))
                    {
                        // Analyze text for inappropriate content
                        var result = _detector.Analyze(text);
                        
                        if (!result.IsClean)
                        {
                            OnDetection?.Invoke(this, new AudioDetectionEventArgs
                            {
                                Timestamp = DateTime.Now,
                                TranscribedText = text,
                                DetectionResult = result,
                                AudioFilePath = audioFile
                            });
                        }
                    }
                    
                    // Clean up old audio file
                    try { File.Delete(audioFile); } catch { }
                }
                
                // Wait before next capture
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audio monitoring error: {ex.Message}");
            }
        }
    }
    
    private string CaptureAudio(int durationSeconds)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var outputFile = Path.Combine(_outputPath, $"audio_{timestamp}.wav");
        
        try
        {
            // Use FFmpeg to capture audio from default microphone
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-f dshow -i audio=\"Microphone\" -t {durationSeconds} -acodec pcm_s16le -ar 16000 -ac 1 \"{outputFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            _ffmpegProcess = Process.Start(startInfo);
            _ffmpegProcess.WaitForExit(durationSeconds * 1000 + 5000); // Add 5 sec buffer
            
            if (_ffmpegProcess.ExitCode == 0 && File.Exists(outputFile))
            {
                return outputFile;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio capture error: {ex.Message}");
        }
        
        return null;
    }
    
    private string ConvertAudioToText(string audioFile)
    {
        // Simplified version - In production, use proper speech-to-text API
        // Options: Google Speech API, Azure Cognitive Services, IBM Watson
        
        try
        {
            // For demo purposes, extract audio metadata
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{audioFile}\" -f ffmetadata -",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using (var process = Process.Start(startInfo))
            {
                var output = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                // In production, this would return actual transcribed text
                // For now, return a placeholder
                return ExtractSimpleMetadata(output);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio to text error: {ex.Message}");
        }
        
        return string.Empty;
    }
    
    private string ExtractSimpleMetadata(string ffmpegOutput)
    {
        // Extract basic info from FFmpeg output
        // In production, replace with actual speech-to-text
        if (ffmpegOutput.Contains("Duration:"))
        {
            return "Audio captured successfully";
        }
        return string.Empty;
    }
    
    public void SetCustomBadWords(string[] words)
    {
        foreach (var word in words)
        {
            _detector.AddCustomWord(word);
        }
    }
}


// Speech-to-text service interface for future implementation
public interface ISpeechToTextService
{
    Task<string> TranscribeAsync(string audioFilePath);
    Task<string> TranscribeStreamAsync(Stream audioStream);
}

// Example implementation using Windows Speech Recognition
public class WindowsSpeechService : ISpeechToTextService
{
    public async Task<string> TranscribeAsync(string audioFilePath)
    {
        // This would use System.Speech.Recognition or 
        // Microsoft.CognitiveServices.Speech SDK
        await Task.Delay(100); // Placeholder
        return "Transcribed text placeholder";
    }
    
    public async Task<string> TranscribeStreamAsync(Stream audioStream)
    {
        await Task.Delay(100); // Placeholder
        return "Transcribed stream text placeholder";
    }
}
