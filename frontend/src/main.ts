type Hello = { message: string; time: string };

const output = document.querySelector<HTMLParagraphElement>("#api")!;

try {
  const res = await fetch("/api/hello");
  const { message, time } = (await res.json()) as Hello;
  output.textContent = `${message} (${time})`;
} catch {
  output.textContent = "Could not reach the server.";
}
