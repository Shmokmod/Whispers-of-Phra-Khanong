using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Reflection;

public class VolumeProfileValidator : EditorWindow
{
    [MenuItem("Tools/Validate Volume Profiles")]
    public static void ValidateProfiles()
    {
        string[] guids = AssetDatabase.FindAssets("t:VolumeProfile");

        if (guids.Length == 0)
        {
            Debug.LogWarning("❗ No Volume Profiles found in the project.");
            return;
        }

        Debug.Log("===== Volume Profile Validation Start =====");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);

            if (profile == null)
            {
                Debug.LogError($"❌ Profile at path {path} is NULL!");
                continue;
            }

            Debug.Log($"\n🔍 Checking Profile: {profile.name}");

            foreach (var comp in profile.components)
            {
                if (comp == null)
                {
                    Debug.LogError($"  ❌ Null component found in profile {profile.name}");
                    continue;
                }

                System.Type t = comp.GetType();
                Debug.Log($"  ✔ Component: {t.Name}");

                // ตรวจ field ด้านใน component
                var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    var value = field.GetValue(comp);

                    if (value == null)
                    {
                        Debug.LogError($"    ❌ Null field: {field.Name} in component {t.Name}");
                    }
                }
            }

            Debug.Log($"🔧 Finished checking {profile.name}");
        }

        Debug.Log("===== Volume Profile Validation Complete =====");
    }
}
