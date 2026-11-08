# Modelos originales · Blender y Unity

Se diseñaron modelos propios para Nebula Defender con Blender **5.1.2** instalado en este equipo. No se usaron generaciones pagadas, bibliotecas comerciales, modelos descargados ni créditos de Meshy/Tripo.

## Dirección visual

- **Aurora:** interceptor blanco y cobalto, proa larga, cabina facetada elevada, alas barridas con espesor, motores con toberas y cámara iónica, dos estabilizadores y cañones laterales. La geometría permite reconocer proa y popa al girar.
- **Marauder:** silueta de pinza, dos alas adelantadas, blindaje oscuro, ojo de puntería y líneas rojas. Se distingue del jugador por forma y color.
- **Nemesis Prime:** nave ancha con seis torretas, plataformas de blindaje, puente elevado, seis motores y reactor central independiente. Su reactor conserva la animación de las dos fases.
- **Asteroides:** cuatro mallas con semillas reproducibles, cráteres hundidos, bordes elevados y cuatro tonos de basalto. Los fragmentos reutilizan estos modelos a distintas escalas.

Las mallas de cascos se agrupan por modelo, manteniendo distintos materiales para cada superficie. Los materiales URP Lit son compartidos. El cristal es opaco y brillante para asegurar una silueta legible en equipos modestos. No se añadieron texturas ni efectos costosos.

## Archivos editables

- `ArtSource/AuthoredV1/*.blend`: siete fuentes nativas editables con Blender.
- `Assets/Nebula/Art/Models/AuthoredV1/*.fbx`: siete modelos que importa Unity; Blender no es necesario para jugar.
- `Assets/Nebula/Art/Materials/AuthoredV1`: paleta URP, editable en Unity.
- `Tools/create_blender_models.py`: receta de geometría, materiales, exportación y conteos.
- `Assets/Nebula/Editor/OriginalArtWorkshop.cs`: importación, orientación, integración y vistas previas.

La herramienta trabaja sobre los prefabs automáticos conocidos y conserva sus GUID, Rigidbody, colliders, comportamiento y referencias físicas. La nave conserva la referencia a sus propulsores; el jefe recibe la referencia al nuevo reactor. Las escenas conservan sus referencias al objeto de presentación del menú. Un modelo personalizado que sustituya la estructura conocida detiene esa sustitución. Los materiales existentes se conservan al reinstalar el arte.

## Reproducir

Con Unity cerrado, desde la raíz del proyecto:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.1\blender.exe' --background --python Tools/create_blender_models.py
```

Esto escribe únicamente la carpeta dedicada AuthoredV1 y sus fuentes. Para integrarlo, abre Unity y selecciona **Tools → Nebula Defender → Install Original Art**. Para obtener nuevas imágenes sin sustituir prefabs, selecciona **Render Original Art Previews**. Guarda tus modelos personalizados en otra carpeta.

La integración y las vistas previas también pueden ejecutarse con el Editor en batch y gráficos habilitados:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\Users\tonyh\OneDrive\Documents\nebula 3D' -executeMethod NebulaDefender.Editor.OriginalArtWorkshop.Install -logFile 'Documentation/Evidence/original-art-import.log' --burst-disable-compilation
```

Los conteos que utiliza el juego son los de la importación Unity en `Evidence/authored-models-validation.txt`. Pueden diferir levemente del conteo de origen porque el importador elimina triángulos degenerados. Las imágenes `model-*.png` son renderizados reales de esas mallas en Unity; no ilustraciones conceptuales.

Referencia del exportador: [FBX en la API oficial de Blender](https://docs.blender.org/api/5.0/bpy.ops.export_scene.html).
