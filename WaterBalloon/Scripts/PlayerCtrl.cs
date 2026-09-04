using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IObstacleKnockbackReceiver
{
    void ReceiveKnockback(Vector3 velocity);
}

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Rigidbody))]
public partial class PlayerCtrl : NetworkBehaviour, IObstacleKnockbackReceiver
{
    private long steamId;
    public long SteamID => steamId;

    public static PlayerCtrl Local { get; private set; }

    [Networked] public NetworkString<_32> PlayerName { get; set; }

    // 닉네임 RPC가 Spawned 시점에 누락될 수 있어(특히 게스트), 네트워크에 반영될 때까지 재전송.
    private bool nicknameSynced;
    private float nicknameResendTimer;

    [Header("References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform decalRoot;

    [Header("Occlusion Outline")]
    [SerializeField] private bool enableOcclusionOutline = true;
    [SerializeField] private bool enableNearbyObstacleOcclusionOutline = true;
    [SerializeField] private float nearbyObstacleOutlineRadius = 7.5f;
    [SerializeField] private float nearbyObstacleOutlineUpdateInterval = 0.15f;
    [SerializeField] private float nearbyObstacleForegroundSkipDistance = 0.35f;
    [SerializeField] private LayerMask nearbyObstacleOutlineMask = ~0;
    [SerializeField] private LayerMask playerOcclusionOutlineOccluderMask = ~0;
    [SerializeField] private float playerOcclusionOutlineProbeRadius = 0.08f;
    [SerializeField] private float playerOcclusionOutlineGroundNormalThreshold = 0.55f;
    [SerializeField] private float playerOcclusionOutlineGroundHeightTolerance = 0.05f;
    private const uint OcclusionOutlineRenderingLayer = 1u << 7;
    private const uint NearbyObstacleOutlineRenderingLayer = 1u << 6;
    private const uint ReservedOutlineRenderingLayers = OcclusionOutlineRenderingLayer | NearbyObstacleOutlineRenderingLayer;
    private const string PlatformTag = "Platform";
    private const string OutlineIgnoreTag = "NoOutline";
    private readonly RaycastHit[] playerOcclusionOutlineHits = new RaycastHit[16];
    private readonly Dictionary<Renderer, uint> playerOcclusionOutlineOriginalMasks = new Dictionary<Renderer, uint>();
    private readonly List<Renderer> playerOcclusionOutlineRenderers = new List<Renderer>();
    private readonly Collider[] nearbyObstacleOutlineHits = new Collider[256];
    private readonly Dictionary<Renderer, uint> nearbyObstacleOutlineOriginalMasks = new Dictionary<Renderer, uint>();
    private readonly HashSet<Renderer> nearbyObstacleOutlineActiveRenderers = new HashSet<Renderer>();
    private readonly List<Renderer> reusableNearbyObstacleOutlineRenderers = new List<Renderer>();
    private readonly List<Renderer> nearbyNoOutlineRenderers = new List<Renderer>();
    // GetComponentsInChildren 비할당 오버로드용 재사용 리스트
    private readonly List<Renderer> nearbyObstacleOutlineTempRenderers = new List<Renderer>();
    private float nextNearbyObstacleOutlineUpdateTime;

    [Header("Move")]
    [SerializeField] private float moveForce = 42f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float airControl = 0.35f;
    [SerializeField] private float brakeForce = 3.0f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpCooldown = 0.35f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float coyoteTime = 0.12f;
    private float jumpInputLockAfterPad = 0.15f;

    // Obstacle move modifiers — keyed by obstacle instance (local only, not networked)
    // accelerationMultiplier → moveForce (ApplyMovement)
    // moveSpeedMultiplier    → maxSpeed  (LimitHorizontalSpeed)
    // jumpPowerMultiplier    → jumpForce (ApplyJump)
    private readonly Dictionary<GameObject, ObstacleMoveModifier> obstacleMoveModifiers = new Dictionary<GameObject, ObstacleMoveModifier>();
    private float currentAccelerationMultiplier = 1f;
    private float currentMoveSpeedMultiplier = 1f;
    private float currentJumpPowerMultiplier = 1f;

    [Header("Control")]
    private bool IsKinematic
    {
        get
        {
            if (rb == null)
                return true;
            return rb.isKinematic;
        }

        set
        {
            if (rb == null)
                return;
            rb.isKinematic = value;
        }
    }

    private Vector3 LinearVelocity
    {
        get
        {
            if (rb == null)
                return Vector3.zero;
            return rb.linearVelocity;
        }
        set
        {
            if (rb == null || IsKinematic)
                return;
            rb.linearVelocity = value;
        }
    }

    [Networked] public NetworkBool IsInputEnabled { get; private set; }

    [SerializeField] private float inputSmooth = 14f;
    [SerializeField] private float turnTorque = 7f;
    [SerializeField] private float highSpeedControlRate = 0.7f;

    [Tooltip("렉처럼 보일 수 있으므로 처음에는 0 권장. 나중에 0.1~0.35 정도만 사용.")]
    [SerializeField] private float randomWobble = 0f;

    [Header("Fake Player Push")]
    [SerializeField] private float playerPushForce = 16f;
    [SerializeField] private float playerPushUpForce = 0.25f;
    [SerializeField] private float playerPushMaxSpeed = 10f;
    [SerializeField] private float playerPushCooldown = 0.03f;

    [Header("Manual Network Sync")]
    [SerializeField] private float remoteLerpSpeed = 18f;
    [SerializeField] private float remoteRotationLerpSpeed = 20f;
    [SerializeField] private float remoteSnapDistance = 4f;
    [SerializeField] private float syncInterval = 0.033f;

    [Networked] private Vector3 NetPosition { get; set; }
    [Networked] private Quaternion NetRotation { get; set; }
    [Networked] private Vector3 NetVelocity { get; set; }

    [Header("Ground Check")]
    private const float GroundContactNormalThreshold = 0.35f;

    [SerializeField] private float groundCheckDistance = 0.12f;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private float groundCheckOriginOffset = 0.03f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Camera")]
    [SerializeField] private Transform cameraTarget;

    [Header("Visual Squash")]
    [SerializeField] private float squashAmount = 0.14f;
    [SerializeField] private float squashLerpSpeed = 14f;
    [SerializeField] private float jumpSquashTime = 0.07f;

    [SerializeField] private bool lockCursor = true;

    private Rigidbody rb;
    private Camera mainCam;

    private Vector2 rawInput;
    private Vector2 smoothInput;

    private bool jumpPressed;
    private bool isGrounded;
    private bool jumpedThisFixedUpdate;
    private bool initialized;
    private readonly RaycastHit[] groundHits = new RaycastHit[8];
    private Collider[] ownColliders;
    private ObstaclePlatform currentMovingPlatform;
    private Vector3 lastMovingPlatformPosition;

    private float lastJumpTime = -999f;
    private float lastJumpPressedTime = -999f;
    private float lastGroundedTime = -999f;
    private float lastJumpPadLockTime = -999f;
    private float lastSyncTime = -999f;
    private float lastPlayerPushTime = -999f;

    private bool localInputBlockedByChat;

    private Vector3 defaultVisualScale;
    private Vector3 targetVisualScale;

    public Transform GetCameraTarget()
    {
        return cameraTarget != null ? cameraTarget : transform;
    }

    public bool HasLocalPlayerAuthority =>
        Object != null &&
        Object.IsValid &&
        (Runner == null || Runner.GameMode != GameMode.Shared
            ? Object.HasInputAuthority
            : Object.HasStateAuthority);

    public PlayerRef OwnerPlayerRef
    {
        get
        {
            if (Object == null || !Object.IsValid)
                return PlayerRef.None;

            return Runner != null && Runner.GameMode == GameMode.Shared
                ? Object.StateAuthority
                : Object.InputAuthority;
        }
    }

    private void CacheOcclusionOutlineRenderers()
    {
        playerOcclusionOutlineOriginalMasks.Clear();
        playerOcclusionOutlineRenderers.Clear();

        if (!HasLocalPlayerAuthority)
        {
            StripOwnReservedOutlineRenderingLayers();
            return;
        }

        if (visualRoot == null)
            return;

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null)
                continue;

            playerOcclusionOutlineRenderers.Add(targetRenderer);
            playerOcclusionOutlineOriginalMasks[targetRenderer] = targetRenderer.renderingLayerMask & ~ReservedOutlineRenderingLayers;
        }

        SetOcclusionOutlineRenderingLayer(false);
    }

