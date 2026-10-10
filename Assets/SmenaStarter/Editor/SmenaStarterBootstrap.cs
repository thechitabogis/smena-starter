#if UNITY_EDITOR
using System.IO;
using kcp2k;
using Mirror;
using SmenaStarter;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class SmenaStarterBootstrap
{
    const string Root = "Assets/SmenaStarter";
    const string Scenes = Root + "/Scenes";
    const string Prefabs = Root + "/Prefabs";

    [MenuItem("Smena/Build Starter Scenes")]
    public static void Build()
    {
        EnsureFolders();
        ApplyPlayerSettings();

        var player = MakePlayerPrefab();
        var hostMarker = MakeHostMarkerPrefab();
        var grabCube = MakeGrabCubePrefab();
        var plaque = MakePlaquePrefab();

        var lobby = BuildLobby(player, hostMarker);
        var room = BuildRoom(grabCube, plaque);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(lobby, true),
            new EditorBuildSettingsScene(room, true)
        };

        EditorSceneManager.OpenScene(lobby);
        AssignNetworkScenes(lobby, room);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Smena: сцены 00_Lobby и 01_Room собраны. Пакеты Mirror/XR должны уже импортироваться.");
    }

    static void ApplyPlayerSettings()
    {
        PlayerSettings.companyName = "KonstantaTime";
        PlayerSettings.productName = "VidR Smena Multiplayer";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "by.ndt.vidr.smena2026");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "by.ndt.vidr.smena2026");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EnableNewInputSystem();
    }

    static void EnableNewInputSystem()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (assets == null || assets.Length == 0)
            return;
        var so = new SerializedObject(assets[0]);
        var prop = so.FindProperty("activeInputHandler");
        if (prop == null)
            return;
        // 0 = Input Manager (Old), 1 = Input System Package, 2 = Both.
        // Both на Android не поддерживается и ломает контроллеры Quest.
        prop.intValue = 1;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AssignNetworkScenes(string lobbyPath, string roomPath)
    {
        var nm = Object.FindAnyObjectByType<SmenaNetworkManager>();
        if (nm == null)
            return;
        nm.offlineScene = lobbyPath;
        nm.onlineScene = "";
        nm.roomScene = roomPath;
        EditorUtility.SetDirty(nm);
        EditorSceneManager.MarkSceneDirty(nm.gameObject.scene);
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(Root))
            AssetDatabase.CreateFolder("Assets", "SmenaStarter");
        if (!AssetDatabase.IsValidFolder(Scenes))
            AssetDatabase.CreateFolder(Root, "Scenes");
        if (!AssetDatabase.IsValidFolder(Prefabs))
            AssetDatabase.CreateFolder(Root, "Prefabs");
        if (!AssetDatabase.IsValidFolder(Root + "/Scripts"))
            AssetDatabase.CreateFolder(Root, "Scripts");
    }

    static GameObject MakePlayerPrefab()
    {
        var go = new GameObject("VrPlayer");
        go.transform.position = Vector3.up;
        go.AddComponent<NetworkIdentity>();
        AddNetXf(go);

        var model = new GameObject("Model");
        model.transform.SetParent(go.transform, false);

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(model.transform, false);
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().sharedMaterial = NewMat("Player", new Color(0.75f, 0.8f, 0.85f));

        var left = MakeHandVisual("LeftHand", model.transform, new Color(0.2f, 0.6f, 1f));
        var right = MakeHandVisual("RightHand", model.transform, new Color(0.96f, 0.77f, 0.12f));

        var vis = go.AddComponent<VrPlayerVisual>();
        vis.head = go.transform;
        vis.leftHand = left.transform;
        vis.rightHand = right.transform;
        AddHandNet(go, left.transform);
        AddHandNet(go, right.transform);
        go.AddComponent<VrPlayerLocomotion>();
        var path = Prefabs + "/VrPlayer.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static GameObject MakeHandVisual(string name, Transform parent, Color color)
    {
        var hand = new GameObject(name);
        hand.transform.SetParent(parent, false);
        var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh.name = "Mesh";
        mesh.transform.SetParent(hand.transform, false);
        mesh.transform.localPosition = new Vector3(0f, 0f, -0.04f);
        mesh.transform.localScale = new Vector3(0.04f, 0.03f, 0.12f);
        Object.DestroyImmediate(mesh.GetComponent<Collider>());
        mesh.GetComponent<Renderer>().sharedMaterial = NewMat(name, color);
        return hand;
    }

    static void AddHandNet(GameObject owner, Transform target)
    {
        var nt = owner.AddComponent<NetworkTransformUnreliable>();
        nt.target = target;
        nt.syncScale = false;
        nt.coordinateSpace = CoordinateSpace.Local;
        nt.syncDirection = SyncDirection.ClientToServer;
    }

    static GameObject MakeHostMarkerPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "HostMarker";
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        go.GetComponent<Renderer>().sharedMaterial = NewMat("HostMarker", new Color(0.2f, 0.6f, 1f));
        go.AddComponent<NetworkIdentity>();
        AddNetXf(go);
        var path = Prefabs + "/HostMarker.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static GameObject MakeGrabCubePrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "GrabCube";
        go.transform.localScale = Vector3.one * 0.25f;
        go.GetComponent<Renderer>().sharedMaterial = NewMat("GrabCube", new Color(0.96f, 0.77f, 0.1f));
        go.AddComponent<NetworkIdentity>();
        AddNetXf(go);
        var grab = go.AddComponent<XRGrabInteractable>();
        go.AddComponent<GrabNetworkAuthority>();
        var path = Prefabs + "/GrabCube.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static GameObject MakePlaquePrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "IdeaPlaque";
        go.transform.localScale = new Vector3(0.4f, 0.3f, 0.04f);
        go.GetComponent<Renderer>().sharedMaterial = NewMat("IdeaPlaque", new Color(0.9f, 0.9f, 0.85f));
        go.AddComponent<NetworkIdentity>();
        AddNetXf(go);
        go.AddComponent<XRGrabInteractable>();
        go.AddComponent<GrabNetworkAuthority>();
        go.AddComponent<ColorSyncExample>();
        var path = Prefabs + "/IdeaPlaque.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static void AddNetXf(GameObject go)
    {
        go.AddComponent<NetworkTransformUnreliable>();
    }

    static Material NewMat(string assetName, Color c)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            throw new System.InvalidOperationException("Shader Universal Render Pipeline/Lit не найден. Назначь URP Asset в Graphics.");
        var dir = Root + "/Materials";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder(Root, "Materials");
        var path = dir + "/" + assetName + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.shader = shader;
            existing.color = c;
            return existing;
        }
        var created = new Material(shader) { name = assetName, color = c };
        AssetDatabase.CreateAsset(created, path);
        return created;
    }

    static string BuildLobby(GameObject player, GameObject hostMarker)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        SceneManager.SetActiveScene(scene);
        var light = Object.FindAnyObjectByType<Light>();
        if (light != null)
            light.intensity = 1.1f;

        Floor("LobbyFloor", new Color(0.12f, 0.14f, 0.18f));
        MakeNetworkManager(player, hostMarker);
        MakeLobbyUi();

        var path = Scenes + "/00_Lobby.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    static string BuildRoom(GameObject cubePrefab, GameObject plaquePrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Floor("RoomFloor", new Color(0.18f, 0.2f, 0.22f));
        var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
        walls.name = "BackWall";
        walls.transform.position = new Vector3(0, 1.5f, 4);
        walls.transform.localScale = new Vector3(10, 3, 0.2f);
        walls.GetComponent<Renderer>().sharedMaterial = NewMat("Wall", new Color(0.55f, 0.56f, 0.58f));

        var cube = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab);
        cube.name = "GrabCube_A";
        cube.transform.position = new Vector3(-0.4f, 1.1f, 0.5f);
        var cubeB = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab);
        cubeB.name = "GrabCube_B";
        cubeB.transform.position = new Vector3(0.4f, 1.1f, 0.5f);

        for (int i = 0; i < 5; i++)
        {
            var slot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slot.name = "IdeaSlot_" + (i + 1);
            slot.transform.position = new Vector3(-2f + i * 1f, 0.05f, 1.6f);
            slot.transform.localScale = new Vector3(0.7f, 0.08f, 0.7f);
            slot.GetComponent<Renderer>().sharedMaterial = NewMat("IdeaSlot", new Color(1f, 0.78f, 0.15f));
            var marker = slot.AddComponent<IdeaSlot>();
            marker.slotName = "Слот " + (i + 1);
        }

        var plaque = (GameObject)PrefabUtility.InstantiatePrefab(plaquePrefab);
        plaque.name = "ExamplePlaque";
        plaque.transform.position = new Vector3(-2f, 0.35f, 1.6f);

        var rig = new GameObject("Rigs");
        var switcher = rig.AddComponent<PlatformRigSwitcher>();
        var pcCam = Camera.main != null ? Camera.main.gameObject : new GameObject("PcCamera", typeof(Camera));
        pcCam.name = "PcHostCamera";
        if (pcCam.GetComponent<HostOrbitCamera>() == null)
            pcCam.AddComponent<HostOrbitCamera>().lookAt = cube.transform;
        switcher.pcCameraRoot = pcCam;
        var xr = new GameObject("XrOriginPlaceholder");
        xr.transform.SetParent(rig.transform);
        xr.SetActive(false);
        switcher.xrOriginRoot = xr;

        var path = Scenes + "/01_Room.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    static void Floor(string name, Color color)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = name;
        floor.GetComponent<Renderer>().sharedMaterial = NewMat(name, color);
    }

    static SmenaNetworkManager MakeNetworkManager(GameObject player, GameObject hostMarker)
    {
        var existing = Object.FindAnyObjectByType<SmenaNetworkManager>();
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("NetworkManager");
        var kcp = go.AddComponent<KcpTransport>();
        kcp.Port = 7777;
        var nm = go.AddComponent<SmenaNetworkManager>();
        nm.transport = kcp;
        nm.playerPrefab = player;
        nm.hostMarkerPrefab = hostMarker;
        nm.dontDestroyOnLoad = true;
        nm.offlineScene = "";
        nm.onlineScene = "";
        nm.spawnPrefabs.Clear();
        if (hostMarker != null && !nm.spawnPrefabs.Contains(hostMarker))
            nm.spawnPrefabs.Add(hostMarker);

        var rosterGo = new GameObject("LobbyRoster");
        rosterGo.transform.SetParent(go.transform, false);
        rosterGo.AddComponent<NetworkIdentity>();
        rosterGo.AddComponent<LobbyRoster>();
        return nm;
    }

    static void MakeLobbyUi()
    {
        var canvasGo = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

        if (Object.FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var panel = canvasGo.GetComponent<CanvasGroup>();
        var hud = canvasGo.AddComponent<LobbyHud>();
        hud.panel = panel;
        hud.manager = Object.FindAnyObjectByType<SmenaNetworkManager>();

        hud.ipHint = MakeText(canvasGo.transform, "IpHint", "IP этого ПК", 24, new Vector2(0, 280));
        hud.countLabel = MakeText(canvasGo.transform, "Count", "Игроков в лобби: 0", 32, new Vector2(0, 200));
        hud.statusLabel = MakeText(canvasGo.transform, "Status", "Не в сети", 22, new Vector2(0, 140));
        hud.ipField = MakeInput(canvasGo.transform, "IpField", new Vector2(0, 60));
        hud.hostButton = MakeButton(canvasGo.transform, "Host", "HOST", new Vector2(-160, -40));
        hud.connectButton = MakeButton(canvasGo.transform, "Connect", "CONNECT", new Vector2(160, -40));
        hud.goRoomButton = MakeButton(canvasGo.transform, "GoRoom", "В КОМНАТУ", new Vector2(0, -140));
    }

    static Text MakeText(Transform parent, string name, string value, int size, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(900, 60);
        rt.anchoredPosition = pos;
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = value;
        return t;
    }

    static InputField MakeInput(Transform parent, string name, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(480, 56);
        rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.9f);
        var input = go.AddComponent<InputField>();
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12, 4);
        trt.offsetMax = new Vector2(-12, -4);
        var text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.black;
        text.supportRichText = false;
        input.textComponent = text;
        input.text = "";
        return input;
    }

    static Button MakeButton(Transform parent, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 64);
        rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.96f, 0.77f, 0.12f);
        var btn = go.AddComponent<Button>();
        var tgo = new GameObject("Text");
        tgo.transform.SetParent(go.transform, false);
        var trt = tgo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var t = tgo.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 26;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.black;
        t.text = label;
        btn.targetGraphic = img;
        return btn;
    }
}
#endif
