using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// GameScene에 오클루전 / LOD / 프러스텀 컬링 설정을 한 번에 적용하는 에디터 도구
// 같은 메뉴를 여러 번 실행해도 결과가 같다(이미 있는 LODGroup은 재사용).
public static class SceneCullingSetup
{
    private const string SceneName = "GameScene";
    private const string MapStaticPath = "------------UI------------/Map/Map_v1/Static";

    // 소품 : 화면에서 이 높이 비율보다 작아지면 그리지 않음 (lodBias=2라 실제로는 절반 높이에서 컬링)
    private const float PropCullScreenHeight = 0.04f;

    private const float MainCameraFar = 250f; // 맵 대각선 약 212m + 여유
    private const float MinimapCameraFar = 20f; // 플레이어 머리 위 8m에서 지면까지 + 여유

    // 소품 접두사를 먼저 검사한다 (Concrete_fence_v1_column, Oil_tank_v2가 구조물 접두사에 걸리지 않게)
    private static readonly string[] PropPrefixes =
    {
        "Wooden_box_", "Barrel_", "Palet_", "Conditioner_", "Electric_box_", "Generator_",
        "Dumpsters_", "Road_block_", "Pipes_set_", "Industrial_pipe_", "Oil_tank_v2",
        "Concrete_fence_v1_column"
    };

    private static readonly string[] StructurePrefixes =
    {
        "Hangar_", "Oil_tank_v1", "Cargo_container_", "Concrete_fence_", "UNIConcrete_wall_", "Road_set_"
    };

    private enum Category
    {
        Structure,
        Prop,
        Unknown
    }

    [MenuItem("Tools/Culling/GameScene 컬링 설정 적용")]
    private static void Apply()
    {
        // 1. 대상 씬과 오브젝트가 모두 있는지 확인
        if (EditorSceneManager.GetActiveScene().name != SceneName)
        {
            Debug.LogError($"[Culling] {SceneName} 씬을 열고 실행하세요.");
            return;
        }

        GameObject mapStatic = GameObject.Find(MapStaticPath);
        Camera mainCamera = FindCamera("Main Camera");
        Camera minimapCamera = FindCamera("MinimapCamera");
        if (mapStatic == null || mainCamera == null || minimapCamera == null)
        {
            Debug.LogError("[Culling] Map/Map_v1/Static, Main Camera, MinimapCamera 중 찾지 못한 오브젝트가 있습니다.");
            return;
        }

        StringBuilder occlusionLog = new("[Culling] 오클루전 Static 플래그\n| 분류 | 오브젝트 | 변경 전 → 후 |\n|---|---|---|\n");
        StringBuilder lodLog = new("[Culling] LOD 컬링\n| 오브젝트 | 렌더러 수 | 변경 |\n|---|---|---|\n");
        int occlusionCount = 0;
        int lodCount = 0;

        // 2. Static의 직계 자식 단위로 분류하고, 하위 오브젝트 전체에 같은 분류를 적용
        foreach (Transform child in mapStatic.transform)
        {
            if (IsMovable(child.gameObject))
            {
                Debug.LogWarning($"[Culling] 움직이는 오브젝트라 건너뜀: {child.name}");
                continue;
            }

            Category category = Classify(child.name);
            if (category == Category.Unknown)
            {
                Debug.LogWarning($"[Culling] 분류 목록에 없어 건너뜀: {child.name}");
                continue;
            }

            foreach (Transform t in child.GetComponentsInChildren<Transform>(true))
            {
                if (SetOcclusionFlags(t.gameObject, category, occlusionLog))
                {
                    occlusionCount++;
                }
            }

            if (category == Category.Prop)
            {
                SetupPropLod(child.gameObject, lodLog);
                lodCount++;
            }
        }

        // 3. 카메라 설정 (프러스텀 범위 축소, 미니맵 오클루전 끄기)
        StringBuilder cameraLog = new("[Culling] 카메라\n| 카메라 | 항목 | 변경 전 → 후 |\n|---|---|---|\n");
        Undo.RecordObject(mainCamera, "Culling Setup");
        Undo.RecordObject(minimapCamera, "Culling Setup");

        cameraLog.AppendLine($"| Main Camera | Far Clip | {mainCamera.farClipPlane} → {MainCameraFar} |");
        mainCamera.farClipPlane = MainCameraFar;

        cameraLog.AppendLine($"| MinimapCamera | Occlusion Culling | {minimapCamera.useOcclusionCulling} → False |");
        minimapCamera.useOcclusionCulling = false;

        cameraLog.AppendLine($"| MinimapCamera | Far Clip | {minimapCamera.farClipPlane} → {MinimapCameraFar} |");
        minimapCamera.farClipPlane = MinimapCameraFar;

        EditorSceneManager.MarkSceneDirty(mapStatic.scene);

        Debug.Log(occlusionLog.Insert(0, $"변경 {occlusionCount}개 ").ToString());
        Debug.Log(lodLog.Insert(0, $"LODGroup {lodCount}개 ").ToString());
        Debug.Log(cameraLog.ToString());
    }

