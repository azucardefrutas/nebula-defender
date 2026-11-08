"""Original Nebula art. Run with Blender --background --python this_file.py.

Only the dedicated AuthoredV1 folder is written. No paid services or downloads.
Blender source stays outside Assets; FBX models work without Blender at runtime.
"""
from pathlib import Path
import json
import math
import random
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/Nebula/Art/Models/AuthoredV1"
SOURCE = ROOT / "ArtSource/AuthoredV1"
OUT.mkdir(parents=True, exist_ok=True)
SOURCE.mkdir(parents=True, exist_ok=True)
REPORT = []


def material(name, color, metallic=0.0, roughness=.4, emission=0.0):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Emission Color"].default_value = (*color, 1)
    bsdf.inputs["Emission Strength"].default_value = emission
    return mat


def reset():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def finish(obj, name, mat, bevel=0):
    obj.name = name
    obj.data.materials.append(mat)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        modifier = obj.modifiers.new("Machined edges", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    return obj


def box(name, center, scale, mat, bevel=.035, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=center, rotation=rotation)
    obj = bpy.context.object
    obj.scale = scale
    return finish(obj, name, mat, bevel)


def loft(name, sections, mat, sides=8):
    # Cross-sections along Blender +Y (the ship's nose); faceted elliptical hull.
    vertices = []
    for y, width, height, z in sections:
        for i in range(sides):
            angle = 2 * math.pi * i / sides
            vertices.append((math.cos(angle) * width, y, z + math.sin(angle) * height))
    faces = [tuple(reversed(range(sides)))]
    for s in range(len(sections) - 1):
        for i in range(sides):
            a = s * sides + i
            b = s * sides + (i + 1) % sides
            faces.append((a, b, b + sides, a + sides))
    faces.append(tuple((len(sections) - 1) * sides + i for i in range(sides)))
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    # Recalculate outward normals independently of contour winding.
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(obj, name, mat)


def plate(name, contour, z, thickness, mat):
    n = len(contour)
    verts = [(x, y, z - thickness / 2) for x, y in contour]
    verts += [(x, y, z + thickness / 2) for x, y in contour]
    faces = [tuple(reversed(range(n))), tuple(range(n, 2 * n))]
    faces += [(i, (i + 1) % n, (i + 1) % n + n, i + n) for i in range(n)]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    return finish(obj, name, mat, .018)


def ring(name, center, radius, tube, mat, rotation=(math.pi / 2, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(major_segments=20, minor_segments=6,
        location=center, rotation=rotation, major_radius=radius, minor_radius=tube)
    return finish(bpy.context.object, name, mat)


def sphere(name, center, scale, mat, subdivisions=2):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdivisions, radius=1, location=center)
    obj = bpy.context.object
    obj.scale = scale
    return finish(obj, name, mat)


def marker(name, position):
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    obj.location = position


def export(name, core=None):
    # One hull renderer with material slots; the boss core remains independently animated.
    parts = [o for o in bpy.context.scene.objects if o.type == "MESH" and o != core]
    bpy.ops.object.select_all(action="DESELECT")
    for obj in parts:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    hull = bpy.context.object
    hull.name = "ND_HULL"
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    # Remove duplicate material slots after joining, then triangulate for portable topology.
    bpy.ops.object.material_slot_remove_unused()
    modifier = hull.modifiers.new("Game triangles", "TRIANGULATE")
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    if core:
        core.name = "ND_CORE"
    marker("ND_FORWARD", (0, 2, 0))
    marker("ND_UP", (0, 0, 2))
    path = OUT / (name + ".fbx")
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=False, object_types={"MESH", "EMPTY"},
        axis_forward="-Z", axis_up="Y", bake_anim=False, add_leaf_bones=False,
        use_triangles=True, mesh_smooth_type="FACE")
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE / (name + ".blend")))
    meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    REPORT.append({"model": name, "triangles": sum(sum(len(p.vertices) - 2 for p in o.data.polygons) for o in meshes),
                   "renderers": len(meshes), "materials": len(hull.data.materials), "fbx_bytes": path.stat().st_size})


