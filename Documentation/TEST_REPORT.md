# Informe de pruebas — actualizado el 5 de noviembre de 2026

## Estado

Implementación jugable y compilada para Windows y navegador, con **41 pruebas automatizadas aprobadas**: 20 EditMode y 21 PlayMode. No se declara terminada la validación académica: faltan una campaña completa jugada por una persona, escucha de audio, medición en el equipo de exposición y prueba física con Internet desconectado.

Entorno: Unity **6000.3.15f1**, URP **17.3.0**, Input System **1.19.0**, uGUI/TMP **2.0.0**, Test Framework **1.6.0**. Backend de jugador: Mono, Windows x64. El directorio estaba vacío al inicio; no había una base de pruebas previa ni archivos originales de juego.

## Pruebas ejecutadas

| Grupo | Resultado | Evidencia |
| --- | --- | --- |
| Reglas y guardado EditMode | **20/20 aprobadas**, cero omitidas | Evidence/edit-tests.xml |
| Física, controles e integración PlayMode | **21/21 aprobadas**, cero omitidas | Evidence/play-tests.xml |
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

## Validación de la entrega del 4 de noviembre

La reconstrucción final terminó con **cero errores y cero advertencias**, 95,068,582 bytes y 19.4 s de build incremental. `NebulaDefender-Windows-x64.zip` contiene **161 entradas**, ocupa **38,098,515 bytes** y pasó comprobación CRC completa y presencia de ejecutable, datos, UnityPlayer y Mono. SHA-256: `18ed91cff3d9c9aeec2061cefbf8c73604b27d04fa2c27dfafdfa25b0e2c7b03`.

Se repitió `Generate Demo`: **51 rutas e identificadores de recursos generados permanecieron iguales**. Las escenas reconstruidas volvieron a pasar la validación de referencias. Evidencia: `Evidence/idempotence.txt`.

En el jugador final se recuperó el perfil al reiniciar y se consultó el ranking, mostrando la partida real de 01:19 con cero puntos. Se ajustó la música al 20%, se guardó y se comprobó ese valor al abrir opciones desde la pausa. Después se restauró el valor inicial del 45%. Se verificó el botón de pausa del HUD, el reloj detenido en 00:07 durante la edición de opciones y su avance a 00:15 al continuar. El registro del jugador terminó sin excepciones.

El único perfil creado por las pruebas (`QA Validacion`) y sus partidas reales se archivaron como evidencia en `Evidence/qa-native-save.json`, y se retiraron del progreso activo para que el usuario comience con un registro limpio. No se añadieron puntuaciones de demostración al juego ni al ZIP.

La entrega sigue siendo una **versión candidata a validación académica** por las comprobaciones manuales pendientes descritas arriba. Los automatismos no certifican diversión, equilibrio, percepción del sonido, 60 FPS ni ejecución con la conexión físicamente deshabilitada.

También se abrió el Editor interactivo con CoreSandbox, se pulsó Play y se comprobó el inicio directo del vuelo sin menú. Se salió de Play y se dejó la escena abierta con Play Maximized. Unity mostró una instantánea de 88.4 FPS a 2560×1440, 235 batches y 13.1k triángulos; no es una medición sostenida. Evidencia: `Evidence/editor-smoke.txt`. La actualización inicial de versión de materiales URP quedó registrada en ProjectSettings, sin cambios en los materiales generados.

## Continuación — 5 de noviembre de 2026

Se reprodujo un fallo del pool configurable: al inicializarlo con capacidad 5 se creaban 64 objetos. El XML `Evidence/play-tests-capacity-before-fix.xml` conserva la aserción fallida (esperado ≤5, observado 64). La reserva inicial ahora alterna equipos y está limitada por la capacidad total. La prueba comprueba también disparos de ambos equipos, agotamiento y reutilización.

En el ejecutable anterior se comprobó con ratón el paso a pantalla completa, el regreso a ventana y el cambio de 1600×900 a 1680×1050. La interfaz continuó siendo legible. Al revisar Calidad se encontró que únicamente PC se incluía en Windows; el botón no ofrecía una alternativa real. Se incluyeron los dos perfiles existentes y se renombraron **Eficiente** y **Alta**, conservando sus índices y assets URP.

