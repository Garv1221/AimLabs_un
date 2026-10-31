#if UNITY_EDITOR
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;

[InitializeOnLoad]
public static class ProjectBootstrapper
{
    private const string ScenePath = "Assets/Scenes/ShootingRange.unity";
    static ProjectBootstrapper() { EditorApplication.delayCall += AutoSetup; }
    private static void AutoSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        EnsureFolders();
        ConfigureProject();
        if (File.Exists(ScenePath) && AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)) return;
        GenerateScene();
    }
    [MenuItem("Target Shooting Simulator/Generate or Repair Project")]
    public static void GenerateProject()
    {
        EnsureFolders();
        ConfigureProject();
        if (File.Exists(ScenePath) && AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))
        {
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Existing Shooting Range scene preserved; project settings repaired.");
            return;
        }
        GenerateScene();
    }
    private static void GenerateScene()
    {
        CreateMaterials();
        GameObject targetPrefab = CreateTargetPrefab();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildScene(targetPrefab);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("Target Shooting Simulator scene generated at " + ScenePath);
    }
    private static void ConfigureProject()
    {
        // Unity 6.6 does not expose Active Input Handling as a supported PlayerSettings API.
        // ProjectSettings/ProjectSettings.asset keeps activeInputHandler: 0 (Input Manager Old).
        const string path="Assets/Settings/TargetShooterURP.asset";
        UniversalRenderPipelineAsset pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
        if(!pipeline)
        {
            pipeline=UniversalRenderPipelineAsset.Create();
            AssetDatabase.CreateAsset(pipeline,path);
        }
        const string rendererPath = "Assets/Settings/TargetShooterRenderer.asset";
        UniversalRendererData renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
        if (!renderer)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            renderer.name = "TargetShooterRenderer";
            AssetDatabase.CreateAsset(renderer, rendererPath);
        }
        // Use serialized fields so this remains compatible with the URP 17 renderer list API.
        SerializedObject pipelineObject = new SerializedObject(pipeline);
        SerializedProperty rendererList = pipelineObject.FindProperty("m_RendererDataList");
        if (rendererList != null)
        {
            rendererList.arraySize = 1;
            rendererList.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
        }
        SerializedProperty legacyRenderer = pipelineObject.FindProperty("m_RendererData");
        if (legacyRenderer != null) legacyRenderer.objectReferenceValue = renderer;
        SerializedProperty defaultIndex = pipelineObject.FindProperty("m_DefaultRendererIndex");
        if (defaultIndex != null) defaultIndex.intValue = 0;
        pipelineObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(pipeline);
        GraphicsSettings.defaultRenderPipeline=pipeline;
        // Assign the currently selected quality level; other levels inherit the Graphics default
        // unless they have an explicit per-level override.
        QualitySettings.renderPipeline=pipeline;
    }
    private static void EnsureFolders()
    {
        string[] dirs={"Assets/Scenes","Assets/Settings","Assets/Scripts/Player","Assets/Scripts/Weapons","Assets/Scripts/Targets","Assets/Scripts/Managers","Assets/Scripts/UI","Assets/Editor","Assets/Prefabs","Assets/Materials","Assets/Audio","Assets/Documentation"};
        foreach(string dir in dirs) if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }
    private static Material MakeMaterial(string path, Color color)
    {
        Material mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,path);}
        mat.color=color; EditorUtility.SetDirty(mat); return mat;
    }
    private static Material floorMat, wallMat, targetMat, trimMat;
    private static void CreateMaterials()
    {
        floorMat=MakeMaterial("Assets/Materials/RangeFloor.mat",new Color(.12f,.18f,.2f));
        wallMat=MakeMaterial("Assets/Materials/RangeWall.mat",new Color(.08f,.12f,.15f));
        targetMat=MakeMaterial("Assets/Materials/TargetRed.mat",new Color(.9f,.16f,.08f));
        trimMat=MakeMaterial("Assets/Materials/TargetCenter.mat",new Color(1f,.78f,.12f));
    }
    private static GameObject CreateTargetPrefab()
    {
        GameObject root=GameObject.CreatePrimitive(PrimitiveType.Sphere);root.name="Target";
        root.GetComponent<Renderer>().sharedMaterial=targetMat;
        Target target=root.AddComponent<Target>(); target.Configure(10);
        root.AddComponent<TargetMovement>();
        string path="Assets/Prefabs/Target.prefab";
        GameObject existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(existing) PrefabUtility.SaveAsPrefabAsset(root,path); else PrefabUtility.SaveAsPrefabAsset(root,path);
        Object.DestroyImmediate(root); return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
    private static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.position=pos;o.transform.localScale=scale;
        if(mat)o.GetComponent<Renderer>().sharedMaterial=mat;return o;
    }
    private static void BuildScene(GameObject targetPrefab)
    {
        GameObject environment=new GameObject("Shooting Range Environment");
        Cube("Range Floor",new Vector3(0,-.25f,12),new Vector3(26,.5f,36),floorMat).transform.SetParent(environment.transform);
        Cube("Back Wall",new Vector3(0,4,29),new Vector3(26,8,.5f),wallMat).transform.SetParent(environment.transform);
        Cube("Left Wall",new Vector3(-13,4,12),new Vector3(.5f,8,35),wallMat).transform.SetParent(environment.transform);
        Cube("Right Wall",new Vector3(13,4,12),new Vector3(.5f,8,35),wallMat).transform.SetParent(environment.transform);
        Cube("Ceiling",new Vector3(0,8,12),new Vector3(26,.3f,35),wallMat).transform.SetParent(environment.transform);
        for(int i=-10;i<=10;i+=2) Cube("Lane Mark",new Vector3(i,.012f,12),new Vector3(.035f,.02f,32),trimMat).transform.SetParent(environment.transform);
        GameObject lightObj=new GameObject("Range Lighting");Light light=lightObj.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.15f;lightObj.transform.rotation=Quaternion.Euler(50,-30,0);
        RenderSettings.ambientLight=new Color(.48f,.54f,.6f);
        GameObject player=new GameObject("Player");player.transform.position=new Vector3(0,0,0);CharacterController character=player.AddComponent<CharacterController>();character.center=new Vector3(0,1,0);character.height=2;character.radius=.38f;player.AddComponent<PlayerController>();
        GameObject cameraObj=new GameObject("Player Camera");cameraObj.transform.SetParent(player.transform,false);cameraObj.transform.localPosition=new Vector3(0,1.65f,0);
        Camera cam=cameraObj.AddComponent<Camera>();cam.fieldOfView=78;cameraObj.AddComponent<AudioListener>();MouseLook look=cameraObj.AddComponent<MouseLook>();look.Configure(player.transform);
        GameObject weapon=new GameObject("Pistol");weapon.transform.SetParent(cameraObj.transform,false);weapon.transform.localPosition=new Vector3(.25f,-.24f,.48f);weapon.transform.localRotation=Quaternion.Euler(4,-2,0);
        GameObject grip=GameObject.CreatePrimitive(PrimitiveType.Cube);grip.name="Pistol Grip";grip.transform.SetParent(weapon.transform,false);grip.transform.localPosition=new Vector3(0,-.06f,0);grip.transform.localScale=new Vector3(.09f,.23f,.11f);grip.GetComponent<Renderer>().sharedMaterial=wallMat;Object.DestroyImmediate(grip.GetComponent<Collider>());
        GameObject barrel=GameObject.CreatePrimitive(PrimitiveType.Cube);barrel.name="Pistol Barrel";barrel.transform.SetParent(weapon.transform,false);barrel.transform.localPosition=new Vector3(0,.025f,.15f);barrel.transform.localScale=new Vector3(.11f,.09f,.34f);barrel.GetComponent<Renderer>().sharedMaterial=trimMat;Object.DestroyImmediate(barrel.GetComponent<Collider>());
        GameObject flash=GameObject.CreatePrimitive(PrimitiveType.Sphere);flash.name="Muzzle Flash";flash.transform.SetParent(weapon.transform,false);flash.transform.localPosition=new Vector3(0,.025f,.34f);flash.transform.localScale=Vector3.one*.09f;flash.GetComponent<Renderer>().sharedMaterial=trimMat;Object.DestroyImmediate(flash.GetComponent<Collider>());flash.SetActive(false);
        GunController gun=weapon.AddComponent<GunController>();gun.Configure(cam,flash);
        GameObject targets=new GameObject("Targets");
        GameObject spawnerObj=new GameObject("Target Spawner");TargetSpawner spawner=spawnerObj.AddComponent<TargetSpawner>();spawner.Configure(targetPrefab,targets.transform);
        // A few visible practice targets are present before the first round.
        for(int i=-2;i<=2;i++){GameObject t=(GameObject)PrefabUtility.InstantiatePrefab(targetPrefab);t.name="Practice Target";t.transform.SetParent(targets.transform);t.transform.position=new Vector3(i*3,2.5f,16);t.GetComponent<TargetMovement>().enabled=false;}
        GameObject managers=new GameObject("Game Systems");ScoreManager score=managers.AddComponent<ScoreManager>();GameManager gm=managers.AddComponent<GameManager>();
        GameObject canvasObj=new GameObject("Game UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
        Canvas canvas=canvasObj.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=canvasObj.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=.5f;
        UIManager ui=canvasObj.AddComponent<UIManager>();ui.Initialize(canvas);
        GameObject eventSystem=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        SerializedObject gso=new SerializedObject(gm);gso.FindProperty("ui").objectReferenceValue=ui;gso.FindProperty("spawner").objectReferenceValue=spawner;gso.ApplyModifiedPropertiesWithoutUndo();
        Selection.activeGameObject=player;
    }
    private static void AddSceneToBuildSettings()
    {
        EditorBuildSettingsScene[] old=EditorBuildSettings.scenes;foreach(var s in old)if(s.path==ScenePath)return;
        var list=new System.Collections.Generic.List<EditorBuildSettingsScene>(old);list.Insert(0,new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=list.ToArray();
    }
}
#endif