    private void UpdatePlayerOcclusionOutline()
    {
        if (!HasLocalPlayerAuthority)
        {
            SetOcclusionOutlineRenderingLayer(false);
            return;
        }

        if (!enableOcclusionOutline)
        {
            SetOcclusionOutlineRenderingLayer(false);
            return;
        }

        SetOcclusionOutlineRenderingLayer(true);
    }

    private void SetOcclusionOutlineRenderingLayer(bool active)
    {
        if (active && !HasLocalPlayerAuthority)
            active = false;

        for (int i = 0; i < playerOcclusionOutlineRenderers.Count; i++)
        {
            Renderer targetRenderer = playerOcclusionOutlineRenderers[i];
            if (targetRenderer == null)
                continue;

            uint originalMask = playerOcclusionOutlineOriginalMasks.TryGetValue(targetRenderer, out uint cachedMask)
                ? cachedMask
                : targetRenderer.renderingLayerMask & ~ReservedOutlineRenderingLayers;

            targetRenderer.renderingLayerMask = active
                ? originalMask | OcclusionOutlineRenderingLayer
                : originalMask;
        }
    }

    private void StripOwnReservedOutlineRenderingLayers()
    {
        Transform targetRoot = visualRoot != null ? visualRoot : transform;
        Renderer[] renderers = targetRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null)
                continue;

