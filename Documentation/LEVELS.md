# Sectores

Los tres sectores comparten escena de campaña, físicas, armas y reglas. Sus datos son assets `LevelDefinition`, no copias independientes del juego.

| Sector | Ambiente | Objetivo | Configuración inicial |
| --- | --- | --- | --- |
| 1 · Órbita exterior | Planeta oceánico, azul y cian | Destruir 10 rocas | 5 rocas, velocidad 2, sin drones |
| 2 · Cinturón de asteroides | Nebulosa violeta | 12 rocas y 6 drones | 7 rocas, velocidad 2.8; hasta 2 drones simultáneos |
| 3 · Fortaleza Némesis | Fortaleza y energía naranja | 4 drones de defensa, después jefe | 4 rocas, velocidad 3.2; hasta 2 drones; jefe con 1,100 de salud |

Todos los tamaños y fragmentos destruidos cuentan como una roca. El actor deja de aceptar daño antes de asignar sus puntos, evitando la doble contabilización. Si faltan rocas para la cuota y hay menos de tres, el director genera otra grande lejos del piloto. Los drones se reponen solo mientras su cuota esté pendiente.

El jefe aparece únicamente después de las cuatro defensas. No se reponen drones ni se añaden enemigos tras el objetivo cumplido. La victoria limpia todas las amenazas y balas sin exigir perseguir fragmentos residuales.

Las semillas de los sectores se almacenan en sus assets. Se reproduce la distribución inicial, no una simulación físicamente determinista entre equipos distintos. La dificultad debe medirse con partidas reales antes de una exposición.
