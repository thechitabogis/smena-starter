using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SmenaStarter
{
    /// <summary>Networked wrapper for furniture models, which remain visual-only prefabs.</summary>
    public class NetworkFurniture : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnModelIndexChanged))]
        int modelIndex = -1;

        FurnitureCatalog catalog;
        GameObject modelInstance;

        public void SetModelIndex(int index)
        {
            modelIndex = index;
            ApplyModel();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            ApplyModel();
        }

        void OnModelIndexChanged(int _, int value)
        {
            modelIndex = value;
            ApplyModel();
        }

        void ApplyModel()
        {
            if (modelIndex < 0)
                return;
            if (catalog == null)
                catalog = Resources.Load<FurnitureCatalog>("FurnitureCatalog");
            if (catalog == null || modelIndex >= catalog.items.Count)
                return;

            var entry = catalog.items[modelIndex];
            if (entry == null || entry.prefab == null)
                return;

            if (modelInstance != null)
                Destroy(modelInstance);
            modelInstance = Instantiate(entry.prefab, transform);
            modelInstance.name = entry.displayName;

            var bounds = new Bounds(transform.position, Vector3.zero);
            var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
            var hasBounds = false;
            foreach (var renderer in renderers)
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                    bounds.Encapsulate(renderer.bounds);
            }

            var collider = GetComponent<BoxCollider>();
            if (collider == null)
                collider = gameObject.AddComponent<BoxCollider>();
            if (hasBounds)
            {
                collider.center = transform.InverseTransformPoint(bounds.center);
                var size = transform.InverseTransformVector(bounds.size);
                collider.size = new Vector3(Mathf.Max(0.08f, Mathf.Abs(size.x)),
                    Mathf.Max(0.08f, Mathf.Abs(size.y)), Mathf.Max(0.08f, Mathf.Abs(size.z)));
            }
            collider.enabled = true;
        }

        public static GameObject CreateNetworkObject()
        {
            var root = new GameObject("NetworkFurniture");
            root.AddComponent<NetworkIdentity>();
            var body = root.AddComponent<Rigidbody>();
            body.mass = 20f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            root.AddComponent<BoxCollider>();
            var networkTransform = root.AddComponent<NetworkTransformUnreliable>();
            networkTransform.syncDirection = SyncDirection.ClientToServer;
            root.AddComponent<XRGrabInteractable>();
            root.AddComponent<GrabNetworkAuthority>();
            root.AddComponent<NetworkFurniture>();
            return root;
        }
    }
}
