# Informe de pruebas — 4 de noviembre de 2026

## Estado

Implementación jugable y compilada para Windows, con **35 pruebas automatizadas aprobadas**: 20 EditMode y 15 PlayMode. No se declara terminada la validación académica: faltan una campaña completa jugada por una persona, escucha de audio, medición en el equipo de exposición y prueba física con Internet desconectado.

Entorno: Unity **6000.3.15f1**, URP **17.3.0**, Input System **1.19.0**, uGUI/TMP **2.0.0**, Test Framework **1.6.0**. Backend de jugador: Mono, Windows x64. El directorio estaba vacío al inicio; no había una base de pruebas previa ni archivos originales de juego.

## Pruebas ejecutadas

| Grupo | Resultado | Evidencia |
| --- | --- | --- |
| Reglas y guardado EditMode | **20/20 aprobadas**, cero omitidas | Evidence/edit-tests.xml |
| Física, controles e integración PlayMode | **15/15 aprobadas**, cero omitidas | Evidence/play-tests.xml |
| Hito inicial de mecánica central | **6/6 aprobadas** antes de generar la campaña completa | Evidence/core-milestone-passed.xml |
| Referencias de escenas, prefabs, shaders, URP, capas y perfil | Aprobado; **310 objetos revisados** | Evidence/project-validation.txt |
| WAV originales | 15 archivos, mono 22,050 Hz; pico máximo 0.66; sin revisión subjetiva de audio | Evidence/audio-inspection.json |
| Código de red propio | No se encontraron las API de red auditadas en 28 scripts propios | Evidence/source-inventory.json |

EditMode comprueba puntuación sin desbordamiento, tres vidas y derrota, reparaciones limitadas, cuotas de los tres sectores, continuidad del mapa, balance, varios pilotos y selección, guardado/recarga, estadísticas sin duplicación, ranking por piloto, recuperación de respaldo, preservación de archivos dañados, versión futura en solo lectura y rechazo de apodos/esquemas inválidos. Todas las pruebas de guardado usan carpetas temporales independientes del progreso del usuario.

PlayMode comprueba movimiento/giro con las acciones reales de teclado, posición XZ, límite de velocidad, láser contra collider real, puntuación única, árbol de fragmentación finito, caducidad y reutilización del pool, cupo de proyectiles, colisión física que reduce salud, escudo y cooldown, generación/persecución/disparo de drones, derrota, transición de los tres sectores, dos fases del jefe, pausa y bloqueo de armas. Se añadieron pruebas de **Espacio/Shift**, **Escape** y **Enter** mediante eventos de Input System.

La transición completa de campaña utiliza las API de puntuación/daño para verificar estados y condiciones de cierre de forma controlada. **No equivale a una campaña ganada manualmente disparando a todos los objetivos.** Las pruebas físicas de láser y colisión son escenarios separados con contactos reales del motor.

## Ejecutable Windows

La primera compilación de producción terminó con `BuildResult.Succeeded`, cero errores, una advertencia de shader, 95,068,603 bytes y aproximadamente 2 min 4 s. Se localizaron `NebulaDefender.exe`, `NebulaDefender_Data`, `UnityPlayer.dll`, `MonoBleedingEdge` y las demás bibliotecas. El ZIP completo inicial tenía 38,098,528 bytes.

La advertencia indicaba el uso de `pow` con una base potencialmente negativa en la nebulosa. Se reemplazó el cuadrado por multiplicación y se limitó el otro argumento de `pow`. La reconstrucción final y su resumen se registran en **Evidence/build-summary.txt**; ese archivo prevalece sobre los valores del primer build.

El ejecutable real se abrió y se inspeccionó visualmente. Se comprobaron con ratón:

- Menú principal, nave low-poly, planeta, estrellas y nebulosa.
- Jugar sin piloto abre el registro.
- Escritura y registro de `QA Validacion`.
- Selección de sectores con los sectores 2 y 3 bloqueados inicialmente.
- Inicio de Órbita exterior, nave y rocas tridimensionales, HUD, reloj y daño por contacto.
- Cierre normal de la ventana; el JSON conservó el perfil y una partida real de unos 80 segundos, sin puntuaciones inventadas.

La inyección de teclas de la herramienta de control de Windows no permitió certificar las teclas físicas en el jugador; las mismas acciones sí pasaron en Unity Test Framework. Por tanto, el **teclado físico en el ejecutable** sigue siendo una comprobación manual explícita. El HUD dispone además de un botón de pausa funcional, cuya inspección final se registra abajo.

## Incidencias y correcciones

