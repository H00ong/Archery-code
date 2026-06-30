#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaveSystem.EditorTools
{
    public static class SaveSystemEditorMenu
    {
        private const string FileName = "save.json";
        private const string BackupFileName = "save.json.bak";

        [MenuItem("Tools/SaveSystem/Open Save Folder")]
        public static void OpenSaveFolder()
        {
            string path = Application.persistentDataPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }

        [MenuItem("Tools/SaveSystem/Reset Save")]
        public static void ResetSave()
        {
            string savePath = Path.Combine(Application.persistentDataPath, FileName);
            string backupPath = Path.Combine(Application.persistentDataPath, BackupFileName);

            if (!EditorUtility.DisplayDialog(
                    "Reset Save",
                    $"세이브 파일을 삭제합니다.\n\n{savePath}",
                    "삭제", "취소"))
                return;

            try
            {
                if (File.Exists(savePath)) File.Delete(savePath);
                if (File.Exists(backupPath)) File.Delete(backupPath);
                if (Application.isPlaying && SaveManager.Instance != null)
                    SaveManager.Instance.ResetSave();
                UnityEngine.Debug.Log("[SaveSystemEditorMenu] Save deleted.");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[SaveSystemEditorMenu] Reset failed: {ex.Message}");
            }
        }

        [MenuItem("Tools/SaveSystem/Force Save Now")]
        public static void ForceSave()
        {
            if (!Application.isPlaying || SaveManager.Instance == null)
            {
                UnityEngine.Debug.LogWarning("[SaveSystemEditorMenu] Play mode 필요.");
                return;
            }
            SaveManager.Instance.Save();
        }
    }
}
#endif