    private static Category Classify(string objectName)
    {
        foreach (string prefix in PropPrefixes)
        {
            if (objectName.StartsWith(prefix))
                return Category.Prop;
        }

        foreach (string prefix in StructurePrefixes)
        {
            if (objectName.StartsWith(prefix))
                return Category.Structure;
        }

        return Category.Unknown;
    }

    // 좀비·드럼통·상자처럼 움직이거나 부서지는 오브젝트는 Static으로 만들면 안 됨
    private static bool IsMovable(GameObject go)
    {
        return go.GetComponentInChildren<Rigidbody>(true) != null
               || go.GetComponentInChildren<Animator>(true) != null
               || go.GetComponentInChildren<Drum>(true) != null
               || go.GetComponentInChildren<Box>(true) != null
               || go.GetComponentInChildren<IDamageable>(true) != null;
    }

    // Occluder/Occludee 비트만 바꾸고 Batching·GI 등 나머지 비트는 유지
    private static bool SetOcclusionFlags(GameObject go, Category category, StringBuilder log)
    {
        StaticEditorFlags before = GameObjectUtility.GetStaticEditorFlags(go);
        StaticEditorFlags after = before | StaticEditorFlags.OccludeeStatic;
        if (category == Category.Structure)
        {
            after |= StaticEditorFlags.OccluderStatic;
        }
        else
        {
            after &= ~StaticEditorFlags.OccluderStatic;
        }
        if (after == before) return false;

        Undo.RecordObject(go, "Culling Setup");
        GameObjectUtility.SetStaticEditorFlags(go, after);

        string label = category == Category.Structure ? "구조물" : "소품";
        log.AppendLine($"| {label} | {go.name} | {DescribeOcclusion(before)} → {DescribeOcclusion(after)} |");
        return true;
    }

    private static string DescribeOcclusion(StaticEditorFlags flags)
    {
        bool occluder = (flags & StaticEditorFlags.OccluderStatic) != 0;
        bool occludee = (flags & StaticEditorFlags.OccludeeStatic) != 0;
        if (occluder && occludee) return "Occluder+Occludee";

        if (occludee) return "Occludee";

        return occluder ? "Occluder" : "없음";
    }

    // 맵 메시가 이미 로우폴리라 저해상도 단계 없이 LOD0 + Culled만 둔다
    private static void SetupPropLod(GameObject prop, StringBuilder log)
    {
        Renderer[] renderers = prop.GetComponentsInChildren<Renderer>(true);

        LODGroup lodGroup = prop.GetComponent<LODGroup>();
        string change = "기존 LODGroup 갱신";
        if (lodGroup == null)
        {
            lodGroup = Undo.AddComponent<LODGroup>(prop);
            change = "LODGroup 추가";
        }

        Undo.RecordObject(lodGroup, "Culling Setup");
        lodGroup.fadeMode = LODFadeMode.None;
        lodGroup.SetLODs(new[] { new LOD(PropCullScreenHeight, renderers) });
        lodGroup.RecalculateBounds();

        log.AppendLine(
            $"| {prop.name} | {renderers.Length} | {change} (LOD0 → Culled {PropCullScreenHeight * 100f}%) |");
    }

    private static Camera FindCamera(string cameraName)
    {
        foreach (Camera cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (cam.name == cameraName)
                return cam;
        }

        return null;
    }
}