# Nebula Defender: Galactic Assault

Proyecto 3D independiente para la Semana de Ciencia y Tecnología 2026, Universidad Politécnica de Bacalar. Docente: Mtro. Esdras Chuc C. No comparte código ni recursos con Dead Tap.

## Experiencia

Shooter arcade individual, offline, desde una cámara elevada en perspectiva. El piloto de la nave experimental Aurora protege tres sectores frente a máquinas hostiles. Partidas cortas y controles de Asteroids: acelerar, conservar inercia, girar para orientar el cañón, frenar y esquivar. La simulación transcurre en XZ; Y aporta volumen visual.

## Reglas

- Tres vidas, cien puntos de salud por vida. Contacto con roca: 25; dron: 30; láser enemigo: 15; láser del jefe: 18.
- Después de perder una vida: reaparece en el centro, salud restaurada e invulnerabilidad de tres segundos. Entre impactos hay protección de 0.65 s.
- Escudo: 2.8 s de duración y 10 s entre activaciones. El indicador también muestra la protección de reaparición.
- Láser: 25 de daño, intervalo de 0.18 s, vida máxima de 1.4 s; objetos reutilizados y cupo global de 144.
- Asteroide grande: 75 de salud, 150 puntos; mediano: 50/100; pequeño: 25/50. Grande produce dos medianos; mediano dos pequeños. Cupo de 42 rocas.
- Dron: 200 puntos. Jefe: 2,000. Completar sector: 500. Sector sin daño: 250 adicionales.
- Reparación poco frecuente: +30 de salud sin superar 100. Disparo doble: catorce segundos; disponible como recogida ocasional de drones.
- La pausa congela física, generadores, cadencias y reloj. Los menús continúan respondiendo con tiempo no escalado.

## Campaña

1. Órbita exterior: diez asteroides destruidos.
2. Cinturón de asteroides: doce asteroides y seis drones.
3. Fortaleza Némesis: cuatro drones de defensa, después Nemesis Prime. La fase 2 comienza por debajo del 50% de salud y reduce el intervalo entre salvas de 1.9 a 1.15 s.

Cada destrucción de una roca, incluidos sus fragmentos, cuenta una vez. Al alcanzar las cuotas se cierra el combate y se retiran las amenazas; ninguna roca residual, recogida o bala pendiente bloquea el avance. La siguiente zona se desbloquea únicamente al ganar el sector anterior. Las vidas y la puntuación continúan entre sectores; se reparan 25 puntos al avanzar.

## Presentación y accesibilidad

Arte low-poly original generado mediante geometría y primitivas; azul/cian para Aurora, naranja para amenazas. Fondo con estrellas, nebulosa, planeta y fortaleza a distintas profundidades. HUD de alto contraste escalado desde 1920×1080. Volumen de música/efectos, sensibilidad, calidad, resolución, pantalla y vibración configurables. Sin publicidad, cuentas online, comercio ni multijugador.

## Progreso local

Varios pilotos mediante apodo, identificadores únicos y estadísticas agregadas. Ranking de los diez mejores pilotos, una mejor partida por piloto, indicando sector y supervivencia. No es una clasificación global. Se conserva una interfaz para sustituir su proveedor en el futuro, sin implementar servicios remotos.

Los valores anteriores describen la configuración de código inicial. La dificultad final y la calidad audiovisual requieren pruebas humanas; consultar TEST_REPORT.md.
