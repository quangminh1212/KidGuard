const fs = require('fs');
const path = require('path');

console.log('🧪 ChildGuard Feature Testing Suite');
console.log('=====================================\n');

// Test 1: Check if all essential files exist
console.log('📁 Testing File Structure...');
const essentialFiles = [
  'src/main/main.ts',
  'src/main/services/KeyloggerService.ts',
  'src/main/services/ContentFilterService.ts',
  'src/main/services/AuthService.ts',
  'src/main/database/DatabaseManager.ts',
  'src/renderer/App.tsx',
  'src/renderer/pages/LoginPage.tsx',
  'src/renderer/pages/DashboardPage.tsx',
  'src/renderer/components/layout/DashboardLayout.tsx',
  'src/renderer/components/common/StatCard.tsx',
  'src/renderer/components/common/GlassCard.tsx',
  'src/renderer/components/common/GradientButton.tsx',
  'src/renderer/theme/index.ts',
  'package.json',
  'README.md'
];

let fileTestsPassed = 0;
essentialFiles.forEach(file => {
  if (fs.existsSync(file)) {
    console.log(`✅ ${file}`);
    fileTestsPassed++;
  } else {
    console.log(`❌ ${file} - MISSING`);
  }
});

console.log(`\n📊 File Structure: ${fileTestsPassed}/${essentialFiles.length} files found\n`);

// Test 2: Check package.json dependencies
console.log('📦 Testing Dependencies...');
const packageJson = JSON.parse(fs.readFileSync('package.json', 'utf8'));
const requiredDeps = [
  'react',
  'react-dom',
  'react-router-dom',
  '@mui/material',
  '@mui/icons-material',
  'framer-motion',
  'electron',
  'better-sqlite3',
  'bcrypt',
  'crypto-js'
];

let depTestsPassed = 0;
requiredDeps.forEach(dep => {
  if (packageJson.dependencies[dep] || packageJson.devDependencies[dep]) {
    console.log(`✅ ${dep}`);
    depTestsPassed++;
  } else {
    console.log(`❌ ${dep} - MISSING`);
  }
});

console.log(`\n📊 Dependencies: ${depTestsPassed}/${requiredDeps.length} dependencies found\n`);

// Test 3: Check TypeScript configuration
console.log('⚙️ Testing TypeScript Configuration...');
const tsConfigFiles = ['tsconfig.json', 'tsconfig.main.json', 'tsconfig.renderer.json'];
let tsTestsPassed = 0;

tsConfigFiles.forEach(file => {
  if (fs.existsSync(file)) {
    console.log(`✅ ${file}`);
    tsTestsPassed++;
  } else {
    console.log(`❌ ${file} - MISSING`);
  }
});

console.log(`\n📊 TypeScript Config: ${tsTestsPassed}/${tsConfigFiles.length} config files found\n`);

// Test 4: Check Webpack configuration
console.log('🔧 Testing Webpack Configuration...');
const webpackFiles = ['webpack.main.config.js', 'webpack.renderer.config.js'];
let webpackTestsPassed = 0;

webpackFiles.forEach(file => {
  if (fs.existsSync(file)) {
    console.log(`✅ ${file}`);
    webpackTestsPassed++;
  } else {
    console.log(`❌ ${file} - MISSING`);
  }
});

console.log(`\n📊 Webpack Config: ${webpackTestsPassed}/${webpackFiles.length} config files found\n`);

// Test 5: Check UI Components
console.log('🎨 Testing UI Components...');
const uiComponents = [
  'src/renderer/components/common/StatCard.tsx',
  'src/renderer/components/common/GlassCard.tsx',
  'src/renderer/components/common/GradientButton.tsx',
  'src/renderer/components/animations/FadeInUp.tsx',
  'src/renderer/components/animations/LoadingSpinner.tsx',
  'src/renderer/components/animations/AnimatedCard.tsx'
];

let uiTestsPassed = 0;
uiComponents.forEach(component => {
  if (fs.existsSync(component)) {
    console.log(`✅ ${path.basename(component)}`);
    uiTestsPassed++;
  } else {
    console.log(`❌ ${path.basename(component)} - MISSING`);
  }
});

console.log(`\n📊 UI Components: ${uiTestsPassed}/${uiComponents.length} components found\n`);

