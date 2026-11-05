"""Serve only the built game on the loopback interface, with Unity gzip/Wasm headers."""
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import argparse
import json
import mimetypes
import shutil
import urllib.request
import webbrowser

ROOT = Path(__file__).resolve().parents[1]
WEB = ROOT / "Builds/WebGL"
PACKAGE = ROOT / "Builds/NebulaDefender-Windows-x64.zip"


class GameHandler(SimpleHTTPRequestHandler):
    def do_GET(self):
        if self.path == "/favicon.ico":
            icon = b'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><rect width="64" height="64" rx="12" fill="#071424"/><path d="M32 7 56 54 32 43 8 54Z" fill="#61edff"/><path d="M32 15 32 43 23 47Z" fill="#fff"/></svg>'
            self.send_response(200)
            self.send_header("Content-Type", "image/svg+xml")
            self.send_header("Content-Length", str(len(icon)))
            self.end_headers()
            self.wfile.write(icon)
            return
        if self.path == "/health":
            data = json.dumps({"app": "nebula-defender", "ready": (WEB / "index.html").exists()}).encode()
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(data)))
            self.end_headers()
            self.wfile.write(data)
            return
        if self.path == "/descargar/NebulaDefender-Windows-x64.zip":
            if not PACKAGE.exists():
                self.send_error(404, "El paquete Windows no está disponible")
                return
            self.send_response(200)
            self.send_header("Content-Type", "application/zip")
            self.send_header("Content-Disposition", 'attachment; filename="NebulaDefender-Windows-x64.zip"')
            self.send_header("Content-Length", str(PACKAGE.stat().st_size))
            self.end_headers()
            with PACKAGE.open("rb") as stream:
                shutil.copyfileobj(stream, self.wfile)
            return
        super().do_GET()

    def guess_type(self, path):
        if path.endswith(".gz"):
            path = path[:-3]
        if path.endswith(".wasm"):
            return "application/wasm"
        if path.endswith(".data"):
            return "application/octet-stream"
        return mimetypes.guess_type(path)[0] or "application/octet-stream"

    def end_headers(self):
        if self.path.split("?", 1)[0].endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        self.send_header("Cache-Control", "no-cache")
        self.send_header("X-Content-Type-Options", "nosniff")
        super().end_headers()

    def list_directory(self, path):
        self.send_error(403, "Listado de carpetas desactivado")
        return None


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--port", type=int, default=8080)
    parser.add_argument("--open", action="store_true")
    args = parser.parse_args()
    url = f"http://localhost:{args.port}"
    try:
        with urllib.request.urlopen(url + "/health", timeout=1) as response:
            if json.load(response).get("app") == "nebula-defender":
                print("Juego ya disponible en " + url, flush=True)
                if args.open:
                    webbrowser.open(url)
                return
    except (OSError, ValueError):
        pass
    if not (WEB / "index.html").exists():
        raise SystemExit("Falta la compilación web. Ejecuta primero: python Tools/build_web.py")
    handler = partial(GameHandler, directory=str(WEB))
    with ThreadingHTTPServer(("127.0.0.1", args.port), handler) as server:
        print("NEBULA DEFENDER: " + url + " — Ctrl+C para cerrar el servidor.", flush=True)
        if args.open:
            webbrowser.open(url)
        try:
            server.serve_forever()
        except KeyboardInterrupt:
            pass


if __name__ == "__main__":
    main()
