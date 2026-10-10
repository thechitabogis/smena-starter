using Mirror;
using UnityEngine;

namespace SmenaStarter
{
    public class ColorSyncExample : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnColor))]
        public Color networkedColor = Color.white;

        public Renderer target;

        void Reset()
        {
            target = GetComponent<Renderer>();
        }

        public override void OnStartClient()
        {
            Apply(networkedColor);
        }

        [ContextMenu("Smena/Cycle Color (server)")]
        public void CycleFromHost()
        {
            if (!isServer)
                return;
            networkedColor = Color.HSVToRGB(Random.value, 0.7f, 0.95f);
        }

        void OnColor(Color _, Color next) => Apply(next);

        void Apply(Color c)
        {
            if (target == null)
                target = GetComponent<Renderer>();
            if (target != null)
                target.material.color = c;
        }
    }
}
