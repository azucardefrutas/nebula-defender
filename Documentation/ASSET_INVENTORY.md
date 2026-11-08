# Inventario y procedencia

| Recurso | Origen | Ubicación |
| --- | --- | --- |
| Nave Aurora | Modelo original creado con Blender 5.1.2: casco, cabina, alas, motores y cañones | Art/Models/AuthoredV1/Aurora.fbx; ArtSource/AuthoredV1/Aurora.blend |
| Propulsión Aurora | Resplandor y dos estelas dinámicas; detalle de superficie integrado en FBX | Scripts/Player/ShipPresentation.cs |
| Escudo de energía | Shader transparente de borde y barrido, sin textura | Resources/ShieldRim.shader |
| Cuatro rocas con cráteres | Mallas originales Blender, semillas 170–173, cuatro materiales de basalto | Art/Models/AuthoredV1/Asteroid*.fbx; fuentes en ArtSource/AuthoredV1 |
| Marauder y Nemesis Prime | Modelos originales Blender; reactor del jefe separado para animación | Art/Models/AuthoredV1/Marauder.fbx y Nemesis.fbx |
| Fortaleza | Composición procedural original de la estación de fondo | Generated/Scenes |
| Cielo, estrellas y nebulosa | Skybox procedural direccional original; sustituye el fondo plano durante ejecución | Resources/NebulaSky.shader |
| Estrellas de respaldo | Mesh estático de 500 triángulos, desactivado al iniciar el cielo nuevo | Generated/Meshes/Stars.asset |
| Planeta | Shader URP original sobre geometría 3D | Art/Materials |
| Reflejos ambientales | Cubemap procedural de 32×32 por cara, creado una vez | SpaceBackdrop en ejecución |
| Restos orbitales | Ocho grupos de tres vigas metálicas a distintas profundidades, sin colisión | SpaceBackdrop en ejecución |
| Estelas láser y destello al impacto | TrailRenderer reutilizado con el proyectil y MaterialPropertyBlock en asteroides | Scripts/Combat y Scripts/Asteroids |
| Materiales y escudo | URP Lit; paleta original | Generated/Materials |
| Explosión y fragmentos visuales | Unity Particle System; pool de 16 | Generated/Prefabs/ImpactBurst.prefab |
| Portal de salto | Torus original de 64 × 8 vértices, doce bobinas e iris de energía | Scripts/Levels/JumpGate.cs |
| Estelas de motores | Dos TrailRenderer asociados a propulsión; limpieza al reposicionar | Scripts/Player/ShipPresentation.cs |
| Música y efectos | Síntesis original, sin muestras externas | Audio/Music y Audio/SFX |
| Fuente Liberation Sans y recursos TMP | Recursos esenciales de TextMeshPro incluidos por Unity | Assets/TextMesh Pro; conservar sus avisos de licencia |
| Ajustes URP | Plantilla oficial instalada con Unity 6000.3.15f1 | Assets/Settings |

No se descargaron modelos, sonidos comerciales ni recursos de terceros. El contenido generado se puede sustituir desde los prefabs; guardar los modelos definitivos fuera de `Generated`. La herramienta no sobrescribe modelos personalizados.

La revisión de arte del 6 de noviembre incluye siete FBX y siete fuentes `.blend` propios. Los modelos anteriores de mallas simples permanecen como recursos del proyecto e historial Git. Consulta [ORIGINAL_ART.md](ORIGINAL_ART.md) para regeneración, paleta y alcance de la integración.

`Tools/generate_audio.py` reproduce los WAV originales con la biblioteca estándar de Python. No se ejecuta al jugar y no es una dependencia del ejecutable.

## Meshy · solicitud pendiente, 6 de noviembre de 2026

Plan autorizado: Aurora, dron, Nemesis Prime y roca, con texturas, presupuesto estimado de 60 créditos y máximo de 64. El servicio rechaza la cuenta conectada con HTTP 402 «Task creation on the free plan is no longer supported», incluso después de que el usuario confirmó la activación. Saldo comprobado: 140; lista de tareas vacía. No existen modelos Meshy aceptados, importados o incluidos en la entrega actual. No se gastaron créditos en estas solicitudes rechazadas. La cuenta necesita reconocer el plan activo antes de continuar; no se ejecutan generaciones adicionales ni se adquiere una suscripción automáticamente.
