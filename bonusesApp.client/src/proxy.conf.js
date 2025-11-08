// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

// Отключаем проверку TLS сертификатов для работы с самоподписанными сертификатами в режиме разработки
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const { env } = require('process');
const https = require('https');

// Определяем целевой URL бэкенда
// По умолчанию используем HTTP для избежания проблем с TLS/SSL сертификатами
const useHttps = env.USE_HTTPS_PROXY === 'true';
const httpsPort = env.ASPNETCORE_HTTPS_PORT || '7085';
const httpPort = env.ASPNETCORE_HTTP_PORT || '5225';

// Используем HTTP по умолчанию для избежания проблем с TLS
// OpenIddict настроен для разрешения HTTP в режиме разработки
const target = useHttps
  ? (env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
     env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7085')
  : `http://localhost:${httpPort}`;

console.log('Proxy target:', target);

const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/swagger",
      "/connect",
      "/oauth",
      "/.well-known"
    ],
    target,
    secure: false, // Игнорируем ошибки SSL сертификата для самоподписанных сертификатов
    changeOrigin: true,
    // Дополнительные настройки для работы с HTTPS и самоподписанными сертификатами
    rejectUnauthorized: false, // Игнорируем ошибки проверки сертификата
    // Настройка HTTPS агента для работы с самоподписанными сертификатами
    agent: target.startsWith('https://') 
      ? new https.Agent({ 
          rejectUnauthorized: false,
          keepAlive: true 
        })
      : undefined,
    // Логирование для отладки
    configure: (proxy, _options) => {
      // Настройка для работы с самоподписанными сертификатами
      if (target.startsWith('https://')) {
        proxy.on('proxyReq', (proxyReq, req, _res) => {
          // Отключаем проверку сертификата для HTTPS соединений
          proxyReq.setHeader('Connection', 'keep-alive');
        });
      }
      
      proxy.on('error', (err, req, res) => {
        console.error('❌ Proxy error:', err.message);
        console.error('   Request URL:', req.url);
        console.error('   Target:', target);
        if (err.code === 'ECONNREFUSED') {
          console.error('   ⚠️  Backend не запущен или недоступен на', target);
        } else if (err.message.includes('TLS')) {
          console.error('   ⚠️  Проблема с TLS соединением. Проверьте SSL сертификат бэкенда.');
        }
      });
      proxy.on('proxyReq', (proxyReq, req, _res) => {
        console.log('➡️  Proxying:', req.method, req.url, '→', target + req.url);
      });
      proxy.on('proxyRes', (proxyRes, req, _res) => {
        console.log('⬅️  Response:', proxyRes.statusCode, req.url);
      });
    }
  }
]

module.exports = PROXY_CONFIG;
