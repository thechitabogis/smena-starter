using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SmenaStarter
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class GrabNetworkAuthority : NetworkBehaviour
    {
        XRGrabInteractable grab;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            if (grab == null)
                return;
            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }

        void OnDestroy()
        {
            if (grab == null)
                return;
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
        }

        void OnGrab(SelectEnterEventArgs _)
        {
            if (!isOwned)
                CmdTake();
        }

        void OnRelease(SelectExitEventArgs _)
        {
        }

        [Command(requiresAuthority = false)]
        void CmdTake(NetworkConnectionToClient sender = null)
        {
            if (sender == null)
                return;
            if (netIdentity.connectionToClient != null)
                netIdentity.RemoveClientAuthority();
            netIdentity.AssignClientAuthority(sender);
        }
    }
}
