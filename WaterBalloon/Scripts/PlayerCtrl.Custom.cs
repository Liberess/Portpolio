using Containers;
using DecalCollider.Runtime;
using Fusion;
using UnityEngine;
using RuntimeDecalCollider = DecalCollider.Runtime.DecalCollider;

/// <summary>
/// PlayerCtrl.Custom
/// </summary>
public partial class PlayerCtrl
{
    [Header("Customization")]
    [SerializeField] private Renderer customBodyRenderer;
    [SerializeField] private Material[] customBodyMaterials;
    [SerializeField] private RuntimeDecalCollider customFaceDecal;
    [SerializeField] private Renderer customFaceRenderer;
    [SerializeField] private Material[] customFaceMaterials;
    [SerializeField] private Transform[] customHatParts;
    [SerializeField] private Transform[] customBackParts;
    [SerializeField] private Transform[] customAccessoryParts;

    private const int NoneId = CostumeProgress.NoneItemId;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

    [Networked] private int NetBodyId { get; set; }
    [Networked] private int NetFaceId { get; set; }
    [Networked] private int NetHatId { get; set; }
    [Networked] private int NetBackId { get; set; }
    [Networked] private int NetAccessoryId { get; set; }
    [Networked] private int NetColorId { get; set; }

    private MaterialPropertyBlock customBodyBlock;
    private EquipData lastAppliedCustomData;
    private bool hasAppliedCustomData;

    private bool customizationSynced;
    private float customizationResendTimer;

    public void ApplyPreviewCustomization(EquipData equipData)
    {
        CacheCustomizationRefs();
        ApplyCustomization(equipData, true);
    }

    /// <summary>
    /// 현재 네트워크로 동기화된 외형 데이터를 반환한다(전 클라이언트 동일).
    /// 시상대 Preview 모델에 다른 플레이어 외형을 복제할 때 사용.
    /// </summary>
    public EquipData GetCurrentEquipData()
    {
        return GetNetworkCustomizationData();
    }

    private void Custom_Spawned()
    {
        CacheCustomizationRefs();
        ConfigureFaceDecal();
        ValidateCustomizationCounts();

        if (HasLocalPlayerAuthority)
            SendCustomizationRpc();

        ApplyCustomization(GetNetworkCustomizationData(), true);
    }

    // InputAuthority 클라에서만 호출. 네트워크에 반영된 외형이 로컬이 원하는 값과 일치할 때까지 RPC를 재전송한다.
    // 단순히 "NetColorId > 0이면 완료"로 판정하면 다음 경우 빨강(colorId=1)에 영구 고정된다:
    //  - NetworkObject 풀 재사용으로 이전 매치의 NetColorId(예: 1)가 남아 있어 재전송 가드가 즉시 트립
    //  - Spawned() 초기 RPC 누락/지연 중 NetColorId가 기본값/stale 값으로 남음
    // host의 ClampRequiredId가 잘못된 값도 항상 1 이상으로 clamp하므로 "0이 아님"만으로는 정상 동기화를 보장할 수 없다.
    // 따라서 로컬이 원하는 외형(GetDesiredCustomizationData)과 네트워크 값이 완전히 일치할 때까지 재전송한다
    // (닉네임 TrySyncNickname과 동일한 패턴).
    private void TrySyncCustomization()
    {
        if (customizationSynced)
            return;

        EquipData desired = GetDesiredCustomizationData();

        if (IsSameCustomization(GetNetworkCustomizationData(), desired))
        {
            customizationSynced = true;
            return;
        }

        customizationResendTimer -= Time.unscaledDeltaTime;

        if (customizationResendTimer > 0f)
            return;

        customizationResendTimer = 0.5f;
        SendCustomizationRpc();
    }

    private void SendCustomizationRpc()
    {
        EquipData equipData = GetDesiredCustomizationData();

        RPC_SetCustomization(
            equipData.bodyId,
            equipData.faceId,
            equipData.hatId,
            equipData.backId,
            equipData.accessoryId,
            equipData.colorId);
    }

    // 로컬 저장된 외형(EquipPrefs.Current)을 현재 프리팹 리소스 개수 기준으로 clamp한 값.
    // 송신(SendCustomizationRpc)과 동기화 판정(TrySyncCustomization)이 동일한 clamp를 거치도록 공유한다.
    // RPC_SetCustomization(host)도 같은 clamp를 사용하므로 정상 반영 시 네트워크 값과 정확히 일치한다.
    private EquipData GetDesiredCustomizationData()
    {
        EquipData equipData = EquipPrefs.Current;

        equipData.bodyId = ClampRequiredId(equipData.bodyId, GetBodyCount());
        equipData.faceId = ClampOptionalId(equipData.faceId, GetFaceCount());
        equipData.hatId = ClampOptionalId(equipData.hatId, GetHatCount());
        equipData.backId = ClampOptionalId(equipData.backId, GetBackCount());
        equipData.accessoryId = ClampOptionalId(equipData.accessoryId, GetAccessoryCount());
        equipData.colorId = ClampRequiredId(equipData.colorId, PlayerCustomizationPalette.BodyColorCount);

        return equipData;
    }

