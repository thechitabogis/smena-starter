#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using SmenaStarter;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class FurnitureCatalogBuilder
{
    const string CatalogPath = "Assets/SmenaStarter/Resources/FurnitureCatalog.asset";
    const string PrefabDirectory = "Assets/Furniture Mega Pack/Prefabs";

    static FurnitureCatalogBuilder()
    {
        EditorApplication.delayCall += EnsureCatalog;
    }

    [MenuItem("Smena/Refresh Furniture Catalog")]
    static void EnsureCatalog()
    {
        if (!AssetDatabase.IsValidFolder("Assets/SmenaStarter/Resources"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/SmenaStarter"))
                return;
            AssetDatabase.CreateFolder("Assets/SmenaStarter", "Resources");
        }

        var catalog = AssetDatabase.LoadAssetAtPath<FurnitureCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<FurnitureCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        var entries = new List<FurnitureCatalog.Entry>();
        if (AssetDatabase.IsValidFolder(PrefabDirectory))
        {
            var paths = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabDirectory })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
            foreach (var path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || prefab.GetComponentsInChildren<Renderer>(true).Length == 0)
                    continue;
                entries.Add(new FurnitureCatalog.Entry { displayName = prefab.name, prefab = prefab });
            }
        }

        catalog.items = entries;
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log("Smena: каталог мебели содержит " + entries.Count + " префабов.");
    }
}
#endif
