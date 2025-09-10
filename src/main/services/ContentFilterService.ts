import { EventEmitter } from 'events';
import * as path from 'path';
import * as fs from 'fs/promises';
import { Logger } from '../utils/Logger';
import type { FilterRule, FilterResult } from '@shared/types';
import { FILTER_CATEGORIES, SEVERITY_LEVELS } from '@shared/constants';

interface FilterDictionary {
  [category: string]: {
    [severity: string]: string[];
  };
}

export class ContentFilterService extends EventEmitter {
  private logger: Logger;
  private filterRules: FilterRule[] = [];
  private filterDictionary: FilterDictionary = {};
  private isInitialized = false;

  constructor() {
    super();
    this.logger = new Logger();
  }

  public async initialize(): Promise<void> {
    try {
      await this.loadFilterDictionary();
      await this.loadCustomRules();
      this.isInitialized = true;
      this.logger.info('Content filter service initialized successfully');
    } catch (error) {
      this.logger.error('Failed to initialize content filter service:', error);
      throw error;
    }
  }

  private async loadFilterDictionary(): Promise<void> {
    // Initialize with comprehensive inappropriate content dictionary
    this.filterDictionary = {
      [FILTER_CATEGORIES.PROFANITY]: {
        [SEVERITY_LEVELS.LOW]: [
          'damn', 'hell', 'crap', 'stupid', 'idiot', 'dumb', 'suck', 'hate'
        ],
        [SEVERITY_LEVELS.MEDIUM]: [
          'shit', 'bitch', 'ass', 'piss', 'bastard', 'slut', 'whore', 'fuck'
        ],
        [SEVERITY_LEVELS.HIGH]: [
          'motherfucker', 'cocksucker', 'cunt', 'nigger', 'faggot', 'retard'
        ],
        [SEVERITY_LEVELS.CRITICAL]: [
          // Extreme profanity and slurs
          'fucking', 'goddamn', 'jesus christ', 'holy shit'
        ]
      },
      
      [FILTER_CATEGORIES.VIOLENCE]: {
        [SEVERITY_LEVELS.LOW]: [
          'fight', 'hit', 'punch', 'kick', 'hurt', 'pain', 'blood'
        ],
        [SEVERITY_LEVELS.MEDIUM]: [
          'kill', 'murder', 'death', 'weapon', 'gun', 'knife', 'bomb', 'violence'
        ],
        [SEVERITY_LEVELS.HIGH]: [
          'suicide', 'torture', 'massacre', 'genocide', 'terrorist', 'assassination'
        ],
        [SEVERITY_LEVELS.CRITICAL]: [
          'school shooting', 'mass murder', 'serial killer', 'homicide'
        ]
      },

      [FILTER_CATEGORIES.ADULT]: {
        [SEVERITY_LEVELS.LOW]: [
          'sexy', 'hot', 'kiss', 'date', 'boyfriend', 'girlfriend'
        ],
        [SEVERITY_LEVELS.MEDIUM]: [
          'sex', 'porn', 'nude', 'naked', 'breast', 'penis', 'vagina'
        ],
        [SEVERITY_LEVELS.HIGH]: [
          'pornography', 'masturbate', 'orgasm', 'sexual', 'erotic'
        ],
        [SEVERITY_LEVELS.CRITICAL]: [
          'child porn', 'pedophile', 'rape', 'sexual abuse', 'incest'
        ]
      },

      [FILTER_CATEGORIES.CYBERBULLYING]: {
        [SEVERITY_LEVELS.LOW]: [
          'loser', 'ugly', 'fat', 'weird', 'freak', 'nerd', 'geek'
        ],
        [SEVERITY_LEVELS.MEDIUM]: [
          'kill yourself', 'nobody likes you', 'you should die', 'worthless'
        ],
        [SEVERITY_LEVELS.HIGH]: [
          'commit suicide', 'end your life', 'world would be better without you'
        ],
        [SEVERITY_LEVELS.CRITICAL]: [
          'i will kill you', 'i know where you live', 'you deserve to die'
        ]
      },

      [FILTER_CATEGORIES.DRUGS]: {
        [SEVERITY_LEVELS.LOW]: [
          'beer', 'wine', 'alcohol', 'drunk', 'party', 'smoking'
        ],
        [SEVERITY_LEVELS.MEDIUM]: [
          'weed', 'marijuana', 'cannabis', 'cocaine', 'heroin', 'meth'
        ],
        [SEVERITY_LEVELS.HIGH]: [
          'drug dealer', 'drug dealing', 'overdose', 'addiction'
        ],
        [SEVERITY_LEVELS.CRITICAL]: [
          'how to make drugs', 'buy drugs online', 'drug trafficking'
        ]
      },

      [FILTER_CATEGORIES.HATE_SPEECH]: {
        [SEVERITY_LEVELS.LOW]: [
          'racist', 'discrimination', 'prejudice', 'stereotype'
        ],
        [SEVERITY_LEVELS.MEDIUM]: [
          'white supremacy', 'nazi', 'hitler', 'holocaust denial'
        ],
        [SEVERITY_LEVELS.HIGH]: [
          'ethnic cleansing', 'racial purity', 'master race'
        ],
        [SEVERITY_LEVELS.CRITICAL]: [
          'kill all jews', 'gas the jews', 'white power', 'racial war'
        ]
      },

      [FILTER_CATEGORIES.SELF_HARM]: {
        [SEVERITY_LEVELS.LOW]: [
          'sad', 'depressed', 'lonely', 'hopeless', 'worthless'
        ],
        [SEVERITY_LEVELS.MEDIUM]: [
          'self harm', 'cutting', 'suicide', 'want to die'
        ],
        [SEVERITY_LEVELS.HIGH]: [
          'how to kill myself', 'suicide methods', 'painless death'
        ],
        [SEVERITY_LEVELS.CRITICAL]: [
          'suicide plan', 'goodbye cruel world', 'ending it tonight'
        ]
      }
    };

    this.logger.info('Filter dictionary loaded with comprehensive content rules');
  }

