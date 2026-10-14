#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using SmenaStarter;
using UnityEditor;
using UnityEngine;
using Mirror;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[InitializeOnLoad]
static class FurnitureCatalogBuilder
{
    const string CatalogPath = "Assets/SmenaStarter/Resources/FurnitureCatalog.asset";
    static readonly (string displayName, string sourcePath)[] Furniture =
    {
        ("Chair01", "Assets/Furniture Mega Pack/Prefabs/Chairs/Chair01.prefab"),
        ("Toilet01", "Assets/Furniture Mega Pack/Prefabs/Bathroom/Toilet01.prefab"),
        ("Bed01", "Assets/Furniture Mega Pack/Prefabs/Beds/Bed01.prefab"),
        ("Closet01", "Assets/Furniture Mega Pack/Prefabs/Closets/Closet01.prefab"),
        ("Cushion01", "Assets/Furniture Mega Pack/Prefabs/Cushioins/Cushion01.prefab"),
        ("Drawer01", "Assets/Furniture Mega Pack/Prefabs/Drawers/Drawer01.prefab"),
        ("Sofa01", "Assets/Furniture Mega Pack/Prefabs/Sofas/Sofa01.prefab"),
        ("Table01", "Assets/Furniture Mega Pack/Prefabs/Tables/Table01.prefab")
    };

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

        var entries = new List<FurnitureCatalog.Entry>(Furniture.Length);
        foreach (var item in Furniture)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(item.sourcePath);
            if (source == null)
            {
                Debug.LogError("Smena: не найден " + item.sourcePath);
                continue;
            }

            GameObject prefab;
            if (item.displayName == "Chair01")
            {
                const string networkChairPath = "Assets/SmenaStarter/Prefabs/NetworkChair01.prefab";
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(networkChairPath);
                if (prefab == null)
                    prefab = BuildNetworkPrefab(item.sourcePath, networkChairPath, item.displayName);
            }
            else
            {
                // These seven source prefabs already have physics, XR grab and Mirror components.
                prefab = source;
                if (!IsNetworkReady(prefab))
                {
                    Debug.LogError("Smena: в префабе " + item.displayName + " не хватает сетевых компонентов.");
                    continue;
                }
            }

            if (prefab == null)
                continue;

            entries.Add(new FurnitureCatalog.Entry { displayName = item.displayName, prefab = prefab });
        }

        catalog.items = entries;
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log("Smena: каталог мебели содержит " + entries.Count + " сетевых предметов.");
    }

    static bool IsNetworkReady(GameObject prefab)
    {
        return prefab.GetComponent<NetworkIdentity>() != null
            && prefab.GetComponent<NetworkTransformUnreliable>() != null
            && prefab.GetComponent<GrabNetworkAuthority>() != null
            && prefab.GetComponent<XRGrabInteractable>() != null
            && prefab.GetComponent<Rigidbody>() != null
            && prefab.GetComponent<Collider>() != null;
    }

    static GameObject BuildNetworkPrefab(string sourcePath, string networkPrefabPath, string displayName)
    {
        var contents = PrefabUtility.LoadPrefabContents(sourcePath);
        try
        {
            var renderers = contents.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogError("Smena: в " + displayName + " нет Renderer.");
                return null;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            var body = contents.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = contents.AddComponent<Rigidbody>();
                body.mass = 8f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
            }

            var collider = contents.GetComponent<Collider>();
            if (collider == null)
            {
                var boxCollider = contents.AddComponent<BoxCollider>();
                boxCollider.center = contents.transform.InverseTransformPoint(bounds.center);
                var size = contents.transform.InverseTransformVector(bounds.size);
                boxCollider.size = new Vector3(Mathf.Max(0.08f, Mathf.Abs(size.x)),
                    Mathf.Max(0.08f, Mathf.Abs(size.y)), Mathf.Max(0.08f, Mathf.Abs(size.z)));
            }

            if (contents.GetComponent<XRGrabInteractable>() == null)
                contents.AddComponent<XRGrabInteractable>();
            if (contents.GetComponent<GrabNetworkAuthority>() == null)
                contents.AddComponent<GrabNetworkAuthority>();
            var networkTransform = contents.GetComponent<NetworkTransformUnreliable>();
            if (networkTransform == null)
                networkTransform = contents.AddComponent<NetworkTransformUnreliable>();
            networkTransform.syncDirection = SyncDirection.ClientToServer;
            if (contents.GetComponent<NetworkIdentity>() == null)
                contents.AddComponent<NetworkIdentity>();

            var result = PrefabUtility.SaveAsPrefabAsset(contents, networkPrefabPath);
            Debug.Log("Smena: создан сетевой префаб " + networkPrefabPath);
            return result;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
#endif
