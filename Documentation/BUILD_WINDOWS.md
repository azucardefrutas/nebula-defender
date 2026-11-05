# Windows x64

## Desde el Editor

1. Abrir el proyecto con Unity 6000.3.15f1 y una licencia activa.
2. Instalar el módulo **Windows Build Support** únicamente si falta. En el equipo de desarrollo se encontró el backend Mono x64; no hace falta Visual Studio para usar este backend.
3. Ejecutar `Tools → Nebula Defender → Generate Demo` si no existen las escenas.
4. Revisar el perfil `Assets/Nebula/Generated/Data/Windows-x64.asset` en File → Build Profiles. Plataforma: Windows x64, jugador standalone, Mono, sin Development Build. Escenas: Campaign y CoreSandbox, en ese orden.
5. Ejecutar `Tools → Nebula Defender → Build Windows x64`.

La herramienta revisa `BuildPipeline.BuildPlayer`, exige `BuildResult.Succeeded`, comprueba los archivos esenciales y solo entonces genera el ZIP. Conserva el ZIP previo con una marca temporal si ya existía. La recreación de Build Profiles está adaptada al esquema de Unity 6.3 y detiene la operación si ese esquema cambia.

## Línea de comandos

Con el Editor cerrado, desde la raíz del proyecto, si Python está disponible:

```powershell
python Tools/run_unity.py generate
python Tools/run_unity.py edit-tests
python Tools/run_unity.py play-tests
python Tools/run_unity.py validate
python Tools/run_unity.py build --graphics
python Tools/verify_package.py
```

También se puede usar directamente el Editor, sin Python:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe' -batchmode -quit -buildTarget StandaloneWindows64 -projectPath 'C:\Users\tonyh\OneDrive\Documents\nebula 3D' -executeMethod NebulaDefender.Editor.WindowsBuilder.BuildWindows -logFile 'C:\Users\tonyh\OneDrive\Documents\nebula 3D\Documentation\Evidence\build.log' --burst-disable-compilation
```

La opción de Burst evita la carga de código nativo opcional que Windows bloqueó en este entorno. No cambia las reglas de juego ni la física Rigidbody. No se modifican políticas de seguridad de Windows. URP arrastra algunas dependencias Burst/Collections, pero el código del juego no utiliza DOTS/ECS.

Para investigar una prueba concreta puede añadirse `--filter NombreDeLaPrueba` a los modos de pruebas. Una ejecución filtrada sustituye su XML de resultados; antes de entregar se ejecuta nuevamente la suite completa. `verify_package.py` revisa CRC, archivos del jugador y runtime Mono, y registra tamaño y SHA-256 del ZIP en `Evidence/package-inspection.json`.

## Entrega

```text
Builds/
  Windows/
    NebulaDefender.exe
    NebulaDefender_Data/
    UnityPlayer.dll
    MonoBleedingEdge/
    [otros archivos que produzca Unity]
  NebulaDefender-Windows-x64.zip
```

Copiar o comprimir la carpeta entera. En el otro equipo: extraer primero el ZIP y abrir NebulaDefender.exe. El ranking permanece local a cada computadora.

## Comprobar en el equipo de exposición

Abrir el ejecutable, registrar un piloto, iniciar una misión, mover/girar, disparar, comprobar daños/escudo, pausar/reanudar, completar los tres sectores, volver a abrir y revisar el ranking. Repetir una prueba con Internet desconectado. Probar sonido, modo ventana/pantalla completa y resolución del proyector. Registrar los resultados en TEST_REPORT.md; no deducirlos del resultado de una compilación.

Referencias técnicas consultadas: [Unity 6000.3.15f1](https://unity.com/releases/editor/whats-new/6000.3.15f1), [compilación por línea de comandos](https://docs.unity3d.com/6000.3/Documentation/Manual/build-command-line.html), [BuildPipeline](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html).
