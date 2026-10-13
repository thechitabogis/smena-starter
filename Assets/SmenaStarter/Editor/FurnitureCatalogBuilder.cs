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
    const string SourcePrefabPath = "Assets/Furniture Mega Pack/Prefabs/Chairs/Chair01.prefab";
    const string NetworkPrefabPath = "Assets/SmenaStarter/Prefabs/NetworkChair01.prefab";

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

        var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
        if (source == null)
        {
            Debug.LogError("Smena: не найден " + SourcePrefabPath);
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkPrefabPath);
        if (prefab == null)
            prefab = BuildNetworkPrefab();
        if (prefab == null)
            return;

        var entries = new List<FurnitureCatalog.Entry>
        {
            new FurnitureCatalog.Entry { displayName = "Chair01", prefab = prefab }
        };

        catalog.items = entries;
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log("Smena: каталог мебели содержит только готовый сетевой Chair01.");
    }

    static GameObject BuildNetworkPrefab()
    {
        var contents = PrefabUtility.LoadPrefabContents(SourcePrefabPath);
        try
        {
            var renderers = contents.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogError("Smena: в Chair01 нет Renderer.");
                return null;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            var body = contents.GetComponent<Rigidbody>();
            if (body == null)
                body = contents.AddComponent<Rigidbody>();
            body.mass = 8f;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            var collider = contents.GetComponent<BoxCollider>();
            if (collider == null)
                collider = contents.AddComponent<BoxCollider>();
            collider.center = contents.transform.InverseTransformPoint(bounds.center);
            var size = contents.transform.InverseTransformVector(bounds.size);
            collider.size = new Vector3(Mathf.Max(0.08f, Mathf.Abs(size.x)),
                Mathf.Max(0.08f, Mathf.Abs(size.y)), Mathf.Max(0.08f, Mathf.Abs(size.z)));

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

            var result = PrefabUtility.SaveAsPrefabAsset(contents, NetworkPrefabPath);
            Debug.Log("Smena: создан сетевой префаб " + NetworkPrefabPath);
            return result;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
#endif
