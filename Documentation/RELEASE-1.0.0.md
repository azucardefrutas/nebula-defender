# Nebula Defender 1.0.0 · Vuelve a casa

Publicación preparada el **11 de noviembre de 2026**. Código, modelos y compilaciones corresponden a la revisión de arte original validada el 6 de noviembre. Publicar archivos no equivale a repetir las pruebas del juego.

## Descargas

[Repositorio](https://github.com/azucardefrutas/nebula-defender) · [Release v1.0.0](https://github.com/azucardefrutas/nebula-defender/releases/tag/v1.0.0).

| Archivo | Contenido |
| --- | --- |
| `NebulaDefender-Setup-x64.exe` | Instalador Windows x64 con juego, accesos directos y desinstalación por usuario. |
| `NebulaDefender-Windows-x64.zip` | Carpeta portable completa: ejecutable, datos y bibliotecas. |
| `NebulaDefender-Web-Local.zip` | WebAssembly, servidor Python, lanzador y ZIP de Windows para la descarga desde la página. |
| `NebulaDefender-Unity-Source.zip` | Código/documentación del commit publicado, escenas, prefabs, materiales, audio, pruebas y siete fuentes Blender. |
| `SHA256SUMS.txt` | Hashes de los cuatro archivos. |

El código también se puede clonar con Git. Cachés del Editor, temporales, preferencias personales y registros completos locales no forman parte de los paquetes.

## Instalación

Para Windows, abre el instalador o extrae todo el ZIP portable y ejecuta `Windows/NebulaDefender.exe`. No requiere Unity ni Visual Studio. La plataforma nativa distribuida es Windows x64; no se ha validado ARM64 ni Android.

Para Web local, extrae todo su ZIP y abre `JUGAR_LOCALHOST.cmd` con Python 3.10 o posterior instalado. La página escucha en `127.0.0.1:8080`; no publica archivos en Internet. Mantén el servidor abierto mientras juegas. La exportación se probó en Chromium; otras combinaciones navegador/hardware quedan pendientes.

## Contenido

Tres sectores, portales físicos, asteroides con fragmentación, drones, jefe con dos fases, salud y vidas, escudo, mejoras, puntos, pausa, opciones, pilotos y ranking local. Registro opcional. Abre tres rutas y vuelve a casa después de derrotar al jefe.

La presentación incluye siete modelos originales Blender/FBX, materiales URP, fuentes editables y herramientas de generación.

## Evidencia y límites

Se conservan 30 PlayMode con el arte actual, 20 EditMode de reglas/guardado, validación de 292 objetos y builds Windows/Web sin errores ni advertencias. El empaquetado para publicación añade los avisos de terceros a Windows y Web. El nuevo instalador pasó QA real de instalación y desinstalación con hashes de **163 archivos**, conservando un archivo ajeno y el registro del usuario; no se recompiló ni se cambió la lógica del juego para añadir estos avisos.

La revisión visual se realizó en Unity y Web. El jugador Windows arrancó, pero su apariencia no pudo certificarse con la captura disponible. Faltan campaña humana completa, escucha, controles físicos de Windows, prueba físicamente desconectada y medición sostenida en el equipo de exposición. La consola Web muestra una advertencia del shader opcional FSR. [Informe detallado](TEST_REPORT.md).

La publicación estará comprobada cuando repositorio, release y descargas sean accesibles y coincidan sus hashes. El resultado se registrará en `Evidence/github-publication.json` después de verificarlo.
