using Mirror;
using UnityEngine;

namespace SmenaStarter
{
    /// <summary>Registers the ready Chair01 prefab before Mirror starts the client.</summary>
    public static class FurnitureNetworkBootstrap
    {
        // Stable custom spawn ID. The same handler runs on each connected build.
        public const uint AssetId = 0x534D4601;

        public static void RegisterSpawnHandler()
        {
            // Mirror clears custom handlers on disconnect, so register at each client start.
            NetworkClient.RegisterSpawnHandler(AssetId, Spawn, Unspawn);
        }

        static GameObject Spawn(Vector3 position, uint _)
        {
            var catalog = Resources.Load<FurnitureCatalog>("FurnitureCatalog");
            if (catalog == null || catalog.items == null || catalog.items.Count == 0 ||
                catalog.items[0] == null || catalog.items[0].prefab == null)
            {
                Debug.LogError("Smena: сетевой префаб Chair01 отсутствует в каталоге.");
                return null;
            }

            var item = Object.Instantiate(catalog.items[0].prefab, position, Quaternion.identity);
            item.name = "Chair01";
            return item;
        }

        static void Unspawn(GameObject item)
        {
            Object.Destroy(item);
        }
    }
}
