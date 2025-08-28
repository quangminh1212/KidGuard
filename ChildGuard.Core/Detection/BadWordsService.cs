using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChildGuard.Core.Events;
using ChildGuard.Core.Models;

namespace ChildGuard.Core.Detection
{
    /// <summary>
    /// Service phát hiện từ nhạy cảm sử dụng thuật toán Aho-Corasick
    /// </summary>
    public class BadWordsService : IEventSource, IDisposable
    {
        private SimpleTrie _trie;
        private readonly Dictionary<string, BadWord> _badWords;
        private readonly object _lockObject = new object();
        private bool _isEnabled = true;
        private int _minWordLength = 3;
        private bool _checkWholeWords = true;
        
        // Event source implementation
        public string SourceName => "BadWordsService";
        public IEventDispatcher? Dispatcher { get; set; }
        
        // Events
        public event EventHandler<BadWordDetectedEventArgs>? WordDetected;
        
        // Configuration
        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set => _isEnabled = value; 
        }
        
        public int MinWordLength 
        { 
            get => _minWordLength; 
            set => _minWordLength = Math.Max(1, value); 
        }
        
        public bool CheckWholeWords 
        { 
            get => _checkWholeWords; 
            set => _checkWholeWords = value; 
        }
        
        public BadWordsService()
        {
            _trie = new SimpleTrie();
            _badWords = new Dictionary<string, BadWord>(StringComparer.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Load bad words từ file
        /// </summary>
        public async Task LoadFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Bad words file not found: {filePath}");
            }
            
            var words = new List<BadWord>();
            var lines = await File.ReadAllLinesAsync(filePath);
            
            foreach (var line in lines)
            {
                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;
                
                var parts = line.Split('|');
                if (parts.Length >= 1)
                {
                    var word = parts[0].Trim().ToLowerInvariant();
                    var severity = parts.Length >= 2 && int.TryParse(parts[1], out var sev) ? sev : 2;
                    
                    if (!string.IsNullOrEmpty(word) && word.Length >= _minWordLength)
                    {
                        words.Add(new BadWord 
                        { 
                            Word = word, 
                            Severity = severity,
                            Category = DetermineCategory(word)
                        });
                    }
                }
            }
            
            LoadWords(words);
        }
        
