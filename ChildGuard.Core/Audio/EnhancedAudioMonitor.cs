using System;
using System.Diagnostics;
using System.IO;
using System.Speech.Recognition;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ChildGuard.Core.Detection;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace ChildGuard.Core.Audio;

/// <summary>
/// Enhanced audio monitor with real-time speech recognition and bad words detection
/// </summary>
public class EnhancedAudioMonitor : IDisposable
{
    private readonly BadWordsDetector _detector;
    private readonly string _ffmpegPath;
    private readonly string _outputPath;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isMonitoring;
    private SpeechRecognitionEngine _speechEngine;
    private WaveInEvent _waveIn;
    private Process _ffmpegProcess;
    private readonly List<string> _detectedPhrases;
    private readonly object _lockObject = new object();
    
    // Events
    public event EventHandler<AudioDetectionEventArgs> OnBadWordDetected;
    public event EventHandler<SpeechDetectedEventArgs> OnSpeechDetected;
    public event EventHandler<AudioLevelEventArgs> OnAudioLevelChanged;
    public event EventHandler<string> OnStatusChanged;
    
    // Configuration
    public bool UseWindowsSpeechRecognition { get; set; } = true;
    public bool UseFFmpegCapture { get; set; } = false;
    public int AudioBufferSeconds { get; set; } = 30;
    public float VolumeThreshold { get; set; } = 0.3f;
    
    public EnhancedAudioMonitor(string ffmpegPath = null)
    {
        _detector = new BadWordsDetector();
        _ffmpegPath = ffmpegPath ?? FindFFmpeg();
        _outputPath = Path.Combine(Path.GetTempPath(), "ChildGuard", "audio");
        Directory.CreateDirectory(_outputPath);
        _detectedPhrases = new List<string>();
        
        InitializeSpeechRecognition();
    }
    
    private string FindFFmpeg()
    {
        // Try to find FFmpeg in common locations
        var possiblePaths = new[]
        {
            "ffmpeg.exe",
            Path.Combine(Environment.CurrentDirectory, "ffmpeg.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin", "ffmpeg.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ffmpeg", "bin", "ffmpeg.exe"),
            @"C:\ffmpeg\bin\ffmpeg.exe"
        };
        
        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }
        