pearl = material("ND_Pearl", (.72, .83, .91), .65, .3)
cobalt = material("ND_Cobalt", (.018, .11, .3), .65, .32)
carbon = material("ND_Carbon", (.028, .042, .06), .6, .38)
steel = material("ND_Steel", (.26, .33, .39), .8, .27)
glass = material("ND_Glass", (.018, .25, .34), .7, .16)
cyan = material("ND_Ion", (.015, .7, 1), .3, .25, 3)
orange = material("ND_Amber", (1, .12, .008), .3, .35, 2)
armor = material("ND_Hostile", (.4, .055, .018), .65, .4)


def aurora():
    reset()
    loft("Aurora titanium spine", [(-1.28,.3,.2,0),(-.7,.45,.25,0),(.15,.38,.23,.02),(.95,.22,.14,.02),(1.65,.025,.035,0)], pearl)
    loft("Underside keel", [(-1.1,.22,.12,-.13),(.15,.28,.18,-.15),(1.25,.025,.035,-.06)], carbon)
    loft("Raised cobalt canopy frame", [(-.48,.28,.15,.27),(.02,.3,.3,.29),(.62,.18,.16,.25),(.85,.03,.03,.19)], cobalt)
    loft("Faceted glass canopy", [(-.32,.22,.13,.38),(.08,.24,.21,.39),(.57,.13,.09,.35),(.7,.03,.02,.27)], glass)
    for side in [-1, 1]:
        shape = [(side*x,y) for x,y in [(.25,.55),(1.76,-.65),(1.7,-1.1),(.5,-1.0),(.25,-.7)]]
        plate("Swept armored wing", shape, -.06, .14, cobalt)
        plate("Titanium wing inset", [(side*x,y) for x,y in [(.52,.12),(1.48,-.64),(1.12,-.73),(.45,-.43)]], .035, .045, pearl)
        plate("Wing ion guide", [(side*x,y) for x,y in [(1.1,-.45),(1.6,-.75),(1.61,-.81),(1.06,-.5)]], .08, .025, cyan)
        loft("Engine nacelle", [(-1.45,.2,.2,.0),(-.95,.28,.25,0),(-.3,.24,.2,.01),(-.03,.12,.1,0)], steel).location.x = side*.77
        ring("Engine nozzle armor", (side*.77,-1.45,0), .195, .055, carbon)
        ring("Ion chamber", (side*.77,-1.49,0), .135, .025, cyan)
        sphere("Engine core", (side*.77,-1.47,0), (.12,.025,.12), cyan)
        box("Twin tail fin", (side*.65,-.93,.48), (.075,.5,.62), cobalt, .025, (0,side*.35,0))
        box("Fin edge", (side*.76,-.93,.77), (.025,.38,.035), cyan, .01)
        box("Wingtip railgun", (side*1.38,-.27,-.02), (.14,1.1,.16), carbon, .03)
        ring("Cannon collar", (side*1.38,.3,-.02), .075, .02, steel)
        box("Cannon emitter", (side*1.38,.33,-.02), (.065,.04,.065), cyan, .008)
        # Real recessed vents emphasize the thickness of the midbody.
        for i in range(4):
            box("Cooling vent", (side*.37,-.55-i*.12,.23), (.13,.045,.035), carbon, .008)
    box("Dorsal navigation strip", (0,-.9,.26), (.09,.4,.025), cyan, .012)
    export("Aurora")


def drone():
    reset()
    loft("Predator head", [(-.8,.32,.2,0),(-.2,.44,.29,.02),(.38,.32,.2,0),(.86,.07,.08,0)], carbon)
    loft("Armored crown", [(-.55,.26,.13,.22),(-.05,.32,.22,.23),(.45,.04,.04,.2)], armor)
    for side in [-1, 1]:
        plate("Forward hooked wing", [(side*x,y) for x,y in [(.26,-.5),(1.15,-.65),(1.0,.65),(.73,.86),(.78,.05),(.3,.25)]], -.02, .16, carbon)
        plate("Orange warning armor", [(side*x,y) for x,y in [(.54,-.4),(.97,-.48),(.84,.36),(.72,.04)]], .08, .045, armor)
        box("Energy blade", (side*.86,.32,.12), (.045,.56,.035), orange, .008)
        ring("Thruster", (side*.63,-.66,.01), .15, .035, steel)
        sphere("Thruster plasma", (side*.63,-.69,.01), (.1,.04,.1), orange)
        box("Paired blaster", (side*.8,.58,-.03), (.12,.5,.12), steel)
    sphere("Targeting optic", (0,.48,.17), (.22,.11,.1), orange)
    ring("Optic armor", (0,.43,.17), .23, .045, steel)
    export("Marauder")


