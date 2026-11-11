# NEBULA DEFENDER: GALACTIC ASSAULT

Shooter espacial arcade 3D, individual y offline para Windows. Creado para la Semana de Ciencia y Tecnología 2026 de la Universidad Politécnica de Bacalar. Docente: Mtro. Esdras Chuc C.

**Vuelve a casa:** despeja tres sectores, abre sus portales de salto y cruza el último después de derrotar a Nemesis Prime. Combate con mouse, asteroides que se fragmentan y vuelo sin perímetro. Registro de pilotos y ranking guardados únicamente en tu computadora.

## Jugar en este equipo

- **Instalador Windows 1.0:** abre `Builds/NebulaDefender-Setup-x64.exe`. Incluye el juego completo, accesos directos y desinstalador; no requiere Unity ni Visual Studio.
- **Windows sin instalar:** abre `JUGAR_WINDOWS.cmd` o `Builds/Windows/NebulaDefender.exe`.
- **Como página:** abre `JUGAR_LOCALHOST.cmd` y entra a **http://localhost:8080**. Mantén el servidor abierto mientras juegas. Incluye la campaña de portales y los modelos originales nuevos. Si tenías abierta una versión anterior, recarga la página desde el menú.
- **Otra computadora Windows:** extrae todo `Builds/NebulaDefender-Windows-x64.zip` y ejecuta `Windows/NebulaDefender.exe`.

La página ejecuta una exportación WebAssembly del mismo juego Unity. El proyecto fuente y el ejecutable usan Unity 6.3; la exportación web utiliza una copia de compilación con el Unity 6.4 que ya tiene soporte web en este equipo. Consulta [BUILD_WEB.md](BUILD_WEB.md). Los perfiles del navegador y de Windows se guardan por separado.

## Abrir y probar primero la nave

En este equipo puedes hacer doble clic en **ABRIR_NEBULA.cmd**: abre Unity 6.3 directamente en CoreSandbox y evita la biblioteca opcional de Burst que Windows bloqueó durante la importación. Si el proyecto ya está abierto, vuelve a esa ventana en lugar de abrir una segunda instancia.

1. En Unity Hub, usa **Add → Add project from disk** y elige esta carpeta completa (`nebula 3D`).
2. Ábrela con **Unity 6.3 LTS, 6000.3.15f1**. Se necesita una licencia válida del Editor. La importación inicial puede tardar.
3. Usa **Tools → Nebula Defender → Open Core Sandbox** y pulsa **Play**. Comienza el vuelo directamente, sin registro ni menú previo.
4. **Mouse** apunta, **clic izquierdo** dispara y **clic derecho** avanza. Pulsa la **rueda** o **Shift** para el escudo. **Esc** pausa. WASD y Espacio siguen disponibles.

Si faltan recursos generados, usa **Tools → Nebula Defender → Generate Demo**. La primera apertura de un proyecto sin escena generada también intenta crearlos automáticamente. No hay que arrastrar componentes ni montar referencias a mano.

## Campaña

Vuela sin perímetro ni saltos entre bordes. La cámara sigue la nave y estabiliza su orientación al apuntar con mouse. Pulsa **C** para la vista táctica y **F1** para una guía breve. La pantalla mantiene salud, objetivo, puntos y escudo en los bordes, dejando libre el combate.

Usa **Tools → Nebula Defender → Open Campaign** y pulsa Play. **JUGAR** abre directamente la preparación de misión: registrar un piloto es opcional. Destruye los objetivos indicados y, cuando aparezca, entra en el centro del portal azul para cerrar el sector. Continúa el viaje desde los resultados. En el tercer sector, derrota al jefe y cruza el último portal para volver a casa. Al perder las tres vidas termina la partida.

Las partidas de invitado permiten jugar los tres sectores, sin registrar resultados en el ranking. Para conservar estadísticas y desbloqueos entre sesiones, selecciona un piloto local. El láser doble aparece por primera vez tras destruir tres rocas; las recogidas cercanas se aproximan a la nave.

El ranking muestra las mejores diez marcas locales, una por piloto. Los perfiles, estadísticas y resultados están en `Application.persistentDataPath/pilots-v1.json` con respaldo `.bak`. La ruta habitual en Windows es `%USERPROFILE%\AppData\LocalLow\UPB Arcade\Nebula Defender Galactic Assault`. Las preferencias simples se guardan por separado en PlayerPrefs.

## Compilar y distribuir

Con el módulo Windows instalado, selecciona **Tools → Nebula Defender → Build Windows x64**. Verifica el resultado real en `Documentation/Evidence/build-summary.txt`.

La salida es `Builds/Windows/NebulaDefender.exe` junto con `NebulaDefender_Data`, `UnityPlayer.dll` y sus bibliotecas. El ZIP `Builds/NebulaDefender-Windows-x64.zip` incluye la carpeta completa. Para generar el instalador con NSIS ya instalado, ejecuta `python Tools/build_installer.py`. Puedes distribuir el instalador solo, o el ZIP completo; el ejecutable interno de Unity requiere sus archivos acompañantes.

Consulta [las instrucciones de compilación](BUILD_WINDOWS.md) y [el informe de pruebas](TEST_REPORT.md) para conocer los resultados obtenidos y las verificaciones pendientes. La existencia de código no equivale a una función verificada.

## Tecnología y organización

La revisión visual incorpora modelos originales de Aurora, Marauder, Nemesis Prime y cuatro asteroides con cráteres, creados en Blender 5.1.2 e importados como FBX. Incluye sus fuentes editables y una herramienta de integración; ver [modelos originales](ORIGINAL_ART.md). No hace falta Blender para abrir el juego o compilar los FBX existentes.

Unity 6.3 LTS · C# · URP 17.3.0 · Input System 1.19.0 · Unity Physics 3D · uGUI/TextMeshPro · Unity Test Framework 1.6.0 · Git local. Sin backend, servicios online ni recursos de pago. La importación de paquetes puede requerir conexión en una computadora nueva; el ejecutable no depende del Editor ni de los paquetes de desarrollo.

- `Assets/Nebula/Scripts`: sistemas modulares del juego.
- `Assets/Nebula/Editor`: generación, validación y compilación.
- `Assets/Nebula/Generated`: escenas, prefabs y configuraciones automáticas.
- `Assets/Nebula/Tests`: pruebas de reglas, guardado e integración.
- `Documentation`: diseño, arquitectura, controles, sectores, progreso e inventario.

Los prefabs y assets existentes de Generated se reutilizan al regenerar; las escenas automáticas se reconstruyen. Guarda tus escenas y modelos personalizados fuera de esa carpeta. Sustituye los hijos visuales de las naves sin cambiar el Rigidbody principal.

Este proyecto es independiente de Dead Tap. No se modificó ese juego ni se configuraron servicios externos.
