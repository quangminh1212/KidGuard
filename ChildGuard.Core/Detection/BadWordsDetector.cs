using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChildGuard.Core.Detection;

public class BadWordsDetector
{
    private readonly HashSet<string> _badWords;
    private readonly HashSet<string> _badPhrases;
    private readonly List<Regex> _patterns;
    
    public BadWordsDetector()
    {
        // Khởi tạo danh sách từ ngữ không phù hợp (tiếng Việt)
        _badWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Thêm các từ cần lọc ở đây
            // Tạm thời dùng các từ mẫu
            "violence", "drug", "weapon",
            // Các từ tiếng Việt
            "bạo lực", "ma túy", "vũ khí", "cờ bạc"
        };
        
        _badPhrases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Các cụm từ cần lọc
            "nội dung xấu",
            "không phù hợp"
        };
        
        // Patterns cho các biến thể
        _patterns = new List<Regex>
        {
            new Regex(@"\b(hack|crack|cheat)\b", RegexOptions.IgnoreCase),
            new Regex(@"\b(xxx|porn|adult)\b", RegexOptions.IgnoreCase)
        };
    }
    
    public DetectionResult Analyze(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new DetectionResult { IsClean = true };
        
        var result = new DetectionResult();
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        // Check individual words
        foreach (var word in words)
        {
            if (_badWords.Contains(word))
            {
                result.FoundWords.Add(word);
            }
        }
        
        // Check phrases
        foreach (var phrase in _badPhrases)
        {
            if (text.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.DetectedPhrases.Add(phrase);
            }
        }
        
        // Check patterns
        foreach (var pattern in _patterns)
        {
            var matches = pattern.Matches(text);
            foreach (Match match in matches)
            {
                result.DetectedPatterns.Add(match.Value);
            }
        }
        
        result.IsClean = !result.HasDetections;
        result.Severity = CalculateSeverity(result);
        
        return result;
    }
    
    private DetectionSeverity CalculateSeverity(DetectionResult result)
    {
        var count = result.FoundWords.Count + 
                   result.DetectedPhrases.Count + 
                   result.DetectedPatterns.Count;
        
        if (count == 0) return DetectionSeverity.None;
        if (count <= 2) return DetectionSeverity.Low;
        if (count <= 5) return DetectionSeverity.Medium;
        return DetectionSeverity.High;
    }
    
    public void AddCustomWord(string word)
    {
        _badWords.Add(word);
    }
    
    public void AddCustomPhrase(string phrase)
    {
        _badPhrases.Add(phrase);
    }
    
    public void LoadFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _badWords.Add(line.Trim());
                }
            }
        }
    }
}

public class DetectionResult
{
    public bool IsClean { get; set; } = true;
    public int Score { get; set; }
    public List<string> Violations { get; set; } = new();
    public List<string> FoundWords { get; set; } = new();
    public List<string> DetectedWords { get; set; } = new(); // Alias for compatibility
    public DetectionSeverity Severity { get; set; } = DetectionSeverity.None;
    public List<string> DetectedPhrases { get; set; } = new List<string>();
    public List<string> DetectedPatterns { get; set; } = new List<string>();
    
    public bool HasDetections => 
        DetectedWords.Any() || DetectedPhrases.Any() || DetectedPatterns.Any();
    
    public string GetSummary()
    {
        if (IsClean)
            return "Nội dung an toàn";
        
        return $"Phát hiện {DetectedWords.Count + DetectedPhrases.Count + DetectedPatterns.Count} nội dung không phù hợp - Mức độ: {Severity}";
    }
}

public enum SeverityLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}

public enum DetectionSeverity
{
    None,
    Low,
    Medium,
    High
}