        /// <summary>
        /// Load bad words từ list
        /// </summary>
        public void LoadWords(IEnumerable<BadWord> words)
        {
            lock (_lockObject)
            {
                _badWords.Clear();
                _trie.Clear();
                
                foreach (var word in words)
                {
                    var key = word.Word.ToLowerInvariant();
                    _badWords[key] = word;
                    _trie.Add(key);
                    
                    // Add variations (leet speak, spaces removed, etc.)
                    var variations = GenerateVariations(key);
                    foreach (var variation in variations)
                    {
                        if (!_badWords.ContainsKey(variation))
                        {
                            _trie.Add(variation);
                            _badWords[variation] = word;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Check text for bad words
        /// </summary>
        public BadWordDetectionResult CheckText(string text, string? source = null, string? windowTitle = null)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(text))
            {
                return new BadWordDetectionResult { IsClean = true };
            }
            
            var normalizedText = NormalizeText(text);
            var detectedWords = new List<DetectedBadWord>();
            
            lock (_lockObject)
            {
                var matches = _trie.FindAllMatches(normalizedText);
                
                foreach (var match in matches)
                {
                    var keyword = match.Word;
                    var position = match.Position;
                    
                    // Check if whole word matching is required
                    if (_checkWholeWords)
                    {
                        if (!IsWholeWord(normalizedText, position, keyword.Length))
                            continue;
                    }
                    
                    if (_badWords.TryGetValue(keyword, out var badWord))
                    {
                        // Get context around the bad word
                        var context = GetContext(text, position, 50);
                        
                        detectedWords.Add(new DetectedBadWord
                        {
                            Word = badWord.Word,
                            OriginalWord = ExtractOriginalWord(text, position, keyword.Length),
                            Position = position,
                            Severity = badWord.Severity,
                            Category = badWord.Category,
                            Context = context
                        });
                        
                        // Fire event for each detected word
                        OnWordDetected(badWord, context, source, windowTitle);
                    }
                }
            }
            
            return new BadWordDetectionResult
            {
                IsClean = detectedWords.Count == 0,
                DetectedWords = detectedWords,
                MaxSeverity = detectedWords.Count > 0 ? detectedWords.Max(w => w.Severity) : 0,
                TotalCount = detectedWords.Count
            };
        }
        
        /// <summary>
        /// Check text asynchronously
        /// </summary>
        public Task<BadWordDetectionResult> CheckTextAsync(string text, string? source = null, string? windowTitle = null)
        {
            return Task.Run(() => CheckText(text, source, windowTitle));
        }
        
        /// <summary>
        /// Add word dynamically
        /// </summary>
        public void AddWord(string word, int severity = 2, string? category = null)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length < _minWordLength)
                return;
            
            lock (_lockObject)
            {
                var badWord = new BadWord
                {
                    Word = word.ToLowerInvariant(),
                    Severity = severity,
                    Category = category ?? DetermineCategory(word)
                };
                
                _badWords[badWord.Word] = badWord;
                
                // Rebuild trie
                var allWords = _badWords.Values.ToList();
                LoadWords(allWords);
            }
        }
        
        /// <summary>
        /// Remove word
        /// </summary>
        public void RemoveWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;
            
            lock (_lockObject)
            {
                var key = word.ToLowerInvariant();
                if (_badWords.Remove(key))
                {
                    // Rebuild trie
                    var allWords = _badWords.Values.ToList();
                    LoadWords(allWords);
                }
            }
        }
        
        /// <summary>
        /// Get all loaded bad words
        /// </summary>
        public IReadOnlyList<BadWord> GetWords()
        {
            lock (_lockObject)
            {
                return _badWords.Values.ToList();
            }
        }
        
