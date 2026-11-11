# Publicación y descargas

Publicación verificada el **11 de noviembre de 2026**.

- [Repositorio y documentación](https://github.com/azucardefrutas/nebula-defender).
- [Todas las descargas de v1.0.0](https://github.com/azucardefrutas/nebula-defender/releases/tag/v1.0.0).
- [Descarga directa del instalador Windows x64](https://github.com/azucardefrutas/nebula-defender/releases/download/v1.0.0/NebulaDefender-Setup-x64.exe).

El último enlace puede pegarse en un documento para descargar el instalador. Es público y no requiere iniciar sesión en GitHub. Incluye el juego completo; no requiere Unity ni Visual Studio.

## Qué se publicó

La rama `main` contiene el proyecto Unity, código, escenas, prefabs, modelos FBX, siete fuentes editables Blender, audio, documentación y evidencias. El tag `v1.0.0` y el ZIP del proyecto conservan la instantánea del juego `e0421f79466e59e1d02f452353654a0fb7ee8dd1`. Las herramientas y comprobaciones de publicación posteriores están en la rama principal.

La rama `codex/release-payload-v1.0.0` conserva los seis archivos Web compilados en `Distribution/WebGL`, con sus bytes preservados mediante `.gitattributes`. Esto permite ensamblar el paquete web sin recompilar Unity ni depender de una licencia de Editor en GitHub.

## Cómo se verificó

1. La instantánea original de fuente en GitHub coincidió con los 538 archivos locales versionados, incluidos siete `.blend` y siete `.fbx`.
2. El instalador público se descargó completo sin autenticación: 28,240,308 bytes, firma MZ y SHA-256 coincidente con el original que pasó instalación/desinstalación local.
3. El [workflow de ensamblado](https://github.com/azucardefrutas/nebula-defender/actions/runs/43074485629) instaló el mismo ejecutable en modo QA, verificó los 163 archivos y creó el portable a partir de esa lista exacta.
4. El workflow comprobó seis archivos web compilados, CRC de los ZIPs y la identidad del portable incluido dentro del paquete Web local.
5. Las seis descargas públicas respondieron HTTP 200; los digests de las cuatro distribuciones coinciden con `SHA256SUMS.txt`.

Evidencia: [publicación](Evidence/github-publication.json), [ensamblado](Evidence/github-archive-assembly.json) y el archivo `distribution-verification.json` adjunto a Releases.

## Reproducir el ensamblado

En Actions, **Assemble verified distribution packages → Run workflow** ejecuta `Tools/assemble_github_release.py` en un runner Windows estándar. El workflow usa el token temporal del repositorio, no contraseñas o claves guardadas en el código. Sus permisos se limitan a contenidos del repositorio para adjuntar archivos a Releases.

El script verifica los archivos de entrada y conserva un paquete existente únicamente después de comprobar su contenido. Elimina solo registros de carga incompleta de los nombres esperados; no sustituye distribuciones completas desconocidas. Los hashes y el informe se actualizan después de verificar los paquetes.

Los ZIPs pueden tener hashes distintos de los archivos locales previos por sus metadatos de compresión, aunque los archivos del juego sean idénticos. Para verificar una descarga pública se utiliza el manifiesto de Releases. El ensamblado no ejecuta Unity ni una partida; las pruebas de jugabilidad y límites pendientes están en [TEST_REPORT.md](TEST_REPORT.md).