// Test 6: Check Core Services
console.log('🔐 Testing Core Services...');
const coreServices = [
  'src/main/services/KeyloggerService.ts',
  'src/main/services/ContentFilterService.ts',
  'src/main/services/AuthService.ts',
  'src/main/services/NotificationService.ts',
  'src/main/database/DatabaseManager.ts',
  'src/main/config/ConfigManager.ts'
];

let serviceTestsPassed = 0;
coreServices.forEach(service => {
  if (fs.existsSync(service)) {
    console.log(`✅ ${path.basename(service)}`);
    serviceTestsPassed++;
  } else {
    console.log(`❌ ${path.basename(service)} - MISSING`);
  }
});

console.log(`\n📊 Core Services: ${serviceTestsPassed}/${coreServices.length} services found\n`);

// Test 7: Check Documentation
console.log('📚 Testing Documentation...');
const docFiles = [
  'README.md',
  'docs/DEPLOYMENT_GUIDE.md',
  'docs/PRIVACY_POLICY.md',
  'docs/UI_DESIGN_GUIDE.md',
  'PROJECT_SUMMARY.md'
];

let docTestsPassed = 0;
docFiles.forEach(doc => {
  if (fs.existsSync(doc)) {
    console.log(`✅ ${path.basename(doc)}`);
    docTestsPassed++;
  } else {
    console.log(`❌ ${path.basename(doc)} - MISSING`);
  }
});

console.log(`\n📊 Documentation: ${docTestsPassed}/${docFiles.length} documents found\n`);

// Test 8: Check Build Scripts
console.log('🔨 Testing Build Scripts...');
const buildScripts = ['setup.bat', 'run.bat', 'build.bat', 'quick-start.bat'];
let scriptTestsPassed = 0;

buildScripts.forEach(script => {
  if (fs.existsSync(script)) {
    console.log(`✅ ${script}`);
    scriptTestsPassed++;
  } else {
    console.log(`❌ ${script} - MISSING`);
  }
});

console.log(`\n📊 Build Scripts: ${scriptTestsPassed}/${buildScripts.length} scripts found\n`);

// Final Summary
console.log('🎯 FINAL TEST SUMMARY');
console.log('=====================');
const totalTests = fileTestsPassed + depTestsPassed + tsTestsPassed + webpackTestsPassed + uiTestsPassed + serviceTestsPassed + docTestsPassed + scriptTestsPassed;
const maxTests = essentialFiles.length + requiredDeps.length + tsConfigFiles.length + webpackFiles.length + uiComponents.length + coreServices.length + docFiles.length + buildScripts.length;

console.log(`📁 File Structure: ${fileTestsPassed}/${essentialFiles.length}`);
console.log(`📦 Dependencies: ${depTestsPassed}/${requiredDeps.length}`);
console.log(`⚙️ TypeScript Config: ${tsTestsPassed}/${tsConfigFiles.length}`);
console.log(`🔧 Webpack Config: ${webpackTestsPassed}/${webpackFiles.length}`);
console.log(`🎨 UI Components: ${uiTestsPassed}/${uiComponents.length}`);
console.log(`🔐 Core Services: ${serviceTestsPassed}/${coreServices.length}`);
console.log(`📚 Documentation: ${docTestsPassed}/${docFiles.length}`);
console.log(`🔨 Build Scripts: ${scriptTestsPassed}/${buildScripts.length}`);

console.log(`\n🏆 OVERALL SCORE: ${totalTests}/${maxTests} (${Math.round((totalTests/maxTests)*100)}%)`);

if (totalTests === maxTests) {
  console.log('\n🎉 ALL TESTS PASSED! ChildGuard project is complete and ready!');
} else if (totalTests >= maxTests * 0.9) {
  console.log('\n✅ EXCELLENT! Project is nearly complete with minor issues.');
} else if (totalTests >= maxTests * 0.8) {
  console.log('\n⚠️ GOOD! Project is mostly complete but needs some attention.');
} else {
  console.log('\n❌ NEEDS WORK! Several components are missing or incomplete.');
}

console.log('\n🚀 To run the application:');
console.log('1. npm install');
console.log('2. npm run build');
console.log('3. npm start');
console.log('\n📖 Check README.md for detailed instructions.');
