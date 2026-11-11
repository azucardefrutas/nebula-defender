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

**Publicación comprobada:** repositorio y release son públicos. Los seis archivos adjuntos responden HTTP 200 sin autenticación. El instalador se descargó completo y coincidió por SHA-256 con el original verificado. Los hashes de las cuatro distribuciones coinciden con el manifiesto publicado y con los digests de GitHub. Evidencia: [github-publication.json](Evidence/github-publication.json).

Los ZIP portable y web se ensamblaron en un runner Windows estándar de GitHub a partir del instalador publicado y de los seis archivos web compilados. Se verificaron los 163 archivos instalados, los archivos web y el ZIP Windows incluido dentro de Web local. No se recompiló Unity. [Ejecución aprobada](https://github.com/azucardefrutas/nebula-defender/actions/runs/43074485629). El informe `distribution-verification.json` también se adjunta a la release.

El proyecto Unity publicado corresponde a la instantánea `e0421f79466e59e1d02f452353654a0fb7ee8dd1`. La rama principal conserva además las herramientas y evidencias posteriores de publicación. Los ZIP ensamblados en GitHub pueden tener un SHA distinto de los ZIP locales por sus metadatos de archivo; se verificó la identidad del contenido del jugador y de los archivos web. Para las descargas públicas utiliza el `SHA256SUMS.txt` de Releases.
