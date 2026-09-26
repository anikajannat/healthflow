import { writeFileSync } from 'node:fs';
const apiUrl = process.env.API_URL || 'http://localhost:5080/api/v1';
writeFileSync('src/assets/env.js', `window.__env = { API_URL: ${JSON.stringify(apiUrl)} };\n`);
console.log(`Generated runtime API URL: ${apiUrl}`);