        return "ffmpeg.exe"; // Default, hope it's in PATH
    }
    
    private void InitializeSpeechRecognition()
    {
        try
        {
            if (UseWindowsSpeechRecognition)
            {
                _speechEngine = new SpeechRecognitionEngine();
                _speechEngine.SetInputToDefaultAudioDevice();
                
                // Create grammar with dictation and custom words
                var dictation = new DictationGrammar();
                _speechEngine.LoadGrammar(dictation);
                
                // Add event handlers
                _speechEngine.SpeechRecognized += OnSpeechRecognized;
                _speechEngine.SpeechDetected += OnSpeechDetectedInternal;
                _speechEngine.AudioLevelUpdated += OnAudioLevelUpdated;
                
                OnStatusChanged?.Invoke(this, "Speech recognition initialized");
            }
        }
        catch (Exception ex)
        {
            OnStatusChanged?.Invoke(this, $"Failed to initialize speech recognition: {ex.Message}");
            UseWindowsSpeechRecognition = false;
        }
    }
    
    public async Task StartMonitoringAsync()
    {
        if (_isMonitoring)
            return;
            
        _isMonitoring = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        OnStatusChanged?.Invoke(this, "Starting audio monitoring...");
        
        // Start speech recognition if available
        if (UseWindowsSpeechRecognition && _speechEngine != null)
        {
            try
            {
                _speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                OnStatusChanged?.Invoke(this, "Windows Speech Recognition started");
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"Speech recognition error: {ex.Message}");
            }
        }
        
        // Start NAudio capture for audio level monitoring
        StartNAudioCapture();
        
        // Start FFmpeg capture if needed
        if (UseFFmpegCapture && !string.IsNullOrEmpty(_ffmpegPath))
        {
            await Task.Run(() => MonitorWithFFmpeg(_cancellationTokenSource.Token));
        }
    }
    
    private void StartNAudioCapture()
    {
        try
        {
            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 1),
                BufferMilliseconds = 100
            };
            
            _waveIn.DataAvailable += OnAudioDataAvailable;
            _waveIn.StartRecording();
            
            OnStatusChanged?.Invoke(this, "NAudio capture started");
        }
        catch (Exception ex)
        {
            OnStatusChanged?.Invoke(this, $"NAudio error: {ex.Message}");
        }
    }
    
    private void OnAudioDataAvailable(object sender, WaveInEventArgs e)
    {
        // Calculate audio level
        float maxValue = 0;
        for (int i = 0; i < e.BytesRecorded; i += 2)
        {
            short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
            float sampleValue = sample / 32768f;
            maxValue = Math.Max(maxValue, Math.Abs(sampleValue));
        }
        
        OnAudioLevelChanged?.Invoke(this, new AudioLevelEventArgs 
        { 
            Level = maxValue,
            Timestamp = DateTime.Now
        });
        
        // Detect loud noises
        if (maxValue > VolumeThreshold)
        {
            OnStatusChanged?.Invoke(this, $"Audio level: {maxValue:F2}");
        }
    }
    
    private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        if (e.Result.Confidence < 0.5) // Skip low confidence results
            return;
            
        var text = e.Result.Text;
        
        OnSpeechDetected?.Invoke(this, new SpeechDetectedEventArgs
        {
            Text = text,
            Confidence = e.Result.Confidence,
            Timestamp = DateTime.Now
        });
        
        // Check for bad words
        var result = _detector.Analyze(text);
        if (!result.IsClean)
        {
            OnBadWordDetected?.Invoke(this, new AudioDetectionEventArgs
            {
                Timestamp = DateTime.Now,
                TranscribedText = text,
                DetectionResult = result,
                AudioFilePath = ""
            });
            
            lock (_lockObject)
            {
                _detectedPhrases.Add($"[{DateTime.Now:HH:mm:ss}] {text} - Detected: {string.Join(", ", result.DetectedWords)}");
            }
        }
    }
    
    private void OnSpeechDetectedInternal(object sender, System.Speech.Recognition.SpeechDetectedEventArgs e)
    {
        OnStatusChanged?.Invoke(this, "Speech detected, listening...");
    }
    
    private void OnAudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
    {
        // Audio level from speech engine
        if (e.AudioLevel > 50) // 0-100 scale
        {
            OnAudioLevelChanged?.Invoke(this, new AudioLevelEventArgs 
            { 
                Level = e.AudioLevel / 100f,
                Timestamp = DateTime.Now
            });
        }
    }
    
    private async Task MonitorWithFFmpeg(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var audioFile = await CaptureAudioWithFFmpeg(AudioBufferSeconds);
                
                if (!string.IsNullOrEmpty(audioFile) && File.Exists(audioFile))
                {
                    // Process captured audio
                    await ProcessAudioFile(audioFile);
                    
                    // Clean up
                    try { File.Delete(audioFile); } catch { }
                }
                
                await Task.Delay(5000, cancellationToken);
            }
            catch (Exception ex)
            {
                OnStatusChanged?.Invoke(this, $"FFmpeg monitoring error: {ex.Message}");
            }
        }
    }
    
    private async Task<string> CaptureAudioWithFFmpeg(int durationSeconds)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var outputFile = Path.Combine(_outputPath, $"capture_{timestamp}.wav");
        
        try
        {
            // Try to capture system audio (Windows WASAPI)
            var captureArgs = GetFFmpegCaptureArgs(outputFile, durationSeconds);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = captureArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using (var process = Process.Start(startInfo))
            {
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0 && File.Exists(outputFile))
                {
                    OnStatusChanged?.Invoke(this, $"Audio captured: {outputFile}");
                    return outputFile;
                }
            }
        }
        catch (Exception ex)
        {
            OnStatusChanged?.Invoke(this, $"FFmpeg capture error: {ex.Message}");
        }
        
        return null;
    }
    
    private string GetFFmpegCaptureArgs(string outputFile, int duration)
    {
        // Try different audio capture methods based on Windows
        // First try: DirectShow (most compatible)
        return $"-f dshow -i audio=\"Stereo Mix (Realtek(R) Audio)\" -t {duration} " +
               $"-acodec pcm_s16le -ar 16000 -ac 1 \"{outputFile}\"";
        
        // Alternative: Use default microphone
        // return $"-f dshow -i audio=\"Microphone\" -t {duration} " +
        //        $"-acodec pcm_s16le -ar 16000 -ac 1 \"{outputFile}\"";
    }
    
    private async Task ProcessAudioFile(string audioFile)
    {
        // This would use a proper speech-to-text service
        // For now, just indicate the file was processed
        OnStatusChanged?.Invoke(this, $"Processing audio file: {Path.GetFileName(audioFile)}");
        
        // Simulate processing
        await Task.Delay(1000);
    }
    
    public void Stop()
    {
        if (!_isMonitoring)
            return;
            
        _isMonitoring = false;
        _cancellationTokenSource?.Cancel();
        
        // Stop speech recognition
        if (_speechEngine != null)
        {
            try
            {
                _speechEngine.RecognizeAsyncStop();
            }
            catch { }
        }
        
        // Stop NAudio
        if (_waveIn != null)
        {
            _waveIn.StopRecording();
            _waveIn.Dispose();
            _waveIn = null;
        }
        
        // Stop FFmpeg
        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
        {
            _ffmpegProcess.Kill();
            _ffmpegProcess.Dispose();
            _ffmpegProcess = null;
        }
        
        OnStatusChanged?.Invoke(this, "Audio monitoring stopped");
    }
    
    public List<string> GetDetectedPhrases()
    {
        lock (_lockObject)
        {
            return new List<string>(_detectedPhrases);
        }
    }
    
    public void ClearDetectedPhrases()
    {
        lock (_lockObject)
        {
            _detectedPhrases.Clear();
        }
    }
    
    public void SetCustomBadWords(string[] words)
    {
        foreach (var word in words)
        {
            _detector.AddCustomWord(word);
        }
        OnStatusChanged?.Invoke(this, $"Added {words.Length} custom bad words");
    }
    
    public void Dispose()
    {
        Stop();
        _speechEngine?.Dispose();
        _waveIn?.Dispose();
        _ffmpegProcess?.Dispose();
    }
}

