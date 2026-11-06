# Arquitectura

## Composición

`Assets/Nebula/Scripts` contiene una asamblea de ejecución con namespace `NebulaDefender`; `Editor` y ambas carpetas de pruebas tienen asambleas separadas. El generador conecta referencias explícitas en escenas y prefabs. Los modelos visuales son hijos de los objetos físicos y se pueden sustituir sin reescribir el control.

- `GameSession`: estado de la sesión, inicio y fin de sectores, conexión de servicios y puntos de guardado. No mueve actores ni resuelve impactos.
- `PilotInput`: acciones del nuevo Input System. `PlayerController` lee las acciones en Update y aplica fuerzas y giro de Rigidbody en FixedUpdate.
- `PlayerHealth` y `PilotVitals`: salud, vidas, escudo y protección. `DamageTarget` define el contrato de daño.
- `WeaponController`, `ProjectilePool`, `Projectile`: cadencia, cupo/reutilización y contacto único. El barrido esférico complementa triggers para impedir que un láser rápido atraviese una roca pequeña.
- `CombatDirector`: generación con semilla, seguimiento de amenazas, fragmentación finita, recogidas y llegada del jefe. Cada actor es dueño de su resistencia y marca su muerte antes de notificar.
- `EnemyController`: máquina de estados Patrol, Detect, Pursue, Attack, Reposition, Destroyed. `BossController`: dos fases y salvas en abanico.
- `GameBalance` y `LevelDefinition`: ScriptableObjects de autoría. `MissionProgress` y `RunStatistics`: reglas y contadores de una partida.
- `ArcadeUI`: Canvas/uGUI y TextMeshPro, navegación y HUD. `AudioService`: un canal musical, propulsor, canal UI y doce voces reutilizadas. `CombatFeedback`: dieciséis efectos reutilizados.
- `ProfileRepository`: perfiles, historial y ranking. `LocalSaveStore`: JSON validado, escritura temporal con Flush(true), reemplazo atómico y respaldo. Datos no recuperables o de una versión futura quedan intactos y en modo de solo lectura, con aviso visible.

## Tiempo y límites

La simulación usa tiempo escalado; los menús tiempo no escalado. Pausa y resultados detienen la física mediante `Time.timeScale=0`. El mapa de vuelo se deshabilita fuera del combate. La pérdida de foco pausa la aplicación interactiva. Los bordes transfieren Rigidbody conservando velocidad y exceso de desplazamiento; se puede desactivar la continuidad en el asset de balance.

`CameraRig` recibe explícitamente el jugador y la sesión. Su vista cercana sigue la nave por detrás; C alterna con el encuadre táctico de toda la arena. Se aplica suavizado y se corta el desplazamiento de cámara cuando la nave cruza un borde. La cámara cambia la presentación; la física sigue en XZ. `CombatDirector.NearestObjective` consulta sus listas existentes para guiar a objetivos pendientes fuera del encuadre.

`ArcadeUI.Guidance` es la parte de presentación de informes, ayuda, aprendizaje de controles y HUD. F1 usa una acción de Input System; la ayuda conserva la pausa y vuelve al contexto de origen. Los contadores y barras derivan de `MissionProgress`, nunca de contadores paralelos de interfaz.

`SpaceBackdrop` crea un cielo envolvente con el shader local `Resources/NebulaSky`, un cubemap de reflejos de 32×32 y restos orbitales decorativos sin colliders. Los materiales y el cubemap se crean una vez y se liberan al destruir la escena. El alabeo afecta solo al hijo visual de la nave; las estelas se limpian al reutilizar cada proyectil y los impactos de rocas usan MaterialPropertyBlock.

La reserva inicial de proyectiles alterna ambos equipos y nunca supera `maxProjectiles`, incluso con presupuestos inferiores a 64. El crecimiento posterior mantiene ese mismo límite total.

Windows incluye dos perfiles gráficos seleccionables: **Eficiente** utiliza el asset URP con escala de renderizado 0.8 y sombras de 1024; **Alta** utiliza escala 1.0 y sombras de 2048. La selección se guarda en las preferencias locales. Los perfiles conservan sus índices originales para mantener las preferencias existentes.

Capas 8–13: Player, Asteroids, Enemies, PlayerProjectiles, EnemyProjectiles, Pickups. El generador habilita únicamente las parejas requeridas entre estas capas; los disparos propios no colisionan con su nave. Los decorados no tienen colliders.

## Archivos persistentes

`Application.persistentDataPath/pilots-v1.json`, versión 1, hasta 64 pilotos y 500 partidas. El historial conserva la mejor partida de cada piloto y elimina los registros antiguos que no son su mejor marca al llegar al cupo. Los puntos de guardado actualizan un mismo ID de partida: no duplican partidas ni estadísticas al guardar varias veces. `.bak` contiene el estado anterior; archivos dañados recuperados se conservan aparte. No se usa PlayerPrefs para perfiles ni puntuaciones.

PlayerPrefs almacena únicamente opciones simples. No se guardan credenciales ni se realiza ninguna petición de red desde el juego.

## Autoría y distribución

`DemoGenerator` crea únicamente contenido del proyecto y recursos automáticos identificados en `Assets/Nebula/Generated`. Los assets existentes de balance, materiales y prefabs se reutilizan; las escenas de demostración se regeneran de forma explícita. Guardar escenas personalizadas fuera de esa carpeta. No se borran modelos originales.

`WindowsBuilder` compilará una carpeta completa para Windows x64, verificará el BuildReport y preparará su ZIP. Los resultados reales, incluido cualquier bloqueo del entorno, se documentan en TEST_REPORT.md.
