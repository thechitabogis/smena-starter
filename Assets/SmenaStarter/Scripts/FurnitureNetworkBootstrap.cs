using Mirror;
using UnityEngine;

namespace SmenaStarter
{
    /// <summary>Registers one custom network spawn handler for every furniture prefab.</summary>
    public static class FurnitureNetworkBootstrap
    {
        // Chair01 keeps its existing ID; each additional catalog entry gets its own ID.
        const uint FirstAssetId = 0x534D4601;

        public static uint GetAssetId(int catalogIndex)
        {
            return FirstAssetId + (uint)catalogIndex;
        }

        public static void RegisterSpawnHandler()
        {
            // Mirror clears custom handlers on disconnect, so register at each client start.
            var catalog = Resources.Load<FurnitureCatalog>("FurnitureCatalog");
            if (catalog == null || catalog.items == null)
            {
                Debug.LogError("Smena: не найден Resources/FurnitureCatalog.asset.");
                return;
            }

            for (var i = 0; i < catalog.items.Count; i++)
            {
                var index = i;
                NetworkClient.RegisterSpawnHandler(GetAssetId(index),
                    (position, _) => Spawn(index, position), Unspawn);
            }
        }

        static GameObject Spawn(int index, Vector3 position)
        {
            var catalog = Resources.Load<FurnitureCatalog>("FurnitureCatalog");
            if (catalog == null || catalog.items == null || index < 0 || index >= catalog.items.Count ||
                catalog.items[index] == null || catalog.items[index].prefab == null)
            {
                Debug.LogError("Smena: сетевой префаб с индексом " + index + " отсутствует в каталоге.");
                return null;
            }

            var entry = catalog.items[index];
            var item = Object.Instantiate(entry.prefab, position, Quaternion.identity);
            item.name = entry.displayName;
            return item;
        }

        static void Unspawn(GameObject item)
        {
            Object.Destroy(item);
        }
    }
}