1. La primera ejecución del Editor no tenía licencia activa y terminó con código 198. El usuario intervino en Unity Hub; las siguientes importaciones y pruebas pudieron ejecutarse.
2. La primera generación guardó una referencia inválida a la nave. `NewScene` podía descargar prefabs mantenidos solo por variables locales. El generador ahora recarga los assets después de cruzar esa frontera y exige referencias persistentes. Las escenas posteriores pasaron validación y pruebas.
3. Las primeras pruebas de teclado no tenían una Game View con foco. El fixture configura y después restaura el comportamiento de entrada del Editor para comprobar los mapas en batch. No se eliminaron las aserciones de movimiento.
4. Windows bloqueó una biblioteca JIT opcional de Burst con código 4551. Las ejecuciones siguientes usan `--burst-disable-compilation`; no se modificó la seguridad de Windows. El juego utiliza Rigidbody 3D, sin código DOTS propio.
5. El importador de paquetes TMP es asíncrono en batch. Se extrajeron los esenciales incluidos en Unity de forma síncrona, preservando sus GUIDs y licencias.
6. La prueba de disparo de drones observaba un instante anterior a la cadencia aleatoria. Se cambió por una espera acotada hasta un disparo real; la prueba pasó.
7. La validación intentaba restaurar un conjunto vacío de escenas en batch. Ahora solo restaura un conjunto válido, manteniendo el comportamiento de restauración en el Editor interactivo.

Se conservaron XML de intentos fallidos en Evidence. Los registros completos `.log` permanecen localmente fuera de Git porque incluyen detalles del equipo. No se desactivaron pruebas para obtener un resultado aprobado.

## Criterios de aceptación

| Criterio | Estado y alcance de la evidencia |
| --- | --- |
| Proyecto importable, escenas y referencias | **Aprobado en Editor real** y validación de assets |
| Nave 3D, cámara y asteroides 3D | **Aprobado visualmente** en jugador Windows |
| Movimiento, giro, disparos, colisiones, fragmentación y puntos | **Aprobado automáticamente** en PlayMode; teclado físico del jugador pendiente |
| Drones que persiguen y disparan | **Aprobado automáticamente** en PlayMode |
| Salud, vidas, escudo y derrota | **Aprobado automáticamente**; daño visto también en jugador |
| Pausa y reanudación | **Aprobado automáticamente**, incluida la acción Escape |
| Sectores 1 y 2 completables y transición | **Aprobado en integración controlada**; partidas manuales pendientes |
| Jefe con dos fases, destrucción y victoria final | **Aprobado en integración controlada**; combate manual pendiente |
| Registro de pilotos | **Aprobado** con UI real y guardado |
| Ranking local y estadísticas sin duplicación | **Aprobado automáticamente y consultado visualmente** después de reiniciar el jugador |
| Guardado, recuperación y corrupción | **Aprobado** en veinte pruebas y cierre del jugador |
| Interfaz legible | **Revisada** en menú, registro, selección y primer sector; revisar en proyector |
| Opciones de audio/pantalla/calidad/giro | **Volumen, guardado y regreso desde pausa comprobados**; resoluciones, calidad y pantalla completa pendientes de recorrido manual |
| Música, efectos, doble disparo y reparación | **Implementados**; escucha y recogidas manuales pendientes |
| Funcionar sin Internet | **Diseño y código propios sin red**; prueba con conexión deshabilitada pendiente |
| Ejecutable y ZIP completos | **Build aprobado**; consultar build-summary.txt para el último resultado |
| 60 FPS y compatibilidad con equipos escolares | Instantánea del Editor: **88.4 FPS a 2560×1440**; falta ensayo sostenido y medición en el equipo de exposición |
| Mando físico | **No probado**; bindings preparados |

## Validación final de esta entrega

La reconstrucción final terminó con **cero errores y cero advertencias**, 95,068,582 bytes y 19.4 s de build incremental. `NebulaDefender-Windows-x64.zip` contiene **161 entradas**, ocupa **38,098,515 bytes** y pasó comprobación CRC completa y presencia de ejecutable, datos, UnityPlayer y Mono. SHA-256: `18ed91cff3d9c9aeec2061cefbf8c73604b27d04fa2c27dfafdfa25b0e2c7b03`.

Se repitió `Generate Demo`: **51 rutas e identificadores de recursos generados permanecieron iguales**. Las escenas reconstruidas volvieron a pasar la validación de referencias. Evidencia: `Evidence/idempotence.txt`.

En el jugador final se recuperó el perfil al reiniciar y se consultó el ranking, mostrando la partida real de 01:19 con cero puntos. Se ajustó la música al 20%, se guardó y se comprobó ese valor al abrir opciones desde la pausa. Después se restauró el valor inicial del 45%. Se verificó el botón de pausa del HUD, el reloj detenido en 00:07 durante la edición de opciones y su avance a 00:15 al continuar. El registro del jugador terminó sin excepciones.

El único perfil creado por las pruebas (`QA Validacion`) y sus partidas reales se archivaron como evidencia en `Evidence/qa-native-save.json`, y se retiraron del progreso activo para que el usuario comience con un registro limpio. No se añadieron puntuaciones de demostración al juego ni al ZIP.

La entrega sigue siendo una **versión candidata a validación académica** por las comprobaciones manuales pendientes descritas arriba. Los automatismos no certifican diversión, equilibrio, percepción del sonido, 60 FPS ni ejecución con la conexión físicamente deshabilitada.

También se abrió el Editor interactivo con CoreSandbox, se pulsó Play y se comprobó el inicio directo del vuelo sin menú. Se salió de Play y se dejó la escena abierta con Play Maximized. Unity mostró una instantánea de 88.4 FPS a 2560×1440, 235 batches y 13.1k triángulos; no es una medición sostenida. Evidencia: `Evidence/editor-smoke.txt`. La actualización inicial de versión de materiales URP quedó registrada en ProjectSettings, sin cambios en los materiales generados.
