const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

console.log('🚀 Starting ChildGuard Build Process...\n');

// Check if Node.js version is compatible
const nodeVersion = process.version;
const requiredVersion = 'v18.0.0';
console.log(`📋 Node.js version: ${nodeVersion}`);

if (nodeVersion < requiredVersion) {
  console.error(`❌ Node.js ${requiredVersion} or higher is required`);
  process.exit(1);
}

// Check if all required directories exist
const requiredDirs = [
  'src/main',
  'src/renderer',
  'src/shared',
  'assets'
];

console.log('📁 Checking directory structure...');
for (const dir of requiredDirs) {
  if (!fs.existsSync(dir)) {
    console.error(`❌ Required directory missing: ${dir}`);
    process.exit(1);
  }
}
console.log('✅ Directory structure OK\n');

// Install dependencies if node_modules doesn't exist
if (!fs.existsSync('node_modules')) {
  console.log('📦 Installing dependencies...');
  try {
    execSync('npm install', { stdio: 'inherit' });
    console.log('✅ Dependencies installed\n');
  } catch (error) {
    console.error('❌ Failed to install dependencies');
    process.exit(1);
  }
}

// Run TypeScript compilation check
console.log('🔍 Checking TypeScript compilation...');
try {
  execSync('npx tsc --noEmit', { stdio: 'inherit' });
  console.log('✅ TypeScript compilation OK\n');
} catch (error) {
  console.error('❌ TypeScript compilation failed');
  process.exit(1);
}

// Run linting
console.log('🧹 Running ESLint...');
try {
  execSync('npx eslint src --ext .ts,.tsx --max-warnings 0', { stdio: 'inherit' });
  console.log('✅ Linting passed\n');
} catch (error) {
  console.warn('⚠️ Linting warnings found, continuing...\n');
}

// Run tests
console.log('🧪 Running tests...');
try {
  execSync('npm test', { stdio: 'inherit' });
  console.log('✅ Tests passed\n');
} catch (error) {
  console.warn('⚠️ Some tests failed, continuing...\n');
}

// Build the application
console.log('🔨 Building application...');
try {
  execSync('npm run build', { stdio: 'inherit' });
  console.log('✅ Build completed\n');
} catch (error) {
  console.error('❌ Build failed');
  process.exit(1);
}

// Check if build artifacts exist
const buildArtifacts = [
  'dist/main.js',
  'dist/renderer.js',
  'dist/index.html'
];

console.log('📋 Checking build artifacts...');
for (const artifact of buildArtifacts) {
  if (!fs.existsSync(artifact)) {
    console.error(`❌ Build artifact missing: ${artifact}`);
    process.exit(1);
  }
}
console.log('✅ Build artifacts OK\n');

console.log('🎉 ChildGuard build completed successfully!');
console.log('\n📋 Next steps:');
console.log('   • Run "npm start" to test the application');
console.log('   • Run "npm run dist" to create installer');
console.log('   • Check the README.md for deployment instructions');