        /// <summary>
        /// Clear all words
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _badWords.Clear();
                _trie.Clear();
            }
        }
        
        
        private IEnumerable<string> GenerateVariations(string word)
        {
            var variations = new List<string>();
            
            // Leet speak variations
            var leetSpeak = word
                .Replace('a', '@')
                .Replace('e', '3')
                .Replace('i', '1')
                .Replace('o', '0')
                .Replace('s', '$');
            
            if (leetSpeak != word)
                variations.Add(leetSpeak);
            
            // With spaces between characters
            var spaced = string.Join(" ", word.ToCharArray());
            variations.Add(spaced.Replace(" ", ""));
            
            // With dots/dashes
            variations.Add(word.Replace(' ', '.'));
            variations.Add(word.Replace(' ', '-'));
            variations.Add(word.Replace(' ', '_'));
            
            return variations.Distinct();
        }
        
        private string NormalizeText(string text)
        {
            // Convert to lowercase and remove diacritics
            var normalized = text.ToLowerInvariant();
            
            // Remove Vietnamese diacritics
            normalized = RemoveVietnameseDiacritics(normalized);
            
            // Remove special characters for matching (but keep spaces for word boundaries)
            normalized = Regex.Replace(normalized, @"[^\w\s]", " ");
            
            return normalized;
        }
        
        private string RemoveVietnameseDiacritics(string text)
        {
            string[] vietnameseChars = new string[]
            {
                "àáạảãâầấậẩẫăằắặẳẵ",
                "èéẹẻẽêềếệểễ",
                "ìíịỉĩ",
                "òóọỏõôồốộổỗơờớợởỡ",
                "ùúụủũưừứựửữ",
                "ỳýỵỷỹ",
                "đ"
            };
            
            string[] replacementChars = new string[]
            {
                "a", "e", "i", "o", "u", "y", "d"
            };
            
            for (int i = 0; i < vietnameseChars.Length; i++)
            {
                foreach (char c in vietnameseChars[i])
                {
                    text = text.Replace(c.ToString(), replacementChars[i]);
                }
            }
            
            return text;
        }
        
        private bool IsWholeWord(string text, int position, int length)
        {
            // Check if the match is a whole word
            bool startOk = position == 0 || !char.IsLetterOrDigit(text[position - 1]);
            bool endOk = position + length >= text.Length || !char.IsLetterOrDigit(text[position + length]);
            
            return startOk && endOk;
        }
        
        private string ExtractOriginalWord(string text, int position, int length)
        {
            if (position < 0 || position + length > text.Length)
                return string.Empty;
            
            return text.Substring(position, Math.Min(length, text.Length - position));
        }
        
        private string GetContext(string text, int position, int contextLength)
        {
            var start = Math.Max(0, position - contextLength);
            var end = Math.Min(text.Length, position + contextLength);
            var context = text.Substring(start, end - start);
            
            // Add ellipsis if truncated
            if (start > 0) context = "..." + context;
            if (end < text.Length) context = context + "...";
            
            return context;
        }
        
        private string DetermineCategory(string word)
        {
            // Simple category detection based on keywords
            if (ContainsAny(word, "sex", "porn", "nude", "xxx", "hentai"))
                return "Adult";
            
            if (ContainsAny(word, "kill", "murder", "suicide", "bomb", "gun", "weapon", "giết", "chết"))
                return "Violence";
            
            if (ContainsAny(word, "drug", "cocaine", "heroin", "meth", "weed", "ma túy", "thuốc phiện"))
                return "Drugs";
            
            if (ContainsAny(word, "fuck", "shit", "bitch", "địt", "đụ", "lồn"))
                return "Profanity";
            
            return "General";
        }
        
        private bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
        }
        
        private void OnWordDetected(BadWord word, string context, string? source, string? windowTitle)
        {
            var args = new BadWordDetectedEventArgs
            {
                Word = word,
                Context = context,
                Source = source ?? "Unknown",
                WindowTitle = windowTitle ?? "Unknown",
                Timestamp = DateTime.UtcNow
            };
            
            // Fire local event
            WordDetected?.Invoke(this, args);
            
            // Publish to event bus
            if (Dispatcher != null)
            {
                var severity = word.Severity switch
                {
                    1 => EventSeverity.Low,
                    2 => EventSeverity.Medium,
                    3 => EventSeverity.High,
                    _ => EventSeverity.Medium
                };
                
                var evt = new BadWordDetectedEvent(
                    word.Word,
                    context,
                    windowTitle ?? "Unknown",
                    source ?? "Unknown",
                    severity
                );
                
                Dispatcher.PublishAsync(evt);
            }
        }
        
        public void Dispose()
        {
            Clear();
        }
    }
    
    /// <summary>
    /// Bad word model
    /// </summary>
    public class BadWord
    {
        public string Word { get; set; } = string.Empty;
        public int Severity { get; set; } = 2; // 1=Low, 2=Medium, 3=High
        public string Category { get; set; } = "General";
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Detected bad word with context
    /// </summary>
    public class DetectedBadWord
    {
        public string Word { get; set; } = string.Empty;
        public string OriginalWord { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Severity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Bad word detection result
    /// </summary>
    public class BadWordDetectionResult
    {
        public bool IsClean { get; set; }
        public List<DetectedBadWord> DetectedWords { get; set; } = new List<DetectedBadWord>();
        public int MaxSeverity { get; set; }
        public int TotalCount { get; set; }
        
        public bool HasHighSeverity => MaxSeverity >= 3;
        public bool HasMediumSeverity => MaxSeverity >= 2;
        public bool HasLowSeverity => MaxSeverity >= 1;
    }
    
    /// <summary>
    /// Event arguments for bad word detection
    /// </summary>
    public class BadWordDetectedEventArgs : EventArgs
    {
        public BadWord Word { get; set; } = new BadWord();
        public string Context { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