  private async loadCustomRules(): Promise<void> {
    // Load additional custom filter rules from database or config
    // This would typically load from a database or configuration file
    this.filterRules = [
      {
        id: 'custom-1',
        pattern: '\\b(buy|purchase|order)\\s+(drugs|cocaine|heroin|meth)\\b',
        category: FILTER_CATEGORIES.DRUGS,
        severity: SEVERITY_LEVELS.CRITICAL,
        isRegex: true,
        isActive: true,
        language: 'en',
        createdAt: new Date()
      },
      {
        id: 'custom-2',
        pattern: '\\b(meet|hookup|sex)\\s+(tonight|now|today)\\b',
        category: FILTER_CATEGORIES.ADULT,
        severity: SEVERITY_LEVELS.HIGH,
        isRegex: true,
        isActive: true,
        language: 'en',
        createdAt: new Date()
      }
    ];

    this.logger.info(`Loaded ${this.filterRules.length} custom filter rules`);
  }

  public async checkContent(text: string): Promise<FilterResult> {
    if (!this.isInitialized) {
      throw new Error('Content filter service not initialized');
    }

    const normalizedText = text.toLowerCase().trim();
    const matchedRules: FilterRule[] = [];
    let highestSeverity: keyof typeof SEVERITY_LEVELS = SEVERITY_LEVELS.LOW;
    let totalConfidence = 0;
    let matchCount = 0;

    try {
      // Check against dictionary patterns
      for (const [category, severityLevels] of Object.entries(this.filterDictionary)) {
        for (const [severity, words] of Object.entries(severityLevels)) {
          for (const word of words) {
            if (this.containsWord(normalizedText, word)) {
              const rule: FilterRule = {
                id: `dict-${category}-${severity}-${word}`,
                pattern: word,
                category,
                severity: severity as keyof typeof SEVERITY_LEVELS,
                isRegex: false,
                isActive: true,
                language: 'en',
                createdAt: new Date()
              };

              matchedRules.push(rule);
              matchCount++;

              // Update highest severity
              if (this.getSeverityWeight(severity as keyof typeof SEVERITY_LEVELS) > 
                  this.getSeverityWeight(highestSeverity)) {
                highestSeverity = severity as keyof typeof SEVERITY_LEVELS;
              }

              // Add to confidence score
              totalConfidence += this.getSeverityWeight(severity as keyof typeof SEVERITY_LEVELS) * 0.25;
            }
          }
        }
      }

      // Check against custom regex rules
      for (const rule of this.filterRules) {
        if (!rule.isActive) continue;

        try {
          const regex = new RegExp(rule.pattern, 'gi');
          if (regex.test(normalizedText)) {
            matchedRules.push(rule);
            matchCount++;

            if (this.getSeverityWeight(rule.severity) > this.getSeverityWeight(highestSeverity)) {
              highestSeverity = rule.severity;
            }

            totalConfidence += this.getSeverityWeight(rule.severity) * 0.3;
          }
        } catch (error) {
          this.logger.error(`Invalid regex pattern in rule ${rule.id}:`, error);
        }
      }

      // Calculate final confidence (0-1 scale)
      const confidence = Math.min(totalConfidence / 4, 1);
      const isFiltered = matchedRules.length > 0;

      const result: FilterResult = {
        isFiltered,
        matchedRules,
        severity: highestSeverity,
        confidence
      };

      if (isFiltered) {
        this.logger.warn(`Content filtered: ${matchCount} matches, severity: ${highestSeverity}, confidence: ${confidence.toFixed(2)}`);
        this.emit('content-filtered', { text: normalizedText, result });
      }

      return result;
    } catch (error) {
      this.logger.error('Error checking content:', error);
      throw error;
    }
  }

