using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SmenaStarter
{
    /// <summary>
    /// Схема как в VR_Menu: XR Origin, набор XRI Default Input Actions,
    /// канвас лобби — дочерний объект камеры шлема, мир, масштаб 0.001, луч курка.
    /// </summary>
    public class QuestClientRig : MonoBehaviour
    {
        const string ActionsResource = "XRI Default Input Actions";

        Camera xrCamera;
        GameObject xrOriginRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindAnyObjectByType<QuestClientRig>() != null)
                return;
            var go = new GameObject("SmenaXR");
            DontDestroyOnLoad(go);
            go.AddComponent<QuestClientRig>();
            go.AddComponent<FloorCalibrator>();
        }

        void Awake()
        {
            if (Application.platform == RuntimePlatform.Android)
                BuildRig();
            SceneManager.sceneLoaded += OnSceneLoaded;
            AdaptScene();
            if (xrOriginRoot != null)
                xrOriginRoot.SetActive(true);
            if (Application.platform == RuntimePlatform.Android)
                RefreshRays();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene _, LoadSceneMode __)
        {
            AdaptScene();
            if (Application.platform == RuntimePlatform.Android)
                RefreshRays();
        }

        void RefreshRays()
        {
            foreach (var ray in FindObjectsByType<XRRayInteractor>(FindObjectsInactive.Exclude))
            {
                ray.enabled = false;
                ray.enabled = true;
            }
        }

        void BuildRig()
        {
            var actions = Resources.Load<InputActionAsset>(ActionsResource);
            if (actions == null)
            {
                Debug.LogError("Smena: не найден Resources/" + ActionsResource + ".inputactions");
                return;
            }

            var origin = new GameObject("XR Origin Menu");
            origin.SetActive(false);
            origin.transform.SetParent(transform, false);
            var xrOrigin = origin.AddComponent<XROrigin>();
            var interaction = origin.AddComponent<XRInteractionManager>();
            var input = origin.AddComponent<InputActionManager>();
            input.actionAssets = new List<InputActionAsset> { actions };

            var offset = new GameObject("Camera Offset");
            offset.transform.SetParent(origin.transform, false);

            var camGo = new GameObject("XR Camera");
            camGo.transform.SetParent(offset.transform, false);
            camGo.tag = "MainCamera";
            xrCamera = camGo.AddComponent<Camera>();
            xrCamera.nearClipPlane = 0.05f;
            xrCamera.farClipPlane = 100f;
            camGo.AddComponent<AudioListener>();
            BindPose(camGo.AddComponent<TrackedPoseDriver>(), actions, "XRI Head");

            xrOrigin.CameraFloorOffsetObject = offset;
            xrOrigin.Camera = xrCamera;
            xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            xrOrigin.CameraYOffset = 0f;

            MakeHand("LeftHand Controller", actions, "XRI Left", "XRI Left Interaction",
                interaction, offset.transform);
            MakeHand("RightHand Controller", actions, "XRI Right", "XRI Right Interaction",
                interaction, offset.transform);
            xrOriginRoot = origin;
        }

        static void MakeHand(string name, InputActionAsset actions, string poseMap, string interactMap,
            XRInteractionManager manager, Transform parent)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.SetParent(parent, false);
            BindPose(go.AddComponent<TrackedPoseDriver>(), actions, poseMap);

            var ray = go.AddComponent<XRRayInteractor>();
            ray.interactionManager = manager;
            ray.enableUIInteraction = true;
            ray.maxRaycastDistance = 12f;
            BindButton(ray.selectInput, actions, interactMap, "Select", "Select Value");
            BindButton(ray.uiPressInput, actions, interactMap, "UI Press", "UI Press Value");

            var line = go.AddComponent<LineRenderer>();
            line.material = Solid(new Color(0.96f, 0.77f, 0.12f));
            line.useWorldSpace = true;
            var visual = go.AddComponent<XRInteractorLineVisual>();
            visual.lineWidth = 0.008f;
            visual.overrideInteractorLineLength = true;
            visual.lineLength = 8f;
            go.SetActive(true);
        }

        static void BindPose(TrackedPoseDriver driver, InputActionAsset actions, string map)
        {
            driver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            driver.positionInput = new InputActionProperty(
                InputActionReference.Create(actions.FindAction(map + "/Position", true)));
            driver.rotationInput = new InputActionProperty(
                InputActionReference.Create(actions.FindAction(map + "/Rotation", true)));
        }

        static void BindButton(XRInputButtonReader reader, InputActionAsset actions, string map, string performed, string value)
        {
            if (reader == null)
                return;
            reader.inputSourceMode = XRInputButtonReader.InputSourceMode.InputActionReference;
            reader.inputActionReferencePerformed = InputActionReference.Create(
                actions.FindAction(map + "/" + performed, true));
            reader.inputActionReferenceValue = InputActionReference.Create(
                actions.FindAction(map + "/" + value, true));
        }

        internal static Material Solid(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;
            return mat;
        }

        void AdaptScene()
        {
            var android = Application.platform == RuntimePlatform.Android;
            if (android)
            {
                DisableForeignCameras();
                StripEventSystemOffCanvases();
            }
            FixEventSystem(android);

            if (!android || xrCamera == null)
                return;

            var lobby = SceneManager.GetActiveScene().name.Contains("Lobby");
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            {
                var hud = canvas.GetComponent<LobbyHud>();
                if (hud != null && lobby)
                    AttachMenuCanvas(canvas);
                else if (canvas.transform.IsChildOf(xrCamera.transform))
                    canvas.gameObject.SetActive(lobby);
            }
        }

        static void StripEventSystemOffCanvases()
        {
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                var system = canvas.GetComponent<EventSystem>();
                if (system == null)
                    continue;
                DestroyModule(canvas.GetComponent<StandaloneInputModule>());
                DestroyModule(canvas.GetComponent<XRUIInputModule>());
                DestroyModule(canvas.GetComponent<InputSystemUIInputModule>());
                DestroyImmediate(system);
            }
        }

        static void FixEventSystem(bool android)
        {
            var systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
            EventSystem keeper = null;
            foreach (var system in systems)
            {
                if (keeper == null)
                {
                    keeper = system;
                    continue;
                }

                if (system.GetComponent<Canvas>() != null)
                {
                    DestroyModule(system.GetComponent<StandaloneInputModule>());
                    DestroyModule(system.GetComponent<XRUIInputModule>());
                    DestroyModule(system.GetComponent<InputSystemUIInputModule>());
                    DestroyImmediate(system);
                }
                else
                    DestroyImmediate(system.gameObject);
            }

            if (keeper == null)
                keeper = new GameObject("EventSystem").AddComponent<EventSystem>();

            DestroyModule(keeper.GetComponent<StandaloneInputModule>());
            if (android)
            {
                DestroyModule(keeper.GetComponent<InputSystemUIInputModule>());
                if (keeper.GetComponent<XRUIInputModule>() == null)
                    keeper.gameObject.AddComponent<XRUIInputModule>();
            }
            else if (keeper.GetComponent<InputSystemUIInputModule>() == null &&
                     keeper.GetComponent<XRUIInputModule>() == null)
            {
                keeper.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        static void DestroyModule(Component module)
        {
            if (module != null)
                DestroyImmediate(module);
        }

        void AttachMenuCanvas(Canvas canvas)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
                scaler.enabled = false;

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = xrCamera;
            var tracked = canvas.GetComponent<TrackedDeviceGraphicRaycaster>();
            if (tracked == null)
                tracked = canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            tracked.ignoreReversedGraphics = false;
            tracked.checkFor3DOcclusion = false;

            var rt = canvas.GetComponent<RectTransform>();
            rt.SetParent(xrCamera.transform, false);
            rt.localRotation = Quaternion.identity;
            rt.localPosition = new Vector3(0f, -0.05f, 2f);
            rt.localScale = Vector3.one * 0.001f;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1920f, 1080f);
            rt.anchoredPosition = Vector2.zero;

            foreach (var field in canvas.GetComponentsInChildren<InputField>(true))
            {
                if (field.GetComponent<QuestLobbyKeyboard>() == null)
                    field.gameObject.AddComponent<QuestLobbyKeyboard>();
            }
        }

        void DisableForeignCameras()
        {
            foreach (var cam in FindObjectsByType<Camera>(FindObjectsInactive.Include))
            {
                if (cam == xrCamera)
                    continue;
                cam.enabled = false;
                if (cam.CompareTag("MainCamera"))
                    cam.tag = "Untagged";
                var listener = cam.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = false;
            }
        }
    }
}
