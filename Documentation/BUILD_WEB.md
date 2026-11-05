# Jugar en localhost

Abre `JUGAR_LOCALHOST.cmd` desde la raíz del proyecto. Sirve el juego en **http://localhost:8080** y abre el navegador. Conserva la ventana del servidor durante la partida; Ctrl+C lo cierra. No se publica el proyecto en Internet.

Pulsa **ENTRAR AL JUEGO** al terminar la carga. Usa el menú para registrar un piloto e iniciar la campaña. **VUELO RÁPIDO / SIN REGISTRO** abre CoreSandbox para probar la nave directamente. Los controles son W/S, A/D, Espacio, Shift y Escape. La pantalla completa se activa desde el botón de la página.

El botón **DESCARGAR PARA WINDOWS** entrega el ZIP completo del ejecutable. También puede iniciarse con `JUGAR_WINDOWS.cmd`, sin navegador ni servidor.

## Compilación

El proyecto principal permanece en Unity 6000.3.15f1 y URP 17.3. El módulo Web faltaba en esa instalación. Para aprovechar el módulo existente, `Tools/build_web.py` copia Assets, Packages y ProjectSettings a `BuildWork/UnityWeb`, configura Unity 6000.4.6f1 y URP 17.4 en esa copia y compila las mismas escenas Campaign/CoreSandbox y scripts del juego. No modifica la versión del proyecto principal. `BuildWork` no se incluye en Git.

```powershell
python Tools/build_web.py
python Tools/serve_web.py --open --port 8080
```

El resultado se copia a `Builds/WebGL`. El resumen real queda en `Documentation/Evidence/web-build-summary.txt`; el registro completo, en `web-build.log`.

También existe `Tools → Nebula Defender → Build Web Localhost` para un Editor con Web Build Support instalado. En Unity 6.3 necesita su módulo web correspondiente.

## Servidor y guardado

El servidor solo escucha en 127.0.0.1. Sirve la carpeta compilada, nunca la raíz del código. Envía `Content-Type: application/wasm` y `Content-Encoding: gzip` para los archivos comprimidos; el ZIP de Windows tiene una ruta de descarga específica.

Los perfiles y el ranking se guardan en el almacenamiento local del navegador (IndexedDB), mediante el mismo JSON versionado y la sincronización del sistema de archivos de Unity. Mantén el mismo navegador y dirección `http://localhost:8080` para recuperarlos. No se comparten con los perfiles del ejecutable. Borrar los datos del sitio elimina ese progreso.

La página no carga fuentes, scripts ni recursos desde servicios externos. Una vez creada la compilación, no necesita Internet para servir el juego en localhost.

Referencias: [archivos de una compilación Web de Unity](https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-building.html), [plantillas web](https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-templates.html), [interacción con JavaScript](https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-interactingwithbrowserscripting.html).
