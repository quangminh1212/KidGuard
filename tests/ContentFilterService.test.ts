import { ContentFilterService } from '../src/main/services/ContentFilterService';

describe('ContentFilterService', () => {
  let contentFilterService: ContentFilterService;

  beforeEach(async () => {
    contentFilterService = new ContentFilterService();
    await contentFilterService.initialize();
  });

  describe('Content Filtering', () => {
    test('should detect inappropriate content', async () => {
      const result = await contentFilterService.checkContent('This is some bad shit');
      
      expect(result.isFiltered).toBe(true);
      expect(result.severity).toBe('medium');
      expect(result.matchedRules.length).toBeGreaterThan(0);
    });

    test('should not filter appropriate content', async () => {
      const result = await contentFilterService.checkContent('Hello world, this is a nice day');
      
      expect(result.isFiltered).toBe(false);
      expect(result.matchedRules.length).toBe(0);
    });

    test('should detect critical level content', async () => {
      const result = await contentFilterService.checkContent('child porn is terrible');
      
      expect(result.isFiltered).toBe(true);
      expect(result.severity).toBe('critical');
    });

    test('should handle empty content', async () => {
      const result = await contentFilterService.checkContent('');
      
      expect(result.isFiltered).toBe(false);
      expect(result.matchedRules.length).toBe(0);
    });

    test('should be case insensitive', async () => {
      const result1 = await contentFilterService.checkContent('DAMN');
      const result2 = await contentFilterService.checkContent('damn');
      
      expect(result1.isFiltered).toBe(true);
      expect(result2.isFiltered).toBe(true);
      expect(result1.severity).toBe(result2.severity);
    });

    test('should detect word boundaries correctly', async () => {
      const result1 = await contentFilterService.checkContent('damn this');
      const result2 = await contentFilterService.checkContent('damnation');
      
      expect(result1.isFiltered).toBe(true);
      expect(result2.isFiltered).toBe(false); // Should not match partial words
    });
  });

  describe('Custom Rules', () => {
    test('should add custom filter rule', async () => {
      const rule = await contentFilterService.addCustomRule({
        pattern: 'customword',
        category: 'custom',
        severity: 'high',
        isRegex: false,
        isActive: true,
        language: 'en'
      });

      expect(rule.id).toBeDefined();
      expect(rule.pattern).toBe('customword');
      expect(rule.severity).toBe('high');
    });

    test('should apply custom rules', async () => {
      await contentFilterService.addCustomRule({
        pattern: 'testword',
        category: 'custom',
        severity: 'medium',
        isRegex: false,
        isActive: true,
        language: 'en'
      });

      const result = await contentFilterService.checkContent('This contains testword');
      
      expect(result.isFiltered).toBe(true);
      expect(result.severity).toBe('medium');
    });

    test('should handle regex patterns', async () => {
      await contentFilterService.addCustomRule({
        pattern: '\\b(test|sample)\\s+word\\b',
        category: 'custom',
        severity: 'low',
        isRegex: true,
        isActive: true,
        language: 'en'
      });

      const result1 = await contentFilterService.checkContent('This is a test word');
      const result2 = await contentFilterService.checkContent('This is a sample word');
      const result3 = await contentFilterService.checkContent('This is a testword');
      
      expect(result1.isFiltered).toBe(true);
      expect(result2.isFiltered).toBe(true);
      expect(result3.isFiltered).toBe(false);
    });
  });

  describe('Rule Management', () => {
    test('should update existing rule', async () => {
      const rule = await contentFilterService.addCustomRule({
        pattern: 'updatetest',
        category: 'custom',
        severity: 'low',
        isRegex: false,
        isActive: true,
        language: 'en'
      });

      const updatedRule = await contentFilterService.updateRule(rule.id, {
        severity: 'high',
        isActive: false
      });

      expect(updatedRule).toBeDefined();
      expect(updatedRule!.severity).toBe('high');
      expect(updatedRule!.isActive).toBe(false);
    });

    test('should delete rule', async () => {
      const rule = await contentFilterService.addCustomRule({
        pattern: 'deletetest',
        category: 'custom',
        severity: 'medium',
        isRegex: false,
        isActive: true,
        language: 'en'
      });

      const deleted = await contentFilterService.deleteRule(rule.id);
      expect(deleted).toBe(true);

      const rules = contentFilterService.getFilterRules();
      expect(rules.find(r => r.id === rule.id)).toBeUndefined();
    });

    test('should return false when deleting non-existent rule', async () => {
      const deleted = await contentFilterService.deleteRule('non-existent-id');
      expect(deleted).toBe(false);
    });
  });

  describe('Performance', () => {
    test('should handle large text efficiently', async () => {
      const largeText = 'This is a test. '.repeat(1000) + 'damn';
      
      const startTime = Date.now();
      const result = await contentFilterService.checkContent(largeText);
      const endTime = Date.now();
      
      expect(result.isFiltered).toBe(true);
      expect(endTime - startTime).toBeLessThan(1000); // Should complete within 1 second
    });

    test('should handle multiple checks concurrently', async () => {
      const texts = [
        'This is clean text',
        'This contains damn word',
        'Another clean text',
        'This has shit in it',
        'Clean text again'
      ];

      const promises = texts.map(text => contentFilterService.checkContent(text));
      const results = await Promise.all(promises);

      expect(results[0].isFiltered).toBe(false);
      expect(results[1].isFiltered).toBe(true);
      expect(results[2].isFiltered).toBe(false);
      expect(results[3].isFiltered).toBe(true);
      expect(results[4].isFiltered).toBe(false);
    });
  });
});
