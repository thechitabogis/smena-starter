using Mirror;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SmenaStarter
{
    /// <summary>
    /// Правый стик — поворот. Перемещение выполняется физически.
    /// Модель ставит под голову, кисти копирует с контроллеров.
    /// </summary>
    public class VrPlayerLocomotion : NetworkBehaviour
    {
        public float turnSpeed = 90f;

        InputAction turn;
        XROrigin origin;
        Transform leftController;
        Transform rightController;

        public override void OnStartLocalPlayer()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                enabled = false;
                return;
            }

            var visual = GetComponent<VrPlayerVisual>();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (UnderHand(renderer.transform, visual))
                    continue;
                renderer.enabled = false;
            }

            var actions = Resources.Load<InputActionAsset>("XRI Default Input Actions");
            if (actions == null)
                return;
            turn = actions.FindAction("XRI Right Locomotion/Turn", false);
        }

        void Update()
        {
            if (!isLocalPlayer)
                return;
            if (origin == null)
                origin = FindAnyObjectByType<XROrigin>();
            if (origin == null)
                return;

            if (SceneManager.GetActiveScene().name == SmenaNetworkManager.RoomScene)
            {
                RoomFrame.Apply(SceneManager.GetActiveScene());
                if (transform.parent != RoomFrame.Anchor)
                    return;
            }

            var rig = origin.transform;
            var head = origin.Camera != null ? origin.Camera.transform : rig;

            if (turn != null)
            {
                var x = turn.ReadValue<Vector2>().x;
                if (Mathf.Abs(x) > 0.01f)
                    rig.RotateAround(head.position, Vector3.up, x * turnSpeed * Time.deltaTime);
            }


            var body = head.position;
            body.y = 1f;
            transform.SetPositionAndRotation(body, Quaternion.Euler(0f, head.eulerAngles.y, 0f));
        }

        void LateUpdate()
        {
            if (!isLocalPlayer)
                return;

            var visual = GetComponent<VrPlayerVisual>();
            if (visual == null)
                return;

            if (leftController == null)
                leftController = FindController("LeftHand Controller");
            if (rightController == null)
                rightController = FindController("RightHand Controller");
            CopyHand(visual.leftHand, leftController);
            CopyHand(visual.rightHand, rightController);
        }

        static void CopyHand(Transform hand, Transform controller)
        {
            if (hand == null || controller == null)
                return;
            hand.SetPositionAndRotation(controller.position, controller.rotation);
        }

        static Transform FindController(string objectName)
        {
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (t.name == objectName)
                    return t;
            }
            return null;
        }

        static bool UnderHand(Transform t, VrPlayerVisual visual)
        {
            if (visual == null)
                return false;
            return Under(t, visual.leftHand) || Under(t, visual.rightHand);
        }

        static bool Under(Transform t, Transform root)
        {
            return root != null && (t == root || t.IsChildOf(root));
        }

        [Command]
        public void CmdCalibrationReady(float measuredMeters)
        {
            if (connectionToClient != null)
                RoomCalibration.MarkReady(connectionToClient.connectionId, measuredMeters);
        }

    }
}
