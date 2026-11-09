// Test-only loopback proxy: omit the CDN SignalR script to exercise the real polling fallback.
// Run alongside the app at http://127.0.0.1:5187; browse http://127.0.0.1:5188.
const http = require('node:http');
const proxy = http.createServer((request, response) => {
  const upstream = http.request({ hostname: '127.0.0.1', port: 5187, path: request.url,
    method: request.method, headers: { ...request.headers, 'accept-encoding': 'identity' } }, (result) => {
    const chunks = [];
    result.on('data', (chunk) => chunks.push(chunk));
    result.on('end', () => {
      let body = Buffer.concat(chunks);
      const headers = { ...result.headers };
      if ((headers['content-type'] || '').includes('text/html')) {
        body = Buffer.from(body.toString('utf8').replace(/<script\b[^>]*\bsrc=["'][^"']*signalr[^"']*["'][^>]*>\s*<\/script>/gi, ''));
        delete headers['content-length']; delete headers['content-encoding'];
      }
      response.writeHead(result.statusCode, headers); response.end(body);
    });
  });
  upstream.on('error', () => { response.writeHead(502); response.end('Local test app unavailable'); });
  request.pipe(upstream);
});
proxy.listen(5188, '127.0.0.1', () => console.log('Test-only proxy omitting SignalR: http://127.0.0.1:5188'));