def nemesis():
    reset()
    loft("Fortress keel", [(-2.1,.65,.5,0),(-1.2,1.2,.6,0),(.3,1.3,.48,0),(2.0,.6,.3,0),(2.7,.12,.12,0)], carbon)
    for side in [-1,1]:
        plate("Dreadnought hammer wing", [(side*x,y) for x,y in [(.6,1.4),(3.8,.8),(4.8,-.1),(4.6,-1.2),(1,-1.8)]], -.1, .62, carbon)
        plate("Upper segmented armor", [(side*x,y) for x,y in [(1,1.0),(3.6,.5),(4.2,-.15),(3.5,-.65),(1.2,-1.25)]], .3, .18, steel)
        for i in range(3):
            box("Armored turret", (side*(1.6+i),-.3,.65), (.65,.8,.45), carbon, .1)
            box("Turret barrel", (side*(1.6+i),.45,.67), (.16,.95,.18), steel)
            box("Cannon energy", (side*(1.6+i),.94,.67), (.105,.04,.1), orange, .015)
        for x in [1.6,2.7,3.8]:
            ring("Drive housing", (side*x,-1.1,0), .28, .09, carbon)
            ring("Plasma drive", (side*x,-1.2,0), .2, .045, orange)
        plate("Threat stripe", [(side*x,y) for x,y in [(1.1,.91),(3.7,.37),(3.8,.26),(1.1,.78)]], .42, .035, orange)
        box("Command tower", (side*.7,-1,.78), (.38,.85,.9), carbon, .08)
        box("Bridge windows", (side*.7,-.58,.93), (.25,.03,.17), orange, .018)
    ring("Reactor collar", (0,.1,.52), .82, .17, steel, (0,0,0))
    core = sphere("Live reactor", (0,.1,.63), (.61,.61,.25), orange, 3)
    for i in range(8):
        a = i*math.pi/4
        box("Reactor containment strut", (.8*math.cos(a),.1+.8*math.sin(a),.68), (.12,.3,.16), carbon, .025, (0,0,a-math.pi/2))
    # Boss faces the pilot at -Z in Unity; importer rotates this entire model 180 degrees.
    export("Nemesis", core)


def asteroid(seed):
    reset()
    rng = random.Random(seed)
    mats = [material("ND_Basalt"+str(i), c, .08, .9) for i,c in enumerate([
        (.22,.2,.19),(.32,.29,.25),(.15,.16,.18),(.4,.36,.29)])]
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=4, radius=1)
    obj = bpy.context.object
    obj.name = "Cratered rock"
    craters = [(Vector((rng.uniform(-1,1),rng.uniform(-1,1),rng.uniform(-1,1))).normalized(),rng.uniform(.16,.37)) for _ in range(6)]
    for vertex in obj.data.vertices:
        d = vertex.co.normalized()
        radius = .88 + .06*math.sin(d.x*9+d.y*3) + .04*math.sin(d.z*13+d.y*7)
        for axis,size in craters:
            distance = (d-axis).length
            radius -= .17 * math.exp(-(distance/(size*.65))**4)
            radius += .05 * math.exp(-((distance-size)/.055)**2)
        vertex.co = Vector((d.x*1.03,d.y*.93,d.z*.89))*radius
    for mat in mats:
        obj.data.materials.append(mat)
    for polygon in obj.data.polygons:
        polygon.material_index = rng.choices(range(4), [6,2,2,1])[0]
    export("Asteroid"+str(seed-170))


aurora()
drone()
nemesis()
for seed in range(170,174):
    asteroid(seed)
(ROOT / "Documentation/Evidence/authored-models-source.json").write_text(
    json.dumps({"tool": bpy.app.version_string, "origin": "Original geometry authored for Nebula Defender", "assets": REPORT}, indent=2)+"\n", encoding="utf-8")
print("NEBULA_ORIGINAL_MODELS_READY", json.dumps(REPORT))