            targetRenderer.renderingLayerMask &= ~ReservedOutlineRenderingLayers;
        }
    }

    private bool IsLocalPlayerForegroundOccluded()
    {
        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam == null)
            return false;

        Bounds visualBounds = GetPlayerVisualBounds();
        Vector3 cameraPosition = mainCam.transform.position;
        Vector3 targetPosition = GetCameraTarget().position;
        return HasForegroundOccluderBetween(cameraPosition, targetPosition, visualBounds)
            || HasForegroundOccluderBetween(cameraPosition, visualBounds.center, visualBounds);
    }

    private Bounds GetPlayerVisualBounds()
    {
        Bounds bounds = default;
        bool hasBounds = false;
        for (int i = 0; i < playerOcclusionOutlineRenderers.Count; i++)
        {
            Renderer targetRenderer = playerOcclusionOutlineRenderers[i];
            if (targetRenderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = targetRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(targetRenderer.bounds);
            }
        }

        return hasBounds ? bounds : new Bounds(transform.position, Vector3.one * 0.1f);
    }

    private bool HasForegroundOccluderBetween(Vector3 origin, Vector3 target, Bounds visualBounds)
    {
        Vector3 direction = target - origin;
        float distance = direction.magnitude;
        if (distance <= 0.001f)
            return false;

        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            Mathf.Max(0.001f, playerOcclusionOutlineProbeRadius),
            direction / distance,
            playerOcclusionOutlineHits,
            distance,
            playerOcclusionOutlineOccluderMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = playerOcclusionOutlineHits[i].collider;
            if (hitCollider == null)
                continue;

            if (hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
                continue;

            if (ShouldIgnoreGroundLikeOccluderHit(playerOcclusionOutlineHits[i], visualBounds))
                continue;

            return true;
        }

        return false;
    }

    private bool ShouldIgnoreGroundLikeOccluderHit(RaycastHit hit, Bounds visualBounds)
    {
        if (hit.normal.y < playerOcclusionOutlineGroundNormalThreshold)
            return false;

        Collider hitCollider = hit.collider;
        if (hitCollider == null)
            return true;

        if (hitCollider.GetComponentInParent<ObstacleBase>() != null)
            return false;

        Transform current = hitCollider.transform;
        while (current != null)
        {
            if (current.CompareTag(PlatformTag))
                return false;

            current = current.parent;
        }

        return hit.point.y <= visualBounds.min.y + playerOcclusionOutlineGroundHeightTolerance;
    }

    private void UpdateNearbyObstacleOcclusionOutline()
    {
        if (!enableNearbyObstacleOcclusionOutline)
        {
            ClearNearbyObstacleOcclusionOutline();
            return;
        }

        if (Time.time < nextNearbyObstacleOutlineUpdateTime)
            return;

        nextNearbyObstacleOutlineUpdateTime = Time.time + Mathf.Max(0.02f, nearbyObstacleOutlineUpdateInterval);
        nearbyObstacleOutlineActiveRenderers.Clear();
        nearbyNoOutlineRenderers.Clear();

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            nearbyObstacleOutlineRadius,
            nearbyObstacleOutlineHits,
            nearbyObstacleOutlineMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = nearbyObstacleOutlineHits[i];
            if (hit == null)
                continue;

            ClearNoOutlineRenderingLayerBits(hit.transform);

            if (!TryGetNearbyOutlineRoot(hit, out Transform outlineRoot))
                continue;

            AddNearbyObstacleOutlineRenderers(outlineRoot);
        }

        RemoveStaleNearbyObstacleOutlineRenderers();
    }

    private void ClearNoOutlineRenderingLayerBits(Transform hitRoot)
    {
        if (hitRoot == null)
            return;

        Transform current = hitRoot;
        while (current != null && current.parent != null)
        {
            if (current.CompareTag(OutlineIgnoreTag))
            {
                ClearOutlineBitsFromTaggedRenderers(current);
                return;
            }

            current = current.parent;
        }
    }

    private void ClearOutlineBitsFromTaggedRenderers(Transform noOutlineRoot)
    {
        nearbyNoOutlineRenderers.Clear();
        noOutlineRoot.GetComponentsInChildren(true, nearbyNoOutlineRenderers);

        for (int i = 0; i < nearbyNoOutlineRenderers.Count; i++)
        {
            Renderer targetRenderer = nearbyNoOutlineRenderers[i];
            if (targetRenderer == null)
                continue;

            targetRenderer.renderingLayerMask &= ~ReservedOutlineRenderingLayers;
            nearbyObstacleOutlineOriginalMasks.Remove(targetRenderer);
            nearbyObstacleOutlineActiveRenderers.Remove(targetRenderer);
        }
    }

    private static bool TryGetNearbyOutlineRoot(Collider hit, out Transform outlineRoot)
    {
        outlineRoot = null;

        if (HasTagInParents(hit.transform, OutlineIgnoreTag))
            return false;

        ObstacleBase obstacle = hit.GetComponentInParent<ObstacleBase>();
        if (obstacle != null)
        {
            outlineRoot = obstacle.transform;
            return true;
        }

        Transform current = hit.transform;
        while (current != null)
        {
            if (current.CompareTag(PlatformTag))
            {
                outlineRoot = current;
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool HasTagInParents(Transform target, string tagName)
    {
        Transform current = target;
        while (current != null)
        {
            if (current.CompareTag(tagName))
                return true;

            current = current.parent;
        }

        return false;
    }

    private void AddNearbyObstacleOutlineRenderers(Transform targetRoot)
    {
        if (HasTagInParents(targetRoot, OutlineIgnoreTag))
            return;

        // 비할당 오버로드 사용: 매 호출 Renderer[] 생성 없음
        nearbyObstacleOutlineTempRenderers.Clear();
        targetRoot.GetComponentsInChildren(true, nearbyObstacleOutlineTempRenderers);

        ClearNoOutlineRenderingLayerBits(nearbyObstacleOutlineTempRenderers);

        if (IsForegroundOccluderForLocalPlayer(targetRoot, nearbyObstacleOutlineTempRenderers))
            return;

        if (IsOccludedByOtherPlayerForLocalCamera(nearbyObstacleOutlineTempRenderers))
            return;

        for (int i = 0; i < nearbyObstacleOutlineTempRenderers.Count; i++)
        {
            Renderer targetRenderer = nearbyObstacleOutlineTempRenderers[i];
            if (targetRenderer == null)
                continue;

            if (HasTagInParents(targetRenderer.transform, OutlineIgnoreTag))
            {
                targetRenderer.renderingLayerMask &= ~ReservedOutlineRenderingLayers;
                continue;
            }

            nearbyObstacleOutlineActiveRenderers.Add(targetRenderer);

            if (!nearbyObstacleOutlineOriginalMasks.ContainsKey(targetRenderer))
                nearbyObstacleOutlineOriginalMasks.Add(targetRenderer, targetRenderer.renderingLayerMask & ~ReservedOutlineRenderingLayers);

            targetRenderer.renderingLayerMask = (targetRenderer.renderingLayerMask & ~ReservedOutlineRenderingLayers) | NearbyObstacleOutlineRenderingLayer;
        }
    }

    private void ClearNoOutlineRenderingLayerBits(List<Renderer> renderers)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null || !HasTagInParents(targetRenderer.transform, OutlineIgnoreTag))
                continue;

            targetRenderer.renderingLayerMask &= ~ReservedOutlineRenderingLayers;
            nearbyObstacleOutlineOriginalMasks.Remove(targetRenderer);
            nearbyObstacleOutlineActiveRenderers.Remove(targetRenderer);
        }
    }

    private static void ClearReservedOutlineRenderingLayersInLoadedScenes()
    {
        Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null)
                continue;

            targetRenderer.renderingLayerMask &= ~ReservedOutlineRenderingLayers;
        }
    }

    private bool IsForegroundOccluderForLocalPlayer(Transform targetRoot, List<Renderer> renderers)
    {
        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam == null || targetRoot == null || renderers == null || renderers.Count == 0)
            return false;

        Bounds bounds = default;
        bool hasBounds = false;
        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = targetRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(targetRenderer.bounds);
            }
        }

        if (!hasBounds)
            return false;

        Transform camTransform = mainCam.transform;
        float playerDepth = Vector3.Dot(transform.position - camTransform.position, camTransform.forward);
        float targetDepth = Vector3.Dot(bounds.center - camTransform.position, camTransform.forward);

        if (targetDepth >= playerDepth - nearbyObstacleForegroundSkipDistance)
            return false;

        Vector3 cameraPosition = camTransform.position;
        Vector3 playerPosition = transform.position;
        Vector3 direction = playerPosition - cameraPosition;
        float distance = direction.magnitude;
        if (distance <= 0.001f)
            return false;

        int hitCount = Physics.SphereCastNonAlloc(
            cameraPosition,
            Mathf.Max(0.001f, playerOcclusionOutlineProbeRadius),
            direction / distance,
            playerOcclusionOutlineHits,
            distance,
            playerOcclusionOutlineOccluderMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = playerOcclusionOutlineHits[i].collider;
            if (hitCollider == null)
                continue;

            Transform hitTransform = hitCollider.transform;
            if (hitTransform == targetRoot || hitTransform.IsChildOf(targetRoot))
                return true;
        }

        return false;
    }

    private bool IsOccludedByOtherPlayerForLocalCamera(List<Renderer> renderers)
    {
        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam == null || renderers == null || renderers.Count == 0)
            return false;

        if (!TryGetRendererBounds(renderers, out Bounds bounds))
            return false;

        Vector3 cameraPosition = mainCam.transform.position;
        Vector3 targetPosition = bounds.center;
        Vector3 direction = targetPosition - cameraPosition;
        float distance = direction.magnitude;
        if (distance <= 0.001f)
            return false;

        int hitCount = Physics.SphereCastNonAlloc(
            cameraPosition,
            Mathf.Max(0.001f, playerOcclusionOutlineProbeRadius),
            direction / distance,
            playerOcclusionOutlineHits,
            distance,
            playerOcclusionOutlineOccluderMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = playerOcclusionOutlineHits[i].collider;
            if (hitCollider == null)
                continue;

            if (hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
                continue;

            PlayerCtrl player = hitCollider.GetComponentInParent<PlayerCtrl>();
            if (player != null && player != this)
                return true;
        }

        return false;
    }

    private static bool TryGetRendererBounds(List<Renderer> renderers, out Bounds bounds)
    {
        bounds = default;
        bool hasBounds = false;

        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = targetRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(targetRenderer.bounds);
            }
        }

        return hasBounds;
    }

    private void RemoveStaleNearbyObstacleOutlineRenderers()
    {
        if (nearbyObstacleOutlineOriginalMasks.Count == 0)
            return;

        reusableNearbyObstacleOutlineRenderers.Clear();
        foreach (KeyValuePair<Renderer, uint> pair in nearbyObstacleOutlineOriginalMasks)
        {
            Renderer targetRenderer = pair.Key;
            if (targetRenderer == null || !nearbyObstacleOutlineActiveRenderers.Contains(targetRenderer))
                reusableNearbyObstacleOutlineRenderers.Add(targetRenderer);
        }

        for (int i = 0; i < reusableNearbyObstacleOutlineRenderers.Count; i++)
            RestoreNearbyObstacleOutlineRenderer(reusableNearbyObstacleOutlineRenderers[i]);
    }

    private void RestoreNearbyObstacleOutlineRenderer(Renderer targetRenderer)
    {
        if (targetRenderer == null)
        {
            nearbyObstacleOutlineOriginalMasks.Remove(targetRenderer);
            return;
        }

        if (nearbyObstacleOutlineOriginalMasks.TryGetValue(targetRenderer, out uint originalMask))
            targetRenderer.renderingLayerMask = originalMask;

        nearbyObstacleOutlineOriginalMasks.Remove(targetRenderer);
    }

    private void ClearNearbyObstacleOcclusionOutline()
    {
        if (nearbyObstacleOutlineOriginalMasks.Count == 0)
            return;

        reusableNearbyObstacleOutlineRenderers.Clear();
        foreach (KeyValuePair<Renderer, uint> pair in nearbyObstacleOutlineOriginalMasks)
            reusableNearbyObstacleOutlineRenderers.Add(pair.Key);

        for (int i = 0; i < reusableNearbyObstacleOutlineRenderers.Count; i++)
            RestoreNearbyObstacleOutlineRenderer(reusableNearbyObstacleOutlineRenderers[i]);

        nearbyObstacleOutlineActiveRenderers.Clear();
    }

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        ownColliders = GetComponentsInChildren<Collider>();

        SetupRigidbody();

        if (visualRoot == null)
            visualRoot = transform;

        if (HasLocalPlayerAuthority)
            ClearReservedOutlineRenderingLayersInLoadedScenes();

        if (HasLocalPlayerAuthority)
            CacheOcclusionOutlineRenderers();
        else
            StripOwnReservedOutlineRenderingLayers();

        SeeThroughWall_Spawned();

        defaultVisualScale = visualRoot.localScale;
        targetVisualScale = defaultVisualScale;

        SetupHealthReferences();
        SetupJuiceReferences();
        Custom_Spawned();

        if (Object.HasStateAuthority)
        {
            IsInputEnabled = false;
            IsAlive = true;

            NetPosition = transform.position;
            NetRotation = transform.rotation;
            NetVelocity = Vector3.zero;
        }

        if (HasLocalPlayerAuthority)
        {
            Local = this;

            string nickname = GetLocalNickname();
            Rpc_SetNickname(nickname);

            CameraCtrl.Inst?.SetLocalPlayerTarget(GetCameraTarget());

            SetRigidbodyAsLocalControlled();

            // 본인 플레이어도 권위 스폰 위치로 스냅 (원점 자유낙사 → 1회 강제 사망 방지)
            if (!Object.HasStateAuthority)
                TeleportTo(NetPosition, NetRotation);

            PushNetworkMoveState(true);
        }
        else
        {
            SetRigidbodyAsRemoteProxy();
            ApplyRemoteNetworkState(true);
        }

        initialized = true;

        GameMgr.Inst?.RegisterPlayer(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        SeeThroughWall_Cleanup();
        SetOcclusionOutlineRenderingLayer(false);
        ClearNearbyObstacleOcclusionOutline();
        Juice_Cleanup();
        GameMgr.Inst?.UnregisterPlayer(this);

        if (Local == this)
            Local = null;

        base.Despawned(runner, hasState);
    }

    public override void Render()
    {
        base.Render();
        Custom_Render();
        Spectator_Render();

        if (!initialized || rb == null)
            return;

        if (HasLocalPlayerAuthority)
            return;

        ApplyRemoteNetworkState(false);
    }

    private void SetupRigidbody()
    {
        if (rb == null)
            return;

        rb.mass = 2f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearDamping = 0.25f;
        rb.angularDamping = 4f;
    }

    private void SetRigidbodyAsLocalControlled()
    {
        if (rb == null)
            return;

        IsKinematic = false;
        rb.detectCollisions = true;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void SetRigidbodyAsRemoteProxy()
    {
        if (rb == null)
            return;

        IsKinematic = true;
        rb.detectCollisions = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void Update()
    {
        if (!HasLocalPlayerAuthority)
            return;

        TrySyncNickname();
        TrySyncCustomization();

        UpdateNearbyObstacleOcclusionOutline();
        UpdatePlayerOcclusionOutline();
        SeeThroughWall_Update();

        if (!CanControl())
        {
            ResetRespawnHoldUI();
            return;
        }

        ReadInput();
        UpdateRespawnHold();
    }

    private void FixedUpdate()
    {
        if (!initialized || rb == null)
            return;

        if (!HasLocalPlayerAuthority)
            return;

        if (CanControl())
        {
            isGrounded = CheckGrounded();

            ApplyMovement();
            ApplyJump();
            ApplyMovingPlatformDelta();
            ApplyRandomWobble();
            LimitHorizontalSpeed();
        }
        else
        {
            rawInput = Vector2.zero;
            smoothInput = Vector2.zero;
            jumpPressed = false;
            ClearMovingPlatformReference();
        }

        // 직전 물리 스텝에서 보류된 낙하 충격사 처리(점프패드 착지면 이미 취소됨)
        ProcessPendingFallImpactPop();

        if (IsAlive)
            UpdateFallImpactTracking();

        PushNetworkMoveState(false);
    }

    private void LateUpdate()
    {
        if (!initialized || rb == null)
            return;

        if (!HasLocalPlayerAuthority)
            return;

        Juice_UpdateMaxSpeed();
        Juice_UpdateEventFX();

        if (!IsAlive)
            return;

        UpdateVisualSquash();
    }

    public void SetInputEnabled(bool enabled)
    {
        if (!Object.HasStateAuthority)
            return;

        IsInputEnabled = enabled;

        if (!enabled)
            ResetInputState();
    }

    public void SetLocalInputBlockedByChat(bool blocked)
    {
        localInputBlockedByChat = blocked;

        if (blocked)
            ResetInputState();
    }

    private void ResetInputState()
    {
        rawInput = Vector2.zero;
        smoothInput = Vector2.zero;
        jumpPressed = false;
        jumpedThisFixedUpdate = false;
        isGrounded = false;
        lastJumpTime = -999f;
        lastJumpPressedTime = -999f;
        lastGroundedTime = -999f;
        lastJumpPadLockTime = -999f;
        ClearMovingPlatformReference();

        ResetRespawnHoldStateOnly();

        if (HasLocalPlayerAuthority)
            InGameUI.Inst?.SetRespawnHoldUI(false, 0f, respawnHoldTime);
    }

    private bool CanControl()
    {
        if (!initialized)
            return false;

        if (Object == null)
            return false;

        if (!HasLocalPlayerAuthority)
            return false;

        if (InGameInputBlocker.IsBlocked)
            return false;

        if (localInputBlockedByChat)
            return false;

        if (!IsInputEnabled)
            return false;

        if (!IsAlive)
            return false;

        return true;
    }

    private void ReadInput()
    {
        if (Keyboard.current == null)
        {
            rawInput = Vector2.zero;
            jumpPressed = false;
            return;
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.aKey.isPressed)
            x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            x += 1f;

        if (Keyboard.current.sKey.isPressed)
            y -= 1f;

        if (Keyboard.current.wKey.isPressed)
            y += 1f;

        rawInput = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpPressed = true;
            lastJumpPressedTime = Time.time;
        }
    }

    private void ApplyMovement()
    {
        smoothInput = Vector2.Lerp(
            smoothInput,
            rawInput,
            inputSmooth * Time.fixedDeltaTime
        );

        Vector3 horizontalVelocity = new Vector3(
            LinearVelocity.x,
            0f,
            LinearVelocity.z
        );

        if (smoothInput.sqrMagnitude < 0.04f)
        {
            ApplyBrake(horizontalVelocity);
            return;
        }

        Vector3 moveDir = GetCameraRelativeMoveDir(smoothInput);

        if (moveDir.sqrMagnitude < 0.001f)
            return;

        float speedRate = Mathf.Clamp01(horizontalVelocity.magnitude / maxSpeed);

        float controlByGround = isGrounded ? 1f : airControl;
        float controlBySpeed = Mathf.Lerp(1f, highSpeedControlRate, speedRate);

        rb.AddForce(
            moveDir * moveForce * currentAccelerationMultiplier * controlByGround * controlBySpeed,
            ForceMode.Acceleration
        );

        Vector3 torqueAxis = Vector3.Cross(Vector3.up, moveDir);

        rb.AddTorque(
            torqueAxis * turnTorque,
            ForceMode.Acceleration
        );
    }

    private void ApplyBrake(Vector3 horizontalVelocity)
    {
        if (!isGrounded)
            return;

        if (horizontalVelocity.sqrMagnitude < 0.01f)
            return;

        float decel = brakeForce * 4f * Time.fixedDeltaTime;
        Vector3 braked = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, decel);
        LinearVelocity = new Vector3(braked.x, LinearVelocity.y, braked.z);
    }

    private void TeleportTo(Vector3 position, Quaternion rotation)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.position = position;
            rb.rotation = rotation;

            transform.SetPositionAndRotation(position, rotation);

            rb.Sleep();
            rb.WakeUp();
        }
        else
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        if (HasLocalPlayerAuthority)
            PushNetworkMoveState(true);

        Physics.SyncTransforms();
    }

    private Vector3 GetCameraRelativeMoveDir(Vector2 moveInput)
    {
        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam == null)
            return new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        Vector3 forward = mainCam.transform.forward;
        Vector3 right = mainCam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 dir = forward * moveInput.y + right * moveInput.x;

        if (dir.sqrMagnitude < 0.001f)
            return Vector3.zero;

        return dir.normalized;
    }

    private void ApplyJump()
    {
        jumpedThisFixedUpdate = false;

        if (!HasBufferedJump())
            return;

        if (Time.time < lastJumpPadLockTime + jumpInputLockAfterPad)
        {
            ClearBufferedJump();
            return;
        }

        if (!CanUseGroundedJump())
            return;

        if (Time.time < lastJumpTime + jumpCooldown)
            return;

        ConsumeBufferedJump();
        lastJumpTime = Time.time;
        jumpedThisFixedUpdate = true;

        SteamAchievements.AddStat(SteamAchievements.STAT_JUMPS_TOTAL, 1);

        Vector3 velocity = LinearVelocity;

        if (velocity.y < 0f)
            velocity.y = 0f;

        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontal.magnitude > maxSpeed)
            horizontal = horizontal.normalized * maxSpeed;

        LinearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);

        rb.AddForce(Vector3.up * jumpForce * currentJumpPowerMultiplier, ForceMode.Impulse);

        rb.AddTorque(
            Random.insideUnitSphere * turnTorque * 0.5f,
            ForceMode.Impulse
        );
            
        SoundMgr.Inst.PlaySfx3D(Consts.eSfx.Player_Bounce_Small, transform.position);
        PlayJumpSquash();
        //Juice_PlayJump();
    }

    private bool HasBufferedJump()
    {
        if (!jumpPressed)
            return false;

        bool buffered = Time.time <= lastJumpPressedTime + jumpBufferTime;

        if (!buffered)
            ClearBufferedJump();

        return buffered;
    }

    private bool CanUseGroundedJump()
    {
        return isGrounded || Time.time <= lastGroundedTime + coyoteTime;
    }

    private void ConsumeBufferedJump()
    {
        ClearBufferedJump();
        lastGroundedTime = -999f;
    }

    private void ClearBufferedJump()
    {
        jumpPressed = false;
        lastJumpPressedTime = -999f;
    }

    private void ApplyRandomWobble()
    {
        if (randomWobble <= 0f)
            return;

        if (!isGrounded)
            return;

        Vector3 horizontalVelocity = new Vector3(
            LinearVelocity.x,
            0f,
            LinearVelocity.z
        );

        float speed = horizontalVelocity.magnitude;

        if (speed < 1.5f)
            return;

        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        );

        if (randomDir.sqrMagnitude < 0.001f)
            return;

        randomDir.Normalize();

        rb.AddForce(
            randomDir * randomWobble * speed,
            ForceMode.Acceleration
        );
    }

    private void LimitHorizontalSpeed()
    {
        Vector3 horizontalVelocity = new Vector3(
            LinearVelocity.x,
            0f,
            LinearVelocity.z
        );

        float effectiveMaxSpeed = maxSpeed * currentMoveSpeedMultiplier;

        if (horizontalVelocity.magnitude <= effectiveMaxSpeed)
            return;

        horizontalVelocity = horizontalVelocity.normalized * effectiveMaxSpeed;

        LinearVelocity = new Vector3(
            horizontalVelocity.x,
            LinearVelocity.y,
            horizontalVelocity.z
        );
    }

    private void PushNetworkMoveState(bool force)
    {
        if (rb == null)
            return;

        if (!force && Time.time < lastSyncTime + syncInterval)
            return;

        lastSyncTime = Time.time;

        Vector3 pos = rb.position;
        Quaternion rot = rb.rotation;
        Vector3 vel = LinearVelocity;
        Vector3 angVel = rb.angularVelocity;

        if (Object.HasStateAuthority)
        {
            NetPosition = pos;
            NetRotation = rot;
            NetVelocity = vel;
            return;
        }

        RPC_SendMoveState(pos, rot, vel, angVel);
    }

    [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    private void RPC_SendMoveState(
        Vector3 position,
        Quaternion rotation,
        Vector3 velocity,
        Vector3 angularVelocity)
    {
        NetPosition = position;
        NetRotation = rotation;
        NetVelocity = velocity;
    }

    private void ApplyRemoteNetworkState(bool snap)
    {
        Vector3 targetPosition = NetPosition;
        Quaternion targetRotation = NetRotation;

        if (targetRotation == default)
            targetRotation = transform.rotation;

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (snap || distance >= remoteSnapDistance)
        {
            transform.SetPositionAndRotation(targetPosition, targetRotation);

            if (rb != null)
            {
                rb.position = targetPosition;
                rb.rotation = targetRotation;
            }

            return;
        }

        Vector3 predictedPosition = targetPosition + NetVelocity * Time.deltaTime;

        Vector3 lerpedPosition = Vector3.Lerp(
            transform.position,
            predictedPosition,
            Time.deltaTime * remoteLerpSpeed
        );

        Quaternion lerpedRotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * remoteRotationLerpSpeed
        );

        transform.SetPositionAndRotation(lerpedPosition, lerpedRotation);

        if (rb != null)
        {
            rb.position = lerpedPosition;
            rb.rotation = lerpedRotation;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        TryApplyFakePlayerPush(collision);
        OnCollisionStay_Health(collision);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryApplyFakePlayerPush(collision);
        OnCollisionEnter_Health(collision);
    }

    private void TryApplyFakePlayerPush(Collision collision)
    {
        if (!initialized)
            return;

        if (rb == null)
            return;

        if (!HasLocalPlayerAuthority)
            return;

        if (!IsAlive)
            return;

        if (Time.time < lastPlayerPushTime + playerPushCooldown)
            return;

        PlayerCtrl other = collision.collider.GetComponentInParent<PlayerCtrl>();

        if (other == null || other == this)
            return;

        if (!other.IsNetworkReadyForCollision())
            return;

        Vector3 pushDir = other.transform.position - transform.position;
        pushDir.y = 0f;

        if (pushDir.sqrMagnitude < 0.0001f && collision.contactCount > 0)
        {
            Vector3 normal = collision.GetContact(0).normal;
            pushDir = new Vector3(-normal.x, 0f, -normal.z);
        }

        if (pushDir.sqrMagnitude < 0.0001f)
            return;

        pushDir.Normalize();

        Vector3 velocity = pushDir * playerPushForce;
        velocity.y = playerPushUpForce;

        other.RPC_ReceivePlayerPush(velocity);

        lastPlayerPushTime = Time.time;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ReceivePlayerPush(Vector3 velocity)
    {
        if (!HasLocalPlayerAuthority)
            return;

        if (!IsAlive)
            return;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null || rb.isKinematic)
            return;

        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        if (horizontalVelocity.magnitude >= playerPushMaxSpeed)
            return;

        velocity.y = Mathf.Clamp(velocity.y, 0f, 0.1f);
        velocity = Vector3.ClampMagnitude(velocity, playerPushForce);

        rb.AddForce(velocity, ForceMode.VelocityChange);

        PushNetworkMoveState(true);
    }

    private bool IsNetworkReadyForCollision()
    {
        return Object != null && Object.IsValid;
    }

    private bool CheckGrounded()
    {
        Vector3 origin = GetGroundCheckOrigin();
        float radius = GetGroundCheckCastRadius();
        float distance = GetGroundCheckCastDistance();

        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            radius,
            Vector3.down,
            groundHits,
            distance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        bool grounded = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = groundHits[i];

            if (hit.collider == null)
                continue;

            if (IsOwnCollider(hit.collider))
                continue;

            if (hit.normal.y < GroundContactNormalThreshold)
                continue;

            grounded = true;
            UpdateMovingPlatformReference(hit, true);
            break;
        }

        if (!grounded)
            ClearMovingPlatformReference();

        if (grounded)
            lastGroundedTime = Time.time;

        return grounded;
    }

    private void UpdateMovingPlatformReference(RaycastHit hit, bool grounded)
    {
        if (!grounded || hit.normal.y < 0.5f)
        {
            ClearMovingPlatformReference();
            return;
        }

        ObstaclePlatform platform = hit.collider.GetComponentInParent<ObstaclePlatform>();

        if (platform == null)
        {
            ClearMovingPlatformReference();
            return;
        }

        if (currentMovingPlatform == platform)
            return;

        currentMovingPlatform = platform;
        lastMovingPlatformPosition = platform.transform.position;
    }

    private void ApplyMovingPlatformDelta()
    {
        Vector3 platformDelta = GetMovingPlatformDelta();

        if (platformDelta.sqrMagnitude <= 0.0000001f)
            return;

        Vector3 targetPosition = rb.position + platformDelta;
        rb.MovePosition(targetPosition);
        transform.position = targetPosition;
    }

    private Vector3 GetMovingPlatformDelta()
    {
        if (currentMovingPlatform == null)
            return Vector3.zero;

        if (!isGrounded || jumpedThisFixedUpdate)
        {
            ClearMovingPlatformReference();
            return Vector3.zero;
        }

        Vector3 currentPlatformPosition = currentMovingPlatform.transform.position;
        Vector3 delta = currentPlatformPosition - lastMovingPlatformPosition;

        lastMovingPlatformPosition = currentPlatformPosition;

        return delta;
    }

    private void ClearMovingPlatformReference()
    {
        currentMovingPlatform = null;
        lastMovingPlatformPosition = Vector3.zero;
    }

    private Vector3 GetGroundCheckOrigin()
    {
        if (TryGetOwnColliderBounds(out Bounds bounds))
        {
            float radius = GetGroundCheckCastRadius();

            return new Vector3(
                bounds.center.x,
                bounds.min.y + radius + groundCheckOriginOffset,
                bounds.center.z
            );
        }

        return transform.position + Vector3.up * groundCheckOriginOffset;
    }

    private float GetGroundCheckCastRadius()
    {
        float radius = Mathf.Max(0.01f, groundCheckRadius);

        if (!TryGetOwnColliderBounds(out Bounds bounds))
            return radius;

        float footprintRadius = Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.8f;
        return Mathf.Clamp(radius, 0.01f, Mathf.Max(0.01f, footprintRadius));
    }

    private float GetGroundCheckCastDistance()
    {
        return Mathf.Max(0f, groundCheckDistance);
    }

    private bool TryGetOwnColliderBounds(out Bounds bounds)
    {
        if (ownColliders == null || ownColliders.Length == 0)
            ownColliders = GetComponentsInChildren<Collider>();

        bounds = default;
        bool hasBounds = false;

        for (int i = 0; i < ownColliders.Length; i++)
        {
            Collider ownCollider = ownColliders[i];

            if (ownCollider == null || ownCollider.isTrigger)
                continue;

            if (!hasBounds)
            {
                bounds = ownCollider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(ownCollider.bounds);
            }
        }

        return hasBounds;
    }

    private bool IsOwnCollider(Collider hitCollider)
    {
        if (hitCollider == null)
            return false;

        if (ownColliders == null || ownColliders.Length == 0)
            ownColliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < ownColliders.Length; i++)
        {
            if (hitCollider == ownColliders[i])
                return true;
        }

        return hitCollider.transform.IsChildOf(transform);
    }

    private void UpdateVisualSquash()
    {
        if (visualRoot == null || rb == null)
            return;

        Vector3 horizontalVelocity = new Vector3(
            LinearVelocity.x,
            0f,
            LinearVelocity.z
        );

        float speedRate = Mathf.Clamp01(horizontalVelocity.magnitude / maxSpeed);

        if (isGrounded)
        {
            float squash = speedRate * squashAmount;

            targetVisualScale = new Vector3(
                defaultVisualScale.x * (1f + squash),
                defaultVisualScale.y * (1f - squash),
                defaultVisualScale.z * (1f + squash)
            );
        }
        else
        {
            float stretch = Mathf.Clamp(
                Mathf.Abs(LinearVelocity.y) * 0.03f,
                0f,
                0.25f
            );

            targetVisualScale = new Vector3(
                defaultVisualScale.x * (1f - stretch * 0.4f),
                defaultVisualScale.y * (1f + stretch),
                defaultVisualScale.z * (1f - stretch * 0.4f)
            );
        }

        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            targetVisualScale,
            Time.deltaTime * squashLerpSpeed
        );
    }

    private void PlayJumpSquash()
    {
        if (visualRoot == null)
            return;

        StopCoroutine(nameof(CoJumpSquash));
        StartCoroutine(nameof(CoJumpSquash));
    }

    private IEnumerator CoJumpSquash()
    {
        visualRoot.localScale = new Vector3(
            defaultVisualScale.x * 1.18f,
            defaultVisualScale.y * 0.75f,
            defaultVisualScale.z * 1.18f
        );

        yield return new WaitForSeconds(jumpSquashTime);

        visualRoot.localScale = new Vector3(
            defaultVisualScale.x * 0.92f,
            defaultVisualScale.y * 1.2f,
            defaultVisualScale.z * 0.92f
        );

        yield return new WaitForSeconds(jumpSquashTime);

        visualRoot.localScale = defaultVisualScale;
        targetVisualScale = defaultVisualScale;
    }

    private void PlayImpactSquash(float impact)
    {
        if (visualRoot == null)
            return;

        float rate = Mathf.Clamp01(impact / 12f);

        visualRoot.localScale = new Vector3(
            defaultVisualScale.x * Mathf.Lerp(1f, 1.22f, rate),
            defaultVisualScale.y * Mathf.Lerp(1f, 0.82f, rate),
            defaultVisualScale.z * Mathf.Lerp(1f, 1.22f, rate)
        );

        targetVisualScale = defaultVisualScale;

        //if (Object != null && Object.HasInputAuthority)
        //    Juice_PlayLanding(impact);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayImpactSquash(float impact)
    {
        PlayImpactSquash(impact);
    }

    public void AddExternalVelocityByObstacle(Vector3 velocity)
    {
        if (!Object.HasStateAuthority)
            return;

        if (!IsAlive)
            return;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null)
            return;

        LinearVelocity = new Vector3(LinearVelocity.x, 0f, LinearVelocity.z);
        rb.AddForce(velocity, ForceMode.VelocityChange);
    }

    public void ApplyObstacleMoveModifierStart(ObstacleMoveModifier modifier)
    {
        if (!HasLocalPlayerAuthority)
            return;

        if (modifier.obstacle == null)
            return;

        obstacleMoveModifiers[modifier.obstacle] = modifier;
        RecalculateMoveModifiers();
    }

    public void ApplyObstacleMoveModifierEnd(GameObject obstacle)
    {
        if (!HasLocalPlayerAuthority)
            return;

        if (obstacle == null)
            return;

        obstacleMoveModifiers.Remove(obstacle);
        RecalculateMoveModifiers();
    }

    public void ApplySlipperySoapSlide(Vector3 direction, float slideForce, float minSlideSpeed)
    {
        if (!HasLocalPlayerAuthority)
            return;

        if (!IsAlive)
            return;

        if (rb == null || IsKinematic)
            return;

        Vector3 horizontal = new Vector3(LinearVelocity.x, 0f, LinearVelocity.z);
        if (horizontal.magnitude >= minSlideSpeed)
            return;

        rb.AddForce(direction.normalized * slideForce, ForceMode.Acceleration);
        PushNetworkMoveState(true);
    }

    private void RecalculateMoveModifiers()
    {
        currentAccelerationMultiplier = 1f;
        currentMoveSpeedMultiplier = 1f;
        currentJumpPowerMultiplier = 1f;

        foreach (var kvp in obstacleMoveModifiers)
        {
            currentAccelerationMultiplier *= kvp.Value.accelerationMultiplier;
            currentMoveSpeedMultiplier *= kvp.Value.moveSpeedMultiplier;
            currentJumpPowerMultiplier *= kvp.Value.jumpPowerMultiplier;
        }
    }

    public void ApplyJumpPad(Vector3 velocity)
    {
        if (!HasLocalPlayerAuthority)
            return;

        if (!IsAlive)
            return;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null || IsKinematic)
            return;

        Vector3 horizontal = new Vector3(LinearVelocity.x, 0f, LinearVelocity.z);
        if (horizontal.magnitude > maxSpeed)
            horizontal = horizontal.normalized * maxSpeed;

        LinearVelocity = new Vector3(horizontal.x, 0f, horizontal.z);
        rb.AddForce(velocity, ForceMode.VelocityChange);

        // 패드 위에 착지(낙사 보류)한 상태였다면 그 낙사를 취소한다.
        // 패드가 발동했으므로 이 착지는 낙사로 처리하지 않는다.
        CancelPendingFallImpactPop();

        ClearBufferedJump();
        lastJumpPadLockTime = Time.time;
        SoundMgr.Inst?.PlaySfx3D(Consts.eSfx.Game_JumpPad_Local, transform.position);
        //Juice_PlayJumpPad();

        SteamAchievements.AddStat(SteamAchievements.STAT_JUMPPAD_USED_TOTAL, 1);
    }

    public void ReceiveKnockback(Vector3 velocity)
    {
        if (!HasLocalPlayerAuthority)
            return;

        if (!IsAlive)
            return;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null)
            return;

        LinearVelocity = velocity;
        SoundMgr.Inst?.PlaySfx(Consts.eSfx.Game_Tbar_Hit);
        PushNetworkMoveState(true);

        // 넉백 수신처는 현재 T-bar 전용(ObstacleTbar.KnockbackPlayer). ApplyEnter 1회라 스팸 없음.
        SteamAchievements.AddStat(SteamAchievements.STAT_HIT_TBAR_TOTAL, 1);
    }

    // InputAuthority 클라이언트에서만 호출. PlayerName이 네트워크에 반영될 때까지 닉네임 RPC를 재전송한다.
    // Spawned() 시점에 보낸 RPC가 연결 초기화 타이밍 때문에 누락되면 게스트 닉네임이 "Player N"으로 남는 문제 보정.
    private void TrySyncNickname()
    {
        if (nicknameSynced)
            return;

        if (!string.IsNullOrWhiteSpace(PlayerName.ToString()))
        {
            nicknameSynced = true;
            return;
        }

        nicknameResendTimer -= Time.unscaledDeltaTime;

        if (nicknameResendTimer > 0f)
            return;

        nicknameResendTimer = 0.5f;
        Rpc_SetNickname(GetLocalNickname());
    }

    private string GetLocalNickname()
    {
        string fallbackName = $"Player {OwnerPlayerRef.PlayerId}";

#if STOVE_BUILD
        string stoveNickname = StoveManager.GetUserNickname();

        if (!string.IsNullOrWhiteSpace(stoveNickname))
        {
            if (stoveNickname.Length > 32)
                stoveNickname = stoveNickname.Substring(0, 32);

            return stoveNickname;
        }
#endif

        if (!IsSteamAvailableForNickname())
            return fallbackName;

        try
        {
            string steamName = Steamworks.SteamFriends.GetPersonaName();

            if (string.IsNullOrWhiteSpace(steamName))
                return fallbackName;

            if (steamName.Length > 32)
                steamName = steamName.Substring(0, 32);

            return steamName;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PlayerCtrl] Failed to get Steam nickname. Use fallback name. Reason: {e.Message}");
            return fallbackName;
        }
    }

    private bool IsSteamAvailableForNickname()
    {
        if (AppInst.Inst == null)
            return false;

        if (AppInst.Inst.SteamNetworkMgr == null)
            return false;

        if (!AppInst.Inst.SteamNetworkMgr.IsConnected)
            return false;

        return SteamManager.Initialized;
    }

    [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    private void Rpc_SetNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            nickname = $"Player {OwnerPlayerRef.PlayerId}";

        if (nickname.Length > 32)
            nickname = nickname.Substring(0, 32);

        PlayerName = nickname;
    }

    public string GetDisplayName()
    {
        if (!initialized || Object == null || !Object.IsValid)
            return "Player";

        string nickname = PlayerName.ToString();

        if (string.IsNullOrWhiteSpace(nickname))
            return $"Player {OwnerPlayerRef.PlayerId}";

        return nickname;
    }

    // 네트워크로 동기화된 닉네임(PlayerName)이 실제로 도착했는지 여부.
    // 아직이면 GetDisplayName이 "Player N" 폴백을 반환하므로 UI 측에서 재빌드 판단에 사용한다.
    public bool HasResolvedNickname()
    {
        if (!initialized || Object == null || !Object.IsValid)
            return false;

        return !string.IsNullOrWhiteSpace(PlayerName.ToString());
    }

    private void OnDrawGizmosSelected()
    {
        if (ownColliders == null || ownColliders.Length == 0)
            ownColliders = GetComponentsInChildren<Collider>();

        float radius = GetGroundCheckCastRadius();
        float distance = GetGroundCheckCastDistance();
        Vector3 origin = GetGroundCheckOrigin();
        Vector3 end = origin + Vector3.down * distance;

        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            radius,
            Vector3.down,
            groundHits,
            distance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        bool groundedNow = false;
        RaycastHit groundHit = default;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = groundHits[i];

            if (hit.collider == null)
                continue;

            if (IsOwnCollider(hit.collider))
                continue;

            if (hit.normal.y < 0.35f)
                continue;

            groundedNow = true;
            groundHit = hit;
            break;
        }

        Gizmos.color = groundedNow ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(end, radius);
        Gizmos.DrawLine(origin + Vector3.left * radius, end + Vector3.left * radius);
        Gizmos.DrawLine(origin + Vector3.right * radius, end + Vector3.right * radius);
        Gizmos.DrawLine(origin + Vector3.forward * radius, end + Vector3.forward * radius);
        Gizmos.DrawLine(origin + Vector3.back * radius, end + Vector3.back * radius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(origin, Mathf.Max(0.03f, radius * 0.15f));

        if (!groundedNow)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundHit.point, Mathf.Max(0.04f, radius * 0.2f));
        Gizmos.DrawLine(origin, groundHit.point);
    }
}
