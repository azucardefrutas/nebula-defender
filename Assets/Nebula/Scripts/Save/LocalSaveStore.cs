using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace NebulaDefender
{
    /// <summary>Single writer, versioned JSON, flushed temporary file, atomic replacement and previous valid backup.</summary>
    public sealed class LocalSaveStore
    {
        public string Path { get; }
        public bool ReadOnly { get; private set; }
        public string Notice { get; private set; }
        private bool preserveDamagedPrimary;
        public LocalSaveStore(string directory) { Path = System.IO.Path.Combine(directory, "pilots-v1.json"); }
        public SaveDatabase Load()
        {
            Notice = "";
            if (!File.Exists(Path) && !File.Exists(Path + ".bak")) return SaveDatabase.Empty();
            if (TryRead(Path, out var data, out bool future)) return data;
            if (future)
            {
                ReadOnly = true; Notice = "Este guardado pertenece a una versión posterior. Se conservará sin cambios.";
                return SaveDatabase.Empty();
            }
            if (TryRead(Path + ".bak", out data, out _))
            {
                preserveDamagedPrimary = File.Exists(Path);
                Notice = "Se recuperó la copia de respaldo del progreso."; return data;
            }
            ReadOnly = true;
            Notice = "No se pudo recuperar el guardado. Los archivos originales se conservaron; esta sesión no guardará progreso.";
            return SaveDatabase.Empty();
        }
        private static bool TryRead(string path, out SaveDatabase data, out bool future)
        {
            data = null; future = false;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 4 * 1024 * 1024) return false;
                data = JsonUtility.FromJson<SaveDatabase>(File.ReadAllText(path, Encoding.UTF8));
                future = data != null && data.format == "nebula-defender" && data.schemaVersion > 1;
                SaveValidation.Validate(data); return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is FormatException)
            { data = null; return false; }
        }
        public void Save(SaveDatabase database)
        {
            if (ReadOnly) throw new IOException(Notice);
            SaveValidation.Validate(database);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            string temporary = Path + ".tmp";
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(database, true));
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (preserveDamagedPrimary)
            {
                File.Copy(Path, Path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"), false);
                // Keep the valid recovery backup intact. Replace creates a separate backup of the damaged primary.
                File.Replace(temporary, Path, Path + ".replaced", true); preserveDamagedPrimary = false;
            }
            else if (File.Exists(Path)) File.Replace(temporary, Path, Path + ".bak", true);
            else File.Move(temporary, Path);
        }
    }
}
