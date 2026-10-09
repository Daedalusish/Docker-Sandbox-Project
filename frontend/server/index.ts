import { createServer } from "node:http";
import { renderPage } from "./page.ts";

const port = Number(process.env.API_PORT ?? 3001);
const startedAt = Date.now();
let requests = 0;

createServer((req, res) => {
  requests++;

  if (req.url === "/") {
    res.writeHead(200, { "content-type": "text/html; charset=utf-8" });
    res.end(renderPage({ startedAt, requests }));
    return;
  }
  if (req.url === "/api/hello") {
    res.writeHead(200, { "content-type": "application/json" });
    res.end(
      JSON.stringify({
        message: "Hellooo from the other noooode!",
        time: new Date().toISOString(),
      }),
    );
    return;
  }
  res.writeHead(404).end();
}).listen(port, () => {
  console.log(`Server listening on http://localhost:${port}`);
});