  private containsWord(text: string, word: string): boolean {
    // Use word boundaries to avoid false positives
    const regex = new RegExp(`\\b${this.escapeRegex(word)}\\b`, 'i');
    return regex.test(text);
  }

  private escapeRegex(string: string): string {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }

  private getSeverityWeight(severity: keyof typeof SEVERITY_LEVELS): number {
    switch (severity) {
      case SEVERITY_LEVELS.LOW: return 1;
      case SEVERITY_LEVELS.MEDIUM: return 2;
      case SEVERITY_LEVELS.HIGH: return 3;
      case SEVERITY_LEVELS.CRITICAL: return 4;
      default: return 1;
    }
  }

  public async addCustomRule(rule: Omit<FilterRule, 'id' | 'createdAt'>): Promise<FilterRule> {
    const newRule: FilterRule = {
      ...rule,
      id: `custom-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      createdAt: new Date()
    };

    this.filterRules.push(newRule);
    this.logger.info(`Added custom filter rule: ${newRule.id}`);
    
    return newRule;
  }

  public async updateRule(ruleId: string, updates: Partial<FilterRule>): Promise<FilterRule | null> {
    const ruleIndex = this.filterRules.findIndex(rule => rule.id === ruleId);
    
    if (ruleIndex === -1) {
      return null;
    }

    this.filterRules[ruleIndex] = { ...this.filterRules[ruleIndex], ...updates };
    this.logger.info(`Updated filter rule: ${ruleId}`);
    
    return this.filterRules[ruleIndex];
  }

  public async deleteRule(ruleId: string): Promise<boolean> {
    const ruleIndex = this.filterRules.findIndex(rule => rule.id === ruleId);
    
    if (ruleIndex === -1) {
      return false;
    }

    this.filterRules.splice(ruleIndex, 1);
    this.logger.info(`Deleted filter rule: ${ruleId}`);
    
    return true;
  }

  public getFilterRules(): FilterRule[] {
    return [...this.filterRules];
  }

  public getFilterCategories(): string[] {
    return Object.values(FILTER_CATEGORIES);
  }

  public async testContent(text: string): Promise<FilterResult> {
    // Test method for debugging and validation
    return await this.checkContent(text);
  }
}