La prueba nueva `ThreeSectorsCanCompleteThroughWeaponAndColliderHits` completó los tres sectores mediante `WeaponController.TryFire`, contactos reales de proyectiles, daño, fragmentación, puntos y condiciones de cierre, sin llamar directamente a las API de daño o puntuación. Los blancos se inmovilizan y colocan en un carril; verifica la integración del combate hasta la victoria, **no la dificultad ni una campaña humana**. Evidencia aislada aprobada: `Evidence/collider-campaign-passed.xml`.

Se añadieron comprobaciones físicas de reparación (+30 una sola vez), recogida del láser doble, dos proyectiles por disparo y caducidad de la mejora conservada durante pausa, además de continuidad de bordes con velocidad y orientación preservadas. La primera preparación de la prueba de campaña eliminaba sus propios blancos; se corrigió reiniciando el sector antes de disparar. Se conservaron los intentos fallidos. Otra ejecución se interrumpió al detectar una espera de tiempo escalado que quedaba suspendida por la pantalla de resultados; el test ahora espera con tiempo real. Estos fueron problemas del escenario de pruebas, sin cambios en las reglas del juego.

La inyección de Enter desde la herramienta de Windows volvió a no producir una acción visible. El teclado físico del jugador continúa pendiente; las pruebas de entrada en Unity no sustituyen esa comprobación.

La ejecución final completa aprobó **20/20 EditMode y 19/19 PlayMode**, cero fallos y cero omisiones. La suite PlayMode tardó 77.91 segundos. El resultado aislado de la campaña no se sumó como una prueba adicional al total: ya forma parte de las 19.

## Entrega Windows y localhost solicitada posteriormente

El paquete Windows del 5 de noviembre contiene 161 entradas y ocupa **38,166,267 bytes**. Su SHA-256 es `3798072b9a4546a6e653861215d308c96f7520e8472ba39cd977c2b603818781`. La comprobación CRC y de archivos requeridos pasó. Se corrigió el verificador para admitir los datos comprimidos de Unity en `data.unity3d`, además de `globalgamemanagers` en builds sin ese empaquetado. El juego no carecía de sus datos; la comprobación anterior exigía el formato equivocado. El .exe se abrió nuevamente y queda disponible mediante `JUGAR_WINDOWS.cmd`.

Se creó una exportación WebAssembly del mismo juego y se sirvió en **http://localhost:8080**. El proyecto principal permanece en Unity 6000.3.15f1/URP 17.3; la exportación usa una copia en `BuildWork/UnityWeb` con Unity **6000.4.6f1 y URP 17.4**, aprovechando su módulo Web instalado. Se canceló la descarga del módulo faltante de 6.3; no se instaló otra versión del Editor ni se actualizaron los paquetes del proyecto principal.

La primera compilación Web aprobó sin errores ni advertencias en 8 min 43 s. La reconstrucción con los controles de pantalla adaptados al navegador y el vuelo sin registro aprobó con **cero errores y cero advertencias**, **16,822,869 bytes** y **4 min 41 s**. Resumen vigente: `Evidence/web-build-summary.txt`.

Se ejecutó Chromium con Playwright sobre la página real. Se verificaron menú, registro, selección de sectores, entrada a misión, W/D para movimiento y giro, Espacio con proyectiles visibles, Shift con escudo y Escape para pausa/reanudación. Los disparos destruyeron cuatro asteroides en la captura de combate (250 puntos); otro impacto pendiente elevó la marca guardada a **300 puntos**. Al volver al menú y recargar, el piloto `QANavegador` y el ranking con 300 puntos/01:10 se recuperaron desde IndexedDB. No se asignó ninguna puntuación directamente. El perfil pertenece únicamente al navegador aislado de pruebas, no al navegador habitual del usuario ni a los perfiles de Windows.

En la exportación final se comprobó además **VUELO RÁPIDO / SIN REGISTRO**, que inicia CoreSandbox directamente, y se repitieron los controles. El botón de pantalla completa devolvió `document.fullscreenElement=true`. El enlace de Windows descargó el ZIP completo desde localhost; el archivo descargado pasó la misma comprobación CRC y SHA-256.

