using System;
using System.Collections.Generic;
using System.Linq;

namespace ChildGuard.Core.Detection
{
    /// <summary>
    /// Simple Trie implementation for bad words detection
    /// </summary>
    public class SimpleTrie
    {
        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; }
            public bool IsEndOfWord { get; set; }
            public string? Word { get; set; }
            
            public TrieNode()
            {
                Children = new Dictionary<char, TrieNode>();
                IsEndOfWord = false;
            }
        }
        
        private readonly TrieNode _root;
        
        public SimpleTrie()
        {
            _root = new TrieNode();
        }
        
        /// <summary>
        /// Add a word to the trie
        /// </summary>
        public void Add(string word)
        {
            if (string.IsNullOrEmpty(word)) return;
            
            var current = _root;
            var lowerWord = word.ToLowerInvariant();
            
            foreach (var ch in lowerWord)
            {
                if (!current.Children.ContainsKey(ch))
                {
                    current.Children[ch] = new TrieNode();
                }
                current = current.Children[ch];
            }
            
            current.IsEndOfWord = true;
            current.Word = lowerWord;
        }
        
        /// <summary>
        /// Add multiple words
        /// </summary>
        public void AddRange(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                Add(word);
            }
        }
        
        /// <summary>
        /// Search for a word
        /// </summary>
        public bool Search(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            
            var current = _root;
            var lowerWord = word.ToLowerInvariant();
            
            foreach (var ch in lowerWord)
            {
                if (!current.Children.ContainsKey(ch))
                {
                    return false;
                }
                current = current.Children[ch];
            }
            
            return current.IsEndOfWord;
        }
        
        /// <summary>
        /// Find all matches in text
        /// </summary>
        public List<TrieMatch> FindAllMatches(string text)
        {
            var matches = new List<TrieMatch>();
            if (string.IsNullOrEmpty(text)) return matches;
            
            var lowerText = text.ToLowerInvariant();
            
            for (int i = 0; i < lowerText.Length; i++)
            {
                var current = _root;
                int j = i;
                
                while (j < lowerText.Length && current.Children.ContainsKey(lowerText[j]))
                {
                    current = current.Children[lowerText[j]];
                    
                    if (current.IsEndOfWord && current.Word != null)
                    {
                        matches.Add(new TrieMatch
                        {
                            Word = current.Word,
                            Position = i,
                            Length = j - i + 1
                        });
                    }
                    
                    j++;
                }
            }
            
            return matches;
        }
        
        /// <summary>
        /// Clear the trie
        /// </summary>
        public void Clear()
        {
            _root.Children.Clear();
        }
    }
    
    /// <summary>
    /// Represents a match found in text
    /// </summary>
    public class TrieMatch
    {
        public string Word { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Length { get; set; }
    }
}
