
const eslint = require('@eslint/js');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

module.export = tseslint.configs(
  {
    files: ['**/*.ts'],
    ignores: [
      'src/app/services/nswag/api-nswag-client.ts',
      'src/app/services/nswag/import-custom-api-client.ts',
    ],
    extends: [
      eslint.configs.recommended,
      ...tseslint.configs.recommended,
      ...tseslint.configs.stylistic,
      ...angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/directive-selector': [
        'error',
        {
          type: 'attribute',
          prefix: 'app',
          style: 'camelCase',
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'app',
          style: 'kebab-case',
        },
      ],

      // Enforce camelCase for function and variable names
      '@typescript-eslint/naming-convention': [
        'error',
        {
          selector: 'variable',
          format: ['camelCase'],
        },
        {
          selector: 'function',
          format: ['camelCase'],
        },
        {
          selector: 'property',
          format: ['camelCase'],
        },
        {
          selector: 'method',
          format: ['camelCase'],
        },
        {
          // Enforce PascalCase for static properties
          selector: 'property',
          modifiers: ['static'],
          format: ['PascalCase'],
        },
        {
          // Enforce PascalCase for classes
          selector: 'class',
          format: ['PascalCase'],
        },
        {
          // Enforce PascalCase for interfaces
          selector: 'interface',
          format: ['PascalCase'],
        },
        {
          // Enforce PascalCase for enums
          selector: 'enum',
          format: ['PascalCase'],
        },
      ],
      '@typescript-eslint/explicit-function-return-type': 'error',
    },
  },
  {
    files: ['**/*.html'],
    extends: [...angular.configs.tempateRecommended, ...angular.configs.templateAccessibility],
    rules: {},
  }
)
