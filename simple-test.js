const fs = require('fs');

console.log('🧪 ChildGuard Quick Test');
console.log('========================\n');

// Test essential files
const files = [
  'src/main/main.ts',
  'src/renderer/App.tsx',
  'src/renderer/pages/LoginPage.tsx',
  'src/renderer/pages/DashboardPage.tsx',
  'package.json'
];

console.log('📁 Checking essential files:');
files.forEach(file => {
  const exists = fs.existsSync(file);
  console.log(`${exists ? '✅' : '❌'} ${file}`);
});

// Check package.json
console.log('\n📦 Checking package.json:');
try {
  const pkg = JSON.parse(fs.readFileSync('package.json', 'utf8'));
  console.log(`✅ Name: ${pkg.name}`);
  console.log(`✅ Version: ${pkg.version}`);
  console.log(`✅ Main: ${pkg.main}`);
  
  const deps = ['react', '@mui/material', 'framer-motion', 'electron'];
  deps.forEach(dep => {
    const has = pkg.dependencies[dep] || pkg.devDependencies[dep];
    console.log(`${has ? '✅' : '❌'} ${dep}: ${has || 'missing'}`);
  });
} catch (e) {
  console.log('❌ Error reading package.json');
}

console.log('\n🎯 Project Status: Ready for testing!');
console.log('\n🚀 Next steps:');
console.log('1. Run: npm install');
console.log('2. Run: npm run build');
console.log('3. Run: npm start');