    private void Custom_Render()
    {
        ApplyCustomization(GetNetworkCustomizationData(), false);
    }

    [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetCustomization(
        int bodyId,
        int faceId,
        int hatId,
        int backId,
        int accessoryId,
        int colorId)
    {
        NetBodyId = ClampRequiredId(bodyId, GetBodyCount());
        NetFaceId = ClampOptionalId(faceId, GetFaceCount());
        NetHatId = ClampOptionalId(hatId, GetHatCount());
        NetBackId = ClampOptionalId(backId, GetBackCount());
        NetAccessoryId = ClampOptionalId(accessoryId, GetAccessoryCount());
        NetColorId = ClampRequiredId(colorId, PlayerCustomizationPalette.BodyColorCount);
    }

    private EquipData GetNetworkCustomizationData()
    {
        return new EquipData
        {
            bodyId = NetBodyId,
            faceId = NetFaceId,
            hatId = NetHatId,
            backId = NetBackId,
            accessoryId = NetAccessoryId,
            colorId = NetColorId
        };
    }

    private void CacheCustomizationRefs()
    {
        if (customBodyRenderer == null)
        {
            customBodyRenderer = visualRoot != null
                ? visualRoot.GetComponentInChildren<MeshRenderer>(true)
                : GetComponentInChildren<MeshRenderer>(true);
        }

        if (customFaceDecal == null)
            customFaceDecal = GetComponentInChildren<RuntimeDecalCollider>(true);

        if (customFaceDecal != null && customFaceRenderer == null)
            customFaceRenderer = customFaceDecal.GetComponent<Renderer>();

        if (customFaceRenderer == null && visualRoot != null)
        {
            Transform faceRoot = FindChildRecursive_Custom(visualRoot, "Face");

            if (faceRoot != null)
                customFaceRenderer = faceRoot.GetComponentInChildren<Renderer>(true);
        }
    }

    private void ConfigureFaceDecal()
    {
        if (customFaceDecal == null)
            return;

        customFaceDecal.decalMode = DecalMode.GridProjection;
        customFaceDecal.projectionSpace = ProjectionSpace.Local;
        customFaceDecal.projectionDirection = ProjectionDirection.Back;
        customFaceDecal.size = Vector2.one;
        customFaceDecal.raycastGridExtent = Vector2.one;
        customFaceDecal.maxDistance = 1f;
        customFaceDecal.meshSubdivisions = 32;
        customFaceDecal.colliderSubdivisions = 8;
        customFaceDecal.updateColliderOnLive = false;
        customFaceDecal.cullIfInvisible = false;

        // 매 프레임 자동 재생성 금지: 플레이어는 항상 움직이므로 alwaysRebuild가 켜져 있으면
        // 프레임마다 레이캐스트 투영 + Mesh 생성/파괴가 반복되어 메모리가 계속 불어난다.
        // 표정 변경 시에는 ApplyCustomFace에서 명시적으로 ForceRebuild를 호출한다.
        customFaceDecal.alwaysRebuild = false;
    }

    private void ApplyCustomization(EquipData equipData, bool force)
    {
        equipData.bodyId = ClampRequiredId(equipData.bodyId, GetBodyCount());
        equipData.faceId = ClampOptionalId(equipData.faceId, GetFaceCount());
        equipData.hatId = ClampOptionalId(equipData.hatId, GetHatCount());
        equipData.backId = ClampOptionalId(equipData.backId, GetBackCount());
        equipData.accessoryId = ClampOptionalId(equipData.accessoryId, GetAccessoryCount());
        equipData.colorId = ClampRequiredId(equipData.colorId, PlayerCustomizationPalette.BodyColorCount);

        if (!force && hasAppliedCustomData && IsSameCustomization(equipData, lastAppliedCustomData))
            return;

        ApplyCustomBody(equipData.bodyId, equipData.colorId);
        ApplyCustomFace(equipData.faceId);
        ApplySingleActive(customHatParts, equipData.hatId);
        ApplySingleActive(customBackParts, equipData.backId);
        ApplySingleActive(customAccessoryParts, equipData.accessoryId);

        lastAppliedCustomData = equipData;
        hasAppliedCustomData = true;
    }

    private void ApplyCustomBody(int bodyId, int colorId)
    {
        if (customBodyRenderer == null)
            return;

        if (customBodyMaterials != null && customBodyMaterials.Length > 0)
        {
        int index = Mathf.Clamp(bodyId - 1, 0, customBodyMaterials.Length - 1);
            Material material = customBodyMaterials[index];

            if (material != null)
                customBodyRenderer.sharedMaterial = material;
        }

        if (customBodyBlock == null)
            customBodyBlock = new MaterialPropertyBlock();

        Color color = PlayerCustomizationPalette.GetBodyColor(colorId);

        customBodyRenderer.GetPropertyBlock(customBodyBlock);
        customBodyBlock.SetColor(BaseColorId, color);
        customBodyBlock.SetColor(ColorPropertyId, color);
        customBodyRenderer.SetPropertyBlock(customBodyBlock);
    }

    private void ApplyCustomFace(int faceId)
    {
        CacheCustomizationRefs();

        if (faceId == NoneId)
        {
            SetFaceVisible(false);
            return;
        }

        if (customFaceRenderer == null || customFaceMaterials == null || customFaceMaterials.Length == 0)
        {
            SetFaceVisible(false);
            return;
        }

        int index = Mathf.Clamp(faceId - 1, 0, customFaceMaterials.Length - 1);
        Material material = customFaceMaterials[index];

        if (material == null)
        {
            SetFaceVisible(false);
            return;
        }

        SetFaceVisible(true);

        customFaceRenderer.sharedMaterial = material;

        ConfigureFaceDecal();

        if (customFaceDecal != null)
            customFaceDecal.ForceRebuild(Application.isPlaying == false);
    }

    private void SetFaceVisible(bool visible)
    {
        if (customFaceRenderer != null)
            customFaceRenderer.enabled = visible;

        if (customFaceDecal != null)
            customFaceDecal.enabled = visible;
    }

    private static void ApplySingleActive(Transform[] parts, int activeId)
    {
        if (parts == null || parts.Length == 0)
            return;

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == null)
                continue;

            parts[i].gameObject.SetActive(activeId != NoneId && i == activeId - 1);
        }
    }

    private bool hasValidatedCustomizationCounts;

    /// <summary>
    /// Logs (once) when the equip resource array lengths do not match the
    /// Table_Costomize item counts, so table/resource drift is caught early.
    /// Equipping is still defended by ClampOptionalId/ClampRequiredId.
    /// </summary>
    private void ValidateCustomizationCounts()
    {
        if (hasValidatedCustomizationCounts)
            return;

        hasValidatedCustomizationCounts = true;

        if (TableMgr.Inst == null)
            return;

        WarnIfCountMismatch(CostumePartType.Face, GetFaceCount(), nameof(customFaceMaterials));
        WarnIfCountMismatch(CostumePartType.Hat, GetHatCount(), nameof(customHatParts));
        WarnIfCountMismatch(CostumePartType.Back, GetBackCount(), nameof(customBackParts));
        WarnIfCountMismatch(CostumePartType.Accessory, GetAccessoryCount(), nameof(customAccessoryParts));
    }

    private static void WarnIfCountMismatch(CostumePartType partType, int resourceCount, string fieldName)
    {
        int tableCount = TableMgr.Inst.GetCostumeItems(partType).Count;
        if (tableCount != resourceCount)
        {
            Debug.LogError(
                $"[PlayerCtrl] Customization count mismatch for {partType}: " +
                $"Table_Costomize={tableCount}, {fieldName}={resourceCount}");
        }
    }

    private int GetBodyCount()
    {
        return customBodyMaterials != null ? customBodyMaterials.Length : 0;
    }

    private int GetFaceCount()
    {
        return customFaceMaterials != null ? customFaceMaterials.Length : 0;
    }

    private int GetHatCount()
    {
        return customHatParts != null ? customHatParts.Length : 0;
    }

    private int GetBackCount()
    {
        return customBackParts != null ? customBackParts.Length : 0;
    }

    private int GetAccessoryCount()
    {
        return customAccessoryParts != null ? customAccessoryParts.Length : 0;
    }

    private static int ClampRequiredId(int value, int count)
    {
        if (count <= 0)
            return NoneId;

        return Mathf.Clamp(value, 1, count);
    }

    private static int ClampOptionalId(int value, int count)
    {
        if (value == NoneId)
            return NoneId;

        if (count <= 0)
            return NoneId;

        return Mathf.Clamp(value, 1, count);
    }

    private static bool IsSameCustomization(EquipData a, EquipData b)
    {
        return a.bodyId == b.bodyId
            && a.faceId == b.faceId
            && a.hatId == b.hatId
            && a.backId == b.backId
            && a.accessoryId == b.accessoryId
            && a.colorId == b.colorId;
    }

    private static Transform FindChildRecursive_Custom(Transform parent, string targetName)
    {
        if (parent == null || string.IsNullOrEmpty(targetName))
            return null;

        if (parent.name == targetName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive_Custom(parent.GetChild(i), targetName);

            if (found != null)
                return found;
        }

        return null;
    }
}
