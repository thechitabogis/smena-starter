using Mirror;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SmenaStarter
{
    /// <summary>
    /// На шлеме, в лобби. Триггер снимает кончик контроллера с наклеек A и B.
    /// Якорь считается локально. На сервер уходит только длина отрезка.
    /// </summary>
    public class FloorCalibrator : MonoBehaviour
    {
        const float MinSeparation = 0.15f;
        const float FloorReach = 0.15f;

        static int step;
        static bool sent;
        static string fault = "";
        static Vector3 first;
        static float measured;
        static GameObject markA;
        static GameObject markB;
        static InputAction triggerRight;
        static InputAction triggerLeft;

        public static string ClientStatus { get; private set; } = "";

        void Awake()
        {
            if (Application.platform != RuntimePlatform.Android)
                enabled = false;
        }

        void Update()
        {
            if (SceneManager.GetActiveScene().name != SmenaNetworkManager.LobbyScene)
            {
                ClientStatus = "";
                return;
            }

            var host = RoomCalibration.FindHost();
            if (host == null || !host.HasPoints)
            {
                ClientStatus = "Жди, пока хост запишет длину базиса.";
                return;
            }

            if (RoomFrame.Active && !sent && TrySendReady())
                sent = true;

            if (sent)
            {
                ClientStatus = "Калибровка готова. Жди комнату.";
                return;
            }

            var length = RoomCalibration.HostLength();
            var meters = length.ToString("0.000");
            if (step == 0)
                ClientStatus = fault + "Базис " + meters + " м. Триггером коснись наклейки A.";
            else
                ClientStatus = fault + "Триггером коснись наклейки B. От A ровно " + meters + " м.";

            if (!TriggerPressed(out var hand))
                return;

            if (!TryTipPoint(hand, out var point))
                return;

            if (step == 0)
            {
                first = point;
                step = 1;
                fault = "";
                markA = Place(markA, point, new Color(0.2f, 0.55f, 1f));
                return;
            }

            var localDelta = point - first;
            localDelta.y = 0f;
            var localLen = localDelta.magnitude;
            if (localLen < MinSeparation)
            {
                fault = "Точки слишком близко. ";
                return;
            }

            if (Mathf.Abs(localLen - length) > RoomCalibration.LengthTolerance)
            {
                fault = "Длина не сошлась с базисом. Допуск 5 см. ";
                step = 0;
                DestroyMark(ref markA);
                DestroyMark(ref markB);
                return;
            }

            markB = Place(markB, point, new Color(0.95f, 0.75f, 0.1f));
            var serverDelta = host.PointB - host.PointA;
            serverDelta.y = 0f;
            var yaw = Vector3.SignedAngle(serverDelta, localDelta, Vector3.up);
            var rot = Quaternion.Euler(0f, yaw, 0f);
            var pos = first - rot * host.PointA;
            pos.y = 0f;
            measured = localLen;
            RoomFrame.Set(pos, rot);
            if (TrySendReady())
                sent = true;
        }

        static bool TrySendReady()
        {
            if (NetworkClient.localPlayer == null)
                return false;
            var loco = NetworkClient.localPlayer.GetComponent<VrPlayerLocomotion>();
            if (loco == null)
                return false;
            loco.CmdCalibrationReady(measured);
            return true;
        }

        public static void Invalidate()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;
            step = 0;
            sent = false;
            fault = "";
            first = Vector3.zero;
            measured = 0f;
            RoomFrame.Clear();
            DestroyMark(ref markA);
            DestroyMark(ref markB);
        }

        static bool TriggerPressed(out Transform hand)
        {
            hand = null;
            EnsureTriggers();
            if (WasPressed(triggerRight))
                hand = FindNamed("RightHand Controller");
            else if (WasPressed(triggerLeft))
                hand = FindNamed("LeftHand Controller");
            return hand != null;
        }

        static void EnsureTriggers()
        {
            if (triggerRight != null)
                return;
            var actions = Resources.Load<InputActionAsset>("XRI Default Input Actions");
            if (actions == null)
                return;
            triggerRight = actions.FindAction("XRI Right Interaction/Activate", false);
            triggerLeft = actions.FindAction("XRI Left Interaction/Activate", false);
            triggerRight?.Enable();
            triggerLeft?.Enable();
        }

        static bool WasPressed(InputAction action)
        {
            return action != null && action.enabled && action.WasPressedThisFrame();
        }

        static bool TryTipPoint(Transform hand, out Vector3 point)
        {
            point = Vector3.zero;
            var origin = FindAnyObjectByType<XROrigin>();
            if (origin == null || hand == null)
                return false;
            var floorY = origin.transform.position.y;
            if (Mathf.Abs(hand.position.y - floorY) > FloorReach)
            {
                fault = "Опусти кончик контроллера на наклейку. ";
                return false;
            }

            point = hand.position;
            point.y = floorY;
            fault = "";
            return true;
        }

        static Transform FindNamed(string objectName)
        {
            var all = FindObjectsByType<Transform>(FindObjectsInactive.Exclude);
            for (var i = 0; i < all.Length; i++)
            {
                if (all[i].name == objectName)
                    return all[i];
            }
            return null;
        }

        static GameObject Place(GameObject mark, Vector3 point, Color color)
        {
            if (mark == null)
            {
                mark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mark.name = "CalibMark";
                var collider = mark.GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);
                mark.transform.localScale = Vector3.one * 0.12f;
                mark.GetComponent<Renderer>().sharedMaterial = QuestClientRig.Solid(color);
            }
            mark.transform.position = point + Vector3.up * 0.03f;
            return mark;
        }

        static void DestroyMark(ref GameObject mark)
        {
            if (mark == null)
                return;
            Destroy(mark);
            mark = null;
        }
    }
}
