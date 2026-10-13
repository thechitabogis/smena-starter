using Mirror;
using UnityEngine;

namespace SmenaStarter
{
    /// <summary>Registers the shared furniture wrapper before Mirror starts the client.</summary>
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
            var item = NetworkFurniture.CreateNetworkObject();
            item.transform.position = position;
            return item;
        }

        static void Unspawn(GameObject item)
        {
            Object.Destroy(item);
        }
    }
}
