const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:42494';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      // Proxy all requests starting with /api/Auth (covers /login, /logout)
      "/api/Auth",
      // Proxy all requests starting with /api/Game
      "/api/Game",
   ],
    proxyTimeout: 10000,
    target: target,
    secure: false, // Set to false for self-signed certs
    headers: {
      Connection: 'Keep-Alive'
    },
    //changeOrigin: true,
    //logLevel: "debug",
  },
  {
    // Paths for SignalR hubs (requires WebSocket proxying)
    context: [
      '/hubs/notification',
      '/hubs/game'
      // Add other hub paths here
    ],
    target: target,
    secure: false, // Set to false for self-signed certs
    ws: true, // *** IMPORTANT: Enable WebSocket proxying for SignalR ***
    //changeOrigin: true,
    //logLevel: 'debug'
  }
]

module.exports = PROXY_CONFIG;