La comprobación HTTP validó el servidor, los tres archivos gzip, sus tamaños, `Content-Encoding: gzip`, `Content-Type: application/wasm` y la firma del módulo Wasm descomprimido. Evidencia: `Evidence/web-http-check.json` y las capturas `web-final-controls.png`, `web-combat.png`, `web-ranking-restored.png` y `web-quickflight.png`.

Después de corregir el icono de la página, la consola del navegador mostró **cero errores**. Chromium emitió seis advertencias de consulta de formatos WebGL y una del shader opcional de escalado FSR no compatible. Se conservaron en el registro; no se ocultaron ni se interpretan como ausencia de advertencias en ejecución. El menú, escenario y combate renderizaron correctamente. La revisión de audio subjetiva, la campaña humana completa, el teclado físico del ejecutable y el rendimiento sostenido en el equipo de exposición siguen pendientes.

## Mejora visual 3D y usabilidad

La cámara predeterminada sigue ahora a la nave desde atrás y arriba, con interpolación y ajuste al cruzar un borde. C alterna con la vista táctica. El modelo visual se inclina al girar sin cambiar las restricciones físicas. El escenario incorpora cielo direccional procedural, reflejos ambientales y ocho grupos de restos a distintas profundidades; los láseres dejan estelas y los asteroides muestran un destello al recibir daño. No se añadieron recursos externos.

Se añadieron preparación de misión, ayuda F1, aprendizaje contextual de los cuatro controles principales, indicadores de objetivos fuera de pantalla, progreso de cuotas, protección/recarga del escudo y avisos de combate. Los objetivos continúan usando los mismos datos y condiciones de victoria. En Chromium se recorrió registro → selección → VER MISIÓN → COMENZAR MISIÓN, y se comprobaron movimiento, giro y disparos. También se revisaron F1 y el cambio de cámara C. Las capturas son `chase-3d-final.png`, `tactical-final.png`, `help-final.png`, `briefing-final.png` y `campaign-3d-start.png` en Evidence. La consola mantuvo cero errores y las siete advertencias WebGL/FSR previamente descritas.

La última suite aprobó **20/20 EditMode y 21/21 PlayMode**, cero fallos y omisiones; PlayMode tardó 78.63 s. Las nuevas pruebas `F1HelpFreezesCombatAndReturnsToFlight` y `Close3DCameraKeepsShipVisibleAndCChangesToTacticalView` comprueban pausa/reanudación desde ayuda, encuadre de la nave y alternancia real por teclado. Un intento anterior acumuló 30,281 s y tres timeouts durante una ejecución prolongada/interrumpida; se conserva en `play-tests-polish-timeouts.xml`. La repetición completa aprobó sin desactivar pruebas ni ampliar sus límites.

Un primer intento de shader usó el identificador reservado `point`; las compilaciones detectaron tres errores y se rechazaron, aunque Unity devolviera inicialmente Succeeded. Los resúmenes fallidos se conservan en `build-sky-attempt.txt` y `web-sky-attempt.txt`. Se corrigió a `starCenter`, se amplió el validador a todos los shaders propios y se redujo la densidad/brillo del cielo tras inspección visual. El validador final pasó.

Compilaciones vigentes: Windows **95,169,530 bytes**, 7.14 s incremental; Web **16,852,712 bytes**, 11.84 s incremental. Ambas tienen **cero errores y cero advertencias de build**. El proyecto fuente sigue en Unity 6.3; Web usa la copia aislada 6.4 descrita arriba. El ZIP Windows contiene **161 entradas**, ocupa **38,175,678 bytes** y pasó CRC y presencia de todos los archivos requeridos. SHA-256: `e73ccf54e156eebadf2d5fa966c627ac97914767b953768bf609fc6efd7a9d30`. `web-http-check.json` registra la comprobación actual de los assets servidos y de la descarga Windows.

La revisión visual de esta mejora se realizó en WebGL. El nuevo ejecutable se compiló y empaquetó; su recorrido visual actualizado, el equilibrio de los tres sectores, la escucha y el rendimiento sostenido siguen pendientes. No se toma la compilación como prueba de jugabilidad humana.
