# Progreso — actualizado el 5 de noviembre de 2026

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

Resultado automatizado final: **20 EditMode + 19 PlayMode aprobadas**, ejecutadas nuevamente el 5 de noviembre. Las cuatro pruebas nuevas cubren capacidad reducida del pool, recogidas físicas y láser doble, continuidad de bordes y campaña por impactos reales. El generador se repitió preservando 51 rutas/GUIDs en la entrega inicial. Ver TEST_REPORT.md para el alcance exacto, pruebas pendientes y artefactos. No se marca como terminado el proceso de aceptación académica.

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
