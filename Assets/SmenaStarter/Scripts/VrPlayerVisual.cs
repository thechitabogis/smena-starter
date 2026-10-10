using Mirror;
using UnityEngine;

namespace SmenaStarter
{
    /// <summary>
    /// Одна модель на игрока: ребёнок Model, внутри тело и кисти.
    /// Кисти видят все. Их положение везут NetworkTransform на этом же объекте.
    /// </summary>
    public class VrPlayerVisual : NetworkBehaviour
    {
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;
        public Camera localCamera;

        public override void OnStartLocalPlayer()
        {
            if (localCamera != null)
                localCamera.enabled = isLocalPlayer && Application.platform == RuntimePlatform.Android;
        }
    }
}
