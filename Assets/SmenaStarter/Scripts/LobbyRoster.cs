using Mirror;
using UnityEngine;

namespace SmenaStarter
{
    public class LobbyRoster : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnCountChanged))]
        public int playerCount;

        public static LobbyRoster Instance { get; private set; }

        public event System.Action<int> CountChanged;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (!isServer)
                return;
            var next = NetworkServer.connections.Count;
            if (next != playerCount)
                playerCount = next;
        }

        void OnCountChanged(int _, int value)
        {
            CountChanged?.Invoke(value);
        }
    }
}
