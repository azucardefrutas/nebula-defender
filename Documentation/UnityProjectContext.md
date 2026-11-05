# Contexto del proyecto — 2026-11-04

- Directorio inspeccionado: `C:/Users/tonyh/OneDrive/Documents/nebula 3D`. Estaba vacío y no era un repositorio Git. No había modelos, scripts, escenas ni AGENTS.md locales. Se inicializó Git sin remoto.
- Unity confirmado: 6000.3.15f1 (6.3 LTS), con soporte Windows standalone Mono x64. También hay otros Editores instalados; no se utilizan.
- URP 17.3.0, Input System 1.19.0, uGUI 2.0.0 (incluye TMP), Test Framework 1.6.0, comprobados con el catálogo del Editor y resueltos en packages-lock.json.
- Arquitectura: componentes con composición explícita, datos ScriptableObject y reglas C# separadas. Física Rigidbody 3D en XZ. Sin red, DOTS propio ni servicios comerciales.
- `Assets/Nebula/Generated/Scenes/CoreSandbox.unity`: primera escena de autoría, inicio directo del vuelo. `Campaign.unity`: entrada de distribución con menú/perfiles.
- Código bajo `Assets/Nebula/Scripts`, Editor separado, pruebas EditMode y PlayMode. Recursos externos a Nebula solo: ajustes oficiales URP y esenciales TMP.
- No hay un proveedor Unity MCP conectado. Se utiliza el ejecutable real del Editor, registros, Unity Test Framework y, para inspección visual, control de aplicaciones de Windows.
- La primera invocación falló por licencia no activa. El usuario intervino en Unity Hub; una ejecución posterior importó paquetes y generó la escena.
- Estado de verificación y fallos: consultar TEST_REPORT.md y Evidence/. No atribuir jugabilidad a la revisión de compilación independiente.

Restricciones: no modificar Dead Tap ni otros proyectos, no publicar servicios, no instalar programas innecesarios, no inventar pruebas. Las preferencias de alcance y arquitectura provienen del prompt actual del usuario.
