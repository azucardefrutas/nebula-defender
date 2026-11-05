# Inventario y procedencia

| Recurso | Origen | Ubicación |
| --- | --- | --- |
| Nave Aurora | Mesh de fuselaje/alas y primitivas creados para este proyecto | Generated/Prefabs/PlayerShip.prefab |
| Cuatro rocas irregulares | Icosaedro subdividido con semilla y vértices desplazados | Generated/Meshes/Asteroid*.asset |
| Drones, Nemesis Prime, fortaleza | Composición procedural original | Generated/Prefabs y escenas |
| Estrellas | Un mesh estático con 500 triángulos | Generated/Meshes/Stars.asset |
| Nebulosa y planeta | Shaders URP originales | Art/Materials |
| Materiales y escudo | URP Lit; paleta original | Generated/Materials |
| Explosión y fragmentos visuales | Unity Particle System; pool de 16 | Generated/Prefabs/ImpactBurst.prefab |
| Música y efectos | Síntesis original, sin muestras externas | Audio/Music y Audio/SFX |
| Fuente Liberation Sans y recursos TMP | Recursos esenciales de TextMeshPro incluidos por Unity | Assets/TextMesh Pro; conservar sus avisos de licencia |
| Ajustes URP | Plantilla oficial instalada con Unity 6000.3.15f1 | Assets/Settings |

No se descargaron modelos, sonidos comerciales ni recursos de terceros. El contenido generado se puede sustituir desde los prefabs; guardar los modelos definitivos fuera de `Generated`. La herramienta no sobrescribe modelos personalizados.

`Tools/generate_audio.py` reproduce los WAV originales con la biblioteca estándar de Python. No se ejecuta al jugar y no es una dependencia del ejecutable.
