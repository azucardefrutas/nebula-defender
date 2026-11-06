# Inventario y procedencia

| Recurso | Origen | Ubicación |
| --- | --- | --- |
| Nave Aurora | Mesh de fuselaje/alas y primitivas creados para este proyecto | Generated/Prefabs/PlayerShip.prefab |
| Detalle Aurora | Estabilizadores inclinados, cañones, placas y líneas luminosas originales | Scripts/Player/ShipPresentation.cs |
| Escudo de energía | Shader transparente de borde y barrido, sin textura | Resources/ShieldRim.shader |
| Cuatro rocas irregulares | Icosaedro subdividido con semilla y vértices desplazados | Generated/Meshes/Asteroid*.asset |
| Drones, Nemesis Prime, fortaleza | Composición procedural original | Generated/Prefabs y escenas |
| Cielo, estrellas y nebulosa | Skybox procedural direccional original; sustituye el fondo plano durante ejecución | Resources/NebulaSky.shader |
| Estrellas de respaldo | Mesh estático de 500 triángulos, desactivado al iniciar el cielo nuevo | Generated/Meshes/Stars.asset |
| Planeta | Shader URP original sobre geometría 3D | Art/Materials |
| Reflejos ambientales | Cubemap procedural de 32×32 por cara, creado una vez | SpaceBackdrop en ejecución |
| Restos orbitales | Ocho grupos de tres vigas metálicas a distintas profundidades, sin colisión | SpaceBackdrop en ejecución |
| Estelas láser y destello al impacto | TrailRenderer reutilizado con el proyectil y MaterialPropertyBlock en asteroides | Scripts/Combat y Scripts/Asteroids |
| Materiales y escudo | URP Lit; paleta original | Generated/Materials |
| Explosión y fragmentos visuales | Unity Particle System; pool de 16 | Generated/Prefabs/ImpactBurst.prefab |
| Música y efectos | Síntesis original, sin muestras externas | Audio/Music y Audio/SFX |
| Fuente Liberation Sans y recursos TMP | Recursos esenciales de TextMeshPro incluidos por Unity | Assets/TextMesh Pro; conservar sus avisos de licencia |
| Ajustes URP | Plantilla oficial instalada con Unity 6000.3.15f1 | Assets/Settings |

No se descargaron modelos, sonidos comerciales ni recursos de terceros. El contenido generado se puede sustituir desde los prefabs; guardar los modelos definitivos fuera de `Generated`. La herramienta no sobrescribe modelos personalizados.

`Tools/generate_audio.py` reproduce los WAV originales con la biblioteca estándar de Python. No se ejecuta al jugar y no es una dependencia del ejecutable.
