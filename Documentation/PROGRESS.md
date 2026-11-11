# Progreso — actualizado el 11 de noviembre de 2026

## Repositorio y distribución publicados

Se publicaron el repositorio [nebula-defender](https://github.com/azucardefrutas/nebula-defender) y la [release v1.0.0](https://github.com/azucardefrutas/nebula-defender/releases/tag/v1.0.0), con instalador Windows x64, portable, web local, fuente Unity, hashes e informe de distribución. Se comprobaron los seis enlaces públicos; el instalador se descargó completo y su SHA-256 coincidió con el original probado.

El ensamblado remoto aprobó la comprobación de los 163 archivos del jugador y seis archivos web antes de publicar ambos ZIP. La revisión funcional del juego permanece en el informe del 6 de noviembre; publicar no implica repetir las pruebas de Unity ni resolver las comprobaciones humanas pendientes. Véanse `PUBLISHING.md` y `Evidence/github-publication.json`.

## Windows 1.0 · Vuelve a casa

### Revisión de modelos originales

Se crearon siete fuentes `.blend` con Blender 5.1.2 y sus siete FBX: Aurora, Marauder, Nemesis y cuatro asteroides con cráteres. Ya están integrados en prefabs, escenas, menú y ambas compilaciones. Sus materiales son URP Lit compartidos; no se descargaron modelos ni se consumieron créditos.

Se ejecutaron **30/30 PlayMode** con el arte nuevo (128.17 s), validación de 292 objetos/referencias y validación de escala, orientación, materiales y geometría de los siete FBX. Windows y Web compilaron sin errores ni advertencias. Se verificaron otra vez la instalación/desinstalación real y los 161 archivos del paquete. En localhost se inspeccionaron menú y vuelo libre, movimiento y proyectiles. La versión Web ya incluye también los portales; sustituye el estado web pendiente del registro anterior.

Pendiente: recorrido visual completo de los sectores 2/3 durante una partida humana, escucha y rendimiento en el equipo de exposición. Los renderizados de los cuatro tipos de modelo se revisaron en Unity; esto no sustituye el recorrido humano. Detalles en `ORIGINAL_ART.md` y `TEST_REPORT.md`.

### Registro de la campaña antes de la revisión de arte

- Implementado: campaña de tres rutas con portal físico, victoria al volver a casa, inicio sin registro, asistencia ligera de puntería, primera mejora garantizada, atracción de recogidas, combate más próximo, estelas de motores y cámara estable.
- Ejecutado: **20/20 EditMode y 30/30 PlayMode**. La nueva campaña atraviesa los tres sectores mediante armas, contactos físicos y entrada a los portales en un escenario controlado. La comprobación posterior del radio de entrada pasó **3/3** pruebas de campaña; ya están incluidas en el total, no se suman otra vez.
- Distribución: jugador Windows 1.0 compilado sin errores ni advertencias. Instalador `.exe` creado y ejecutado en una carpeta temporal; los 161 archivos coinciden por SHA-256. Desinstalación comprobada conservando un archivo ajeno y la inscripción real del usuario.
- Pendiente: campaña completa jugada por una persona, escucha, rendimiento en equipo escolar y ejecución con la conexión físicamente deshabilitada.
- Web: localhost conserva la revisión anterior de vuelo abierto. La campaña nueva está en Windows; todavía no se ha recompilado Web para este cambio.
- Meshy: cuatro modelos autorizados, presupuesto máximo de 64 créditos. La cuenta conectada devuelve HTTP 402 «Free plan» incluso después de la confirmación de activación. No hay tareas aceptadas ni créditos consumidos. Modelos externos e integración pendientes; no se atribuye el arte actual a Meshy.

El registro siguiente conserva la evolución y las cifras de entregas anteriores. Los resultados actuales están en `TEST_REPORT.md` y `Evidence/homecoming-full-suite.xml`.

## Historial hasta el 5 de noviembre

Este registro distingue implementación de verificación. No se considera un juego terminado por compilar o por existir sus scripts.

| Fase | Implementación | Evidencia ejecutada | Validación restante |
| --- | --- | --- | --- |
| 1 · Proyecto base | Unity 6.3, URP, Input System, nave low-poly, cámara, físicas XZ | Escena real generada; movimiento/giro y XZ probados; nave y cámara revisadas en jugador Windows | Control humano con teclado físico |
| 2 · Asteroids | Láser, rocas irregulares, daño, fragmentación, puntos, pool y continuidad de bordes | Seis pruebas del hito central pasaron en PlayMode; reglas de bordes y puntuación pasaron en EditMode | Percepción del control y encuadre |
| 3 · Combate | Drones con estados, disparos hostiles, salud, tres vidas, escudo, recogidas y efectos | Generación, persecución, disparo, daños y derrota aprobados en PlayMode | Dificultad y recogidas manuales |
| 4 · Sectores | Tres assets de sector y jefe con dos fases, resultados y desbloqueos | Campaña completada por disparos e impactos reales contra blancos controlados; fase 2 y victoria probadas | Campaña jugada manualmente y dificultad |
| 5 · Interfaz y progreso | Menús, pilotos, ranking local, JSON atómico, respaldo, opciones | Veinte pruebas EditMode; registro, ranking, reinicio, guardado y pausa comprobados en jugador | Variantes de resolución, calidad y pantalla completa |
| 6 · Presentación | Meshes originales, nebulosa/planeta, fortaleza, bloom moderado, música y SFX sintetizados | Menú y vuelo revisados en jugador y Editor; quince WAV inspeccionados, pico máximo 0.66 | Escucha y revisión completa de los sectores 2/3 |
| 7 · Distribución | Perfil Windows x64 Mono y herramienta de build/ZIP | Build final sin errores/advertencias; ejecutable abierto; ZIP completo con CRC verificado | Prueba desconectada y equipo de exposición |

Resultado automatizado vigente: **20 EditMode + 27 PlayMode aprobadas**. Las pruebas cubren capacidad del pool, recogidas y láser doble, campaña por impactos reales, ayuda F1, cámara, control con mouse y ahora vuelo sin perímetro, combate y reaparición lejos del origen, amenazas locales y seguimiento táctico/jefe. El generador se repitió preservando 51 rutas/GUIDs en la entrega inicial. Ver TEST_REPORT.md para el alcance exacto, pruebas pendientes y artefactos. No se marca como terminado el proceso de aceptación académica.

## Incidencias resueltas durante el desarrollo

- Falta de licencia del Editor: Unity Hub requirió la intervención del usuario. La importación posterior se ejecutó correctamente.
- Referencias de prefabs al cruzar la creación de escenas: se recargan explícitamente los assets después de NewScene y se exige persistencia antes de continuar.
- Biblioteca JIT opcional de Burst bloqueada por Windows (4551): las ejecuciones de validación utilizan la opción oficial de desactivación de Burst. No se cambió la seguridad del sistema.
- Entrada simulada sin Game View en foco: las pruebas configuran temporalmente el comportamiento del Editor para enviar entradas al juego y restauran la preferencia al terminar. El hito central pasó después de esa corrección.
- Importación asíncrona de esenciales TMP en batch: se incluyeron los recursos suministrados por Unity de forma síncrona, conservando GUIDs y licencias.
- Reserva de proyectiles superior al límite configurable: se reprodujo con límite 5 y 64 objetos; la reserva ahora respeta el presupuesto total y la regresión pasa.
- Selector de calidad sin alternativas en Windows: se incluyeron los perfiles Eficiente y Alta, con ajustes URP distintos y sus índices originales.

## Entrega para jugar: Windows y página local

- `JUGAR_WINDOWS.cmd`: inicia el ejecutable ya compilado.
- `JUGAR_LOCALHOST.cmd`: abre el servidor y la página en http://localhost:8080.
- WebAssembly del mismo juego compilado en copia aislada con el soporte Web existente de Unity 6.4; fuente principal en Unity 6.3 preservada.
- Exportación Web aprobada con cero errores/advertencias de build. Movimiento, giro, disparos, destrucción, escudo, pausa, vuelo sin registro, pantalla completa y recuperación de perfil/ranking comprobados en Chromium.
- ZIP de Windows descargado desde la página y verificado íntegro. Ver TEST_REPORT.md para advertencias de navegador y límites de las pruebas.

## Mejora de profundidad y orientación del jugador

- Cámara de seguimiento detrás y encima de la nave por defecto; C cambia a la vista táctica. La simulación continúa en XZ.
- Inclinación visual de la nave, estelas láser, reflejos metálicos, cielo direccional y restos orbitales a distintas profundidades.
- Preparación de misión con objetivos reales, ayuda F1, aprendizaje de cuatro controles, cuotas visibles, recarga del escudo y avisos de combate.
- Suite vigente: 20/20 EditMode y 21/21 PlayMode. Las dos pruebas adicionales verifican ayuda/pausa y encuadre/cambio de cámara.
- Windows y Web reconstruidos con cero errores y advertencias de compilación. Revisión visual de vuelo 3D, vista táctica, ayuda y briefing en Chromium. El ZIP actualizado pasó CRC y la descarga local coincide con su SHA-256.
- Pendiente: revisión humana del nuevo encuadre durante los tres sectores, nuevo recorrido visual en el ejecutable y rendimiento sostenido en el equipo de exposición. Las capturas están en Documentation/Evidence.

## Mouse y presentación sencilla

- Mouse para apuntar, izquierdo para disparar, derecho para propulsión y botón central para escudo. WASD/Espacio siguen funcionando.
- Cámara estabilizada durante puntería y frenado suave al soltar el avance con mouse.
- HUD compacto, guía breve, preparación corta y pistas que desaparecen; se retiraron los paneles grandes de aprendizaje y explicación.
- Detalles adicionales de la nave, escudo transparente de borde y nueva superficie procedural del planeta.
- Las tres pruebas nuevas de mouse pasan, incluida destrucción por contacto real de proyectiles y clics de pausa sin disparos accidentales. Se corrigió el encuadre de UI en proporción 4:3.
- Windows reconstruido, ZIP íntegro y ejecutable abierto/revisado visualmente. La comprobación de dificultad humana y rendimiento sostenido continúa pendiente.

## Vuelo sin perímetro

- Eliminadas las marcas visibles y desactivado el salto entre bordes en el balance entregado; no se detiene ni devuelve al jugador al cruzar la zona anterior.
- Disparos y fragmentos funcionan fuera de esa zona. Perder una vida conserva la posición actual.
- Asteroides y drones se generan alrededor del piloto; los actores muy lejanos se reutilizan fuera de cámara sin sumar puntos o cambiar las cuotas.
- La cámara táctica sigue la nave, el jefe aparece cerca del combate y el regreso al menú recupera su encuadre.
- Suite PlayMode: 27/27 aprobadas. La comprobación ampliada del regreso de cámara al menú también pasó por separado, sin sumarse de nuevo al total.
