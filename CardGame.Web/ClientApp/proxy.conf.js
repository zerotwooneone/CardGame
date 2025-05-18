const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
    ? env.ASPNETCORE_URLS.split(';')[0]
    : 'https://localhost:7184'; // Default to your backend's HTTPS port

const PROXY_CONFIG = [
  {
    context: [
      "/api"  // General rule for all API calls
    ],
    target: target,
    secure: false,
    changeOrigin: true,
    //logLevel: "debug"
  },
  {
    context: [
      '/hubs/notification',
      '/hubs/game'
    ],
    target: target,
    secure: false,
    ws: true,
    //logLevel: "debug"
  }
];

module.exports = PROXY_CONFIG;
