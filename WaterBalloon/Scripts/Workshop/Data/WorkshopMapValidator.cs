using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// 맵 파일/맵 데이터 검증(계획 §9). 로더와 에디터가 공유한다.
    /// assetId 해석은 PlaceableAssetRegistry를 통하며, 해석 불가능한 에셋은 오류로 처리한다.
    /// </summary>
    public static class WorkshopMapValidator
    {
        private const int MaxPropertiesPerObject = 64;
        private const int MaxObjectIdLength = 64;
        private const int MaxAnchorsPerObject = 8;

        // ------------------------------------------------------------------
        // 매니페스트 검증(§9.1). mapFileBytes를 주면 콘텐츠 해시까지 확인한다(null이면 해시 검사 생략).
        // ------------------------------------------------------------------
        public static WorkshopValidationResult ValidateManifest(WorkshopMapManifest manifest, byte[] mapFileBytes)
        {
            var r = new WorkshopValidationResult();

            if (manifest == null)
            {
                r.Error("manifest가 null");
                return r;
            }

            if (manifest.schemaVersion != WorkshopSchema.CurrentSchemaVersion)
                r.Error($"지원하지 않는 스키마 버전: {manifest.schemaVersion} (지원: {WorkshopSchema.CurrentSchemaVersion})");

            if (!string.Equals(manifest.contentType, WorkshopSchema.ContentTypeMap, StringComparison.Ordinal))
                r.Error($"contentType이 map이 아님: '{manifest.contentType}'");

            ValidateString(r, "title", manifest.title, WorkshopLimits.MaxDisplayNameLength, required: true);
            ValidateString(r, "description", manifest.description, WorkshopLimits.MaxDescriptionLength, required: false);

            if (!string.Equals(manifest.mapFile, WorkshopSchema.MapFileName, StringComparison.Ordinal))
                r.Error($"mapFile은 '{WorkshopSchema.MapFileName}'만 허용: '{manifest.mapFile}'");

            if (!string.IsNullOrEmpty(manifest.previewFile)
                && !string.Equals(manifest.previewFile, WorkshopSchema.PreviewFileName, StringComparison.Ordinal))
                r.Error($"previewFile은 '{WorkshopSchema.PreviewFileName}'만 허용: '{manifest.previewFile}'");

            // 1차에는 requiredPackages가 반드시 비어 있어야 한다(§5, §7.1).
            if (manifest.requiredPackages != null && manifest.requiredPackages.Length > 0)
                r.Error("1차에서는 requiredPackages를 지원하지 않는다(빈 배열이어야 함)");

            if (manifest.contentVersion < 1)
                r.Error($"contentVersion은 1 이상이어야 함: {manifest.contentVersion}");

            if (mapFileBytes != null)
            {
                if (string.IsNullOrEmpty(manifest.contentHash))
                    r.Error("contentHash가 비어 있음");
                else if (!WorkshopContentHash.Matches(manifest.contentHash, mapFileBytes))
                    r.Error("콘텐츠 해시 불일치(map.json이 manifest와 다름)");
            }

            return r;
        }

        // ------------------------------------------------------------------
        // 맵 데이터 검증(§9.2).
        //   requiredStartPoints: 최대 참가 인원 수용을 위한 최소 시작 지점 수.
        // ------------------------------------------------------------------
        public static WorkshopValidationResult ValidateMap(WorkshopMapData map, int requiredStartPoints)
        {
            return ValidateMapInternal(map, requiredStartPoints, requirePublishTokenCandidates: false);
        }

        /// <summary>새 게시/업데이트에 필요한 토큰 후보 지점 정책까지 포함해 검증한다.</summary>
        public static WorkshopValidationResult ValidateMapForPublishing(WorkshopMapData map, int requiredStartPoints)
        {
            return ValidateMapInternal(map, requiredStartPoints, requirePublishTokenCandidates: true);
        }

        private static WorkshopValidationResult ValidateMapInternal(
            WorkshopMapData map,
            int requiredStartPoints,
            bool requirePublishTokenCandidates)
        {
            var r = new WorkshopValidationResult();

            if (map == null)
            {
                r.Error("map이 null");
                return r;
            }

            if (map.schemaVersion != WorkshopSchema.CurrentSchemaVersion)
                r.Error($"지원하지 않는 맵 스키마 버전: {map.schemaVersion}");

            if (!IsGuid(map.localMapId))
                r.Error($"localMapId가 GUID 형식이 아님: '{map.localMapId}'");

            ValidateString(r, "displayName", map.displayName, WorkshopLimits.MaxDisplayNameLength, required: true);

            if (!Enum.TryParse(map.difficulty, out eMapDifficulty _))
                r.Error($"difficulty 파싱 불가: '{map.difficulty}'");

            if (!string.IsNullOrEmpty(map.themeId) && !IsStableAssetId(map.themeId))
                r.Error($"themeId가 안정 ID 형식이 아님: '{map.themeId}'");

            // 베이스 씬은 형식만 검사한다. "이 빌드에 그 씬이 있는가"는 데이터 유효성이 아니라 실행 가능
            // 여부라서, 카탈로그/시작 경로의 BuildIndex 게이트에서 따로 막는다(구버전 클라 호환).
            if (!WorkshopBaseScenes.IsValidFormat(map.baseSceneId))
                r.Error($"baseSceneId가 안정 ID 형식이 아님: '{map.baseSceneId}'");

            ValidateBounds(r, map.bounds);

            MapObject[] objects = map.objects ?? Array.Empty<MapObject>();

            if (objects.Length > WorkshopLimits.MaxObjects)
                r.Error($"오브젝트 수 초과: {objects.Length} > {WorkshopLimits.MaxObjects}");

            int startCount = 0;
            int goalCount = 0;
            int checkpointCount = 0;
            int dynamicCount = 0;
            int obstacleCount = 0;
            int tokenCandidateCount = 0;

            var seenObjectIds = new HashSet<string>(StringComparer.Ordinal);
            var perAssetCount = new Dictionary<string, int>(StringComparer.Ordinal);
            var startPositions = new List<Vector3>();
            var goalPositions = new List<Vector3>();
            var tokenCandidatePositions = new List<Vector3>();

            for (int i = 0; i < objects.Length; i++)
            {
                MapObject obj = objects[i];
                string tag = $"objects[{i}]";

                if (obj == null)
                {
                    r.Error($"{tag}가 null");
                    continue;
                }

                // objectId: 유일성 + 형식
                if (!IsGuid(obj.objectId) || obj.objectId.Length > MaxObjectIdLength)
                    r.Error($"{tag}.objectId가 GUID 형식이 아님: '{obj.objectId}'");
                else if (!seenObjectIds.Add(obj.objectId))
                    r.Error($"{tag}.objectId 중복: '{obj.objectId}'");

                // assetId 해석
                if (!PlaceableAssetRegistry.TryResolve(obj.assetId, out PlaceableAssetDef def))
                {
                    r.Error($"{tag}.assetId 해석 불가: '{obj.assetId}'");
                    continue; // 정의가 없으면 이하 범위 검사 불가
                }

                // 카테고리별 집계
                switch (def.progressionRole)
                {
                    case eProgressionRole.Start: startCount++; break;
                    case eProgressionRole.Goal: goalCount++; break;
                    case eProgressionRole.Checkpoint: checkpointCount++; break;
                }

                bool hasValidPosition = IsVec3(obj.position);
                if (hasValidPosition && def.progressionRole == eProgressionRole.Start)
                    startPositions.Add(ToVector3(obj.position));
                else if (hasValidPosition && def.progressionRole == eProgressionRole.Goal)
                    goalPositions.Add(ToVector3(obj.position));

                if (string.Equals(obj.assetId, WorkshopSchema.TokenSpawnCandidateAssetId, StringComparison.Ordinal))
                {
                    tokenCandidateCount++;

                    if (hasValidPosition)
                        tokenCandidatePositions.Add(ToVector3(obj.position));
                }

                if (def.category == ePlaceableCategory.Obstacle)
                    obstacleCount++;

                if (def.IsDynamic)
                    dynamicCount++;

                // 에셋별 최대 개수
                if (def.maxCountPerMap > 0)
                {
                    perAssetCount.TryGetValue(obj.assetId, out int c);
                    perAssetCount[obj.assetId] = c + 1;
                }

                ValidateTransform(r, tag, obj, def);
                ValidateProperties(r, tag, obj, def);
                ValidateAnchors(r, tag, obj);
            }

            // 에셋별 최대 개수 초과 검사
            foreach (KeyValuePair<string, int> kv in perAssetCount)
            {
                if (PlaceableAssetRegistry.TryResolve(kv.Key, out PlaceableAssetDef def)
                    && def.maxCountPerMap > 0 && kv.Value > def.maxCountPerMap)
                {
                    r.Error($"에셋 '{kv.Key}' 개수 초과: {kv.Value} > {def.maxCountPerMap}");
                }
            }

            // 진행 오브젝트 필수 조건(§9.2)
            if (startCount < WorkshopLimits.MinStartPoints)
                r.Error($"시작 지점이 없음(최소 {WorkshopLimits.MinStartPoints})");

            if (goalCount < 1)
                r.Error("골인지점이 없음");

            int needStart = Mathf.Max(requiredStartPoints, WorkshopLimits.MinStartPoints);

            if (startCount < needStart)
                r.Error($"시작 지점 수 부족: {startCount} < 필요 {needStart}(최대 참가 인원 수용)");

            if (startCount > WorkshopLimits.MaxStartPoints)
                r.Error($"시작 지점 수 초과: {startCount} > {WorkshopLimits.MaxStartPoints}");

            if (checkpointCount > WorkshopLimits.MaxCheckpoints)
                r.Error($"체크포인트 수 초과: {checkpointCount} > {WorkshopLimits.MaxCheckpoints}");

            if (obstacleCount > WorkshopLimits.MaxObstacles)
                r.Error($"장애물 수 초과: {obstacleCount} > {WorkshopLimits.MaxObstacles}");

            if (dynamicCount > WorkshopLimits.MaxDynamicObjects)
                r.Error($"동적 오브젝트 수 초과: {dynamicCount} > {WorkshopLimits.MaxDynamicObjects}");

            if (requirePublishTokenCandidates)
            {
                if (tokenCandidateCount < WorkshopLimits.MinTokenSpawnCandidatesForPublish)
                {
                    r.Error(
                        $"게시하려면 토큰 후보 지점이 최소 {WorkshopLimits.MinTokenSpawnCandidatesForPublish}개 필요함: " +
                        $"{tokenCandidateCount}/{WorkshopLimits.MinTokenSpawnCandidatesForPublish}");
                }

                ValidateTokenCandidateDistances(
                    r,
                    tokenCandidatePositions,
                    startPositions,
                    goalPositions);
            }

            return r;
        }

        public static WorkshopValidationResult ValidateMap(WorkshopMapData map)
        {
            return ValidateMap(map, WorkshopLimits.DefaultRequiredStartPoints);
        }

        public static WorkshopValidationResult ValidateMapForPublishing(WorkshopMapData map)
        {
            return ValidateMapForPublishing(map, WorkshopLimits.DefaultRequiredStartPoints);
        }

        // ------------------------------------------------------------------

        private static void ValidateTokenCandidateDistances(
            WorkshopValidationResult r,
            List<Vector3> candidates,
            List<Vector3> starts,
            List<Vector3> goals)
        {
            float candidateSpacingSqr = WorkshopLimits.MinTokenCandidateSpacing * WorkshopLimits.MinTokenCandidateSpacing;
            float startDistanceSqr = WorkshopLimits.MinTokenCandidateDistanceFromStart * WorkshopLimits.MinTokenCandidateDistanceFromStart;
            float goalDistanceSqr = WorkshopLimits.MinTokenCandidateDistanceFromGoal * WorkshopLimits.MinTokenCandidateDistanceFromGoal;

            for (int i = 0; i < candidates.Count; i++)
            {
                for (int j = i + 1; j < candidates.Count; j++)
                {
                    float distance = Vector3.Distance(candidates[i], candidates[j]);

                    if (distance * distance < candidateSpacingSqr)
                    {
                        r.Error(
                            $"토큰 후보 {i + 1}과 {j + 1}의 거리가 너무 가까움: " +
                            $"{distance:0.##}m / 최소 {WorkshopLimits.MinTokenCandidateSpacing:0.##}m");
                    }
                }

                for (int j = 0; j < starts.Count; j++)
                {
                    float distance = Vector3.Distance(candidates[i], starts[j]);

                    if (distance * distance < startDistanceSqr)
                    {
                        r.Error(
                            $"토큰 후보 {i + 1}이 Start와 너무 가까움: " +
                            $"{distance:0.##}m / 최소 {WorkshopLimits.MinTokenCandidateDistanceFromStart:0.##}m");
                        break;
                    }
                }

                for (int j = 0; j < goals.Count; j++)
                {
                    float distance = Vector3.Distance(candidates[i], goals[j]);

                    if (distance * distance < goalDistanceSqr)
                    {
                        r.Error(
                            $"토큰 후보 {i + 1}이 Goal과 너무 가까움: " +
                            $"{distance:0.##}m / 최소 {WorkshopLimits.MinTokenCandidateDistanceFromGoal:0.##}m");
                        break;
                    }
                }
            }
        }

        private static Vector3 ToVector3(float[] value)
        {
            return new Vector3(value[0], value[1], value[2]);
        }

        // ------------------------------------------------------------------

        private static void ValidateBounds(WorkshopValidationResult r, MapBounds bounds)
        {
            if (bounds == null)
            {
                r.Error("bounds가 null");
                return;
            }

            if (!IsVec3(bounds.center))
                r.Error("bounds.center가 유효한 3원소 벡터가 아님");

            if (!IsVec3(bounds.size))
            {
                r.Error("bounds.size가 유효한 3원소 벡터가 아님");
                return;
            }

            for (int a = 0; a < 3; a++)
            {
                if (bounds.size[a] < 0f)
                    r.Error($"bounds.size[{a}]가 음수");
                else if (bounds.size[a] > WorkshopLimits.MaxBoundsExtent * 2f)
                    r.Error($"bounds.size[{a}] 초과: {bounds.size[a]}");
            }
        }

        private static void ValidateTransform(WorkshopValidationResult r, string tag, MapObject obj, PlaceableAssetDef def)
        {
            if (!IsVec3(obj.position))
                r.Error($"{tag}.position이 유효하지 않음");
            else
            {
                for (int a = 0; a < 3; a++)
                {
                    if (Mathf.Abs(obj.position[a]) > WorkshopLimits.MaxAbsCoordinate)
                        r.Error($"{tag}.position[{a}] 좌표 상한 초과: {obj.position[a]}");
                }
            }

            if (!IsVec3(obj.rotation))
                r.Error($"{tag}.rotation이 유효하지 않음");

            if (!IsVec3(obj.scale))
            {
                r.Error($"{tag}.scale이 유효하지 않음");
                return;
            }

            for (int a = 0; a < 3; a++)
            {
                float s = obj.scale[a];

                float lo = Mathf.Max(WorkshopLimits.MinScale, AxisOrDefault(def.minScale, a, WorkshopLimits.MinScale));
                float hi = Mathf.Min(WorkshopLimits.MaxScale, AxisOrDefault(def.maxScale, a, WorkshopLimits.MaxScale));

                if (s < lo - Mathf.Epsilon || s > hi + Mathf.Epsilon)
                    r.Error($"{tag}.scale[{a}] 범위 벗어남: {s} (허용 {lo}~{hi})");
            }
        }

        // 앵커(레이저 시작/종료 등 멀티파트 배치물의 보조 지점). IWorkshopAnchorable 구현체가 인덱스로
        // 접근하므로(예: list[0]/list[1]) 좌표 형식이 깨지면 빌드 시점에 IndexOutOfRangeException으로
        // 이어질 수 있다 — position/rotation과 동일한 IsVec3+좌표 상한 검사를 여기서도 적용한다.
        private static void ValidateAnchors(WorkshopValidationResult r, string tag, MapObject obj)
        {
            MapAnchor[] anchors = obj.anchors ?? Array.Empty<MapAnchor>();

            if (anchors.Length > MaxAnchorsPerObject)
            {
                r.Error($"{tag}.anchors 수 초과: {anchors.Length} > {MaxAnchorsPerObject}");
                return;
            }

            for (int i = 0; i < anchors.Length; i++)
            {
                MapAnchor anchor = anchors[i];

                if (anchor == null || !IsVec3(anchor.position))
                {
                    r.Error($"{tag}.anchors[{i}].position이 유효하지 않음");
                    continue;
                }

                for (int a = 0; a < 3; a++)
                {
                    if (Mathf.Abs(anchor.position[a]) > WorkshopLimits.MaxAbsCoordinate)
                        r.Error($"{tag}.anchors[{i}].position[{a}] 좌표 상한 초과: {anchor.position[a]}");
                }
            }
        }

        private static void ValidateProperties(WorkshopValidationResult r, string tag, MapObject obj, PlaceableAssetDef def)
        {
            MapObjectProperty[] props = obj.properties ?? Array.Empty<MapObjectProperty>();

            if (props.Length > MaxPropertiesPerObject)
            {
                r.Error($"{tag} 속성 수 초과: {props.Length} > {MaxPropertiesPerObject}");
                return;
            }

            // 허용 속성 정의 맵
            var allowed = new Dictionary<string, PlaceablePropertyDef>(StringComparer.Ordinal);

            if (def.allowedProperties != null)
            {
                foreach (PlaceablePropertyDef pd in def.allowedProperties)
                {
                    if (pd != null && !string.IsNullOrEmpty(pd.key))
                        allowed[pd.key] = pd;
                }
            }

            var seenKeys = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < props.Length; i++)
            {
                MapObjectProperty p = props[i];

                if (p == null || string.IsNullOrEmpty(p.key))
                {
                    r.Error($"{tag}.properties[{i}] 키가 비어 있음");
                    continue;
                }

                if (!seenKeys.Add(p.key))
                {
                    r.Error($"{tag}.properties 키 중복: '{p.key}'");
                    continue;
                }

                // 알 수 없는 속성은 명시적으로 거부(§9.2). 조용히 무시하지 않는다.
                if (!allowed.TryGetValue(p.key, out PlaceablePropertyDef pd))
                {
                    r.Error($"{tag} 허용되지 않은 속성 키: '{p.key}'");
                    continue;
                }

                if (!Enum.TryParse(p.type, out ePropertyType declaredType) || declaredType != pd.type)
                {
                    r.Error($"{tag}.properties['{p.key}'] 타입 불일치: 선언 '{p.type}', 기대 '{pd.type}'");
                    continue;
                }

                ValidatePropertyValue(r, $"{tag}.properties['{p.key}']", pd, p.value);
            }
        }

        private static void ValidatePropertyValue(WorkshopValidationResult r, string tag, PlaceablePropertyDef pd, string value)
        {
            switch (pd.type)
            {
                case ePropertyType.Float:
                    if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float f)
                        || float.IsNaN(f) || float.IsInfinity(f))
                        r.Error($"{tag} float 파싱 불가: '{value}'");
                    else if (f < pd.min - Mathf.Epsilon || f > pd.max + Mathf.Epsilon)
                        r.Error($"{tag} 범위 벗어남: {f} (허용 {pd.min}~{pd.max})");
                    break;

                case ePropertyType.Int:
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
                        r.Error($"{tag} int 파싱 불가: '{value}'");
                    else if (n < pd.min || n > pd.max)
                        r.Error($"{tag} 범위 벗어남: {n} (허용 {pd.min}~{pd.max})");
                    break;

                case ePropertyType.Bool:
                    if (!string.Equals(value, "true", StringComparison.Ordinal)
                        && !string.Equals(value, "false", StringComparison.Ordinal))
                        r.Error($"{tag} bool은 'true'/'false'만 허용: '{value}'");
                    break;

                case ePropertyType.Enum:
                    if (pd.enumValues == null || Array.IndexOf(pd.enumValues, value) < 0)
                        r.Error($"{tag} 허용되지 않은 enum 값: '{value}'");
                    break;

                case ePropertyType.Color:
                    if (!ColorUtility.TryParseHtmlString(value, out _))
                        r.Error($"{tag} 색상 파싱 불가(#RRGGBB[AA] 형식): '{value}'");
                    break;

                default:
                    r.Error($"{tag} 알 수 없는 속성 타입");
                    break;
            }
        }

        // --- 헬퍼 ---

        private static void ValidateString(WorkshopValidationResult r, string field, string value, int maxLen, bool required)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    r.Error($"{field}가 비어 있음");
                return;
            }

            if (value.Length > maxLen)
                r.Error($"{field} 길이 초과: {value.Length} > {maxLen}");
        }

        private static bool IsVec3(float[] v)
        {
            if (v == null || v.Length != 3)
                return false;

            for (int i = 0; i < 3; i++)
            {
                if (float.IsNaN(v[i]) || float.IsInfinity(v[i]))
                    return false;
            }

            return true;
        }

        private static float AxisOrDefault(Vector3 v, int axis, float fallback)
        {
            float value = axis == 0 ? v.x : axis == 1 ? v.y : v.z;
            return value <= 0f ? fallback : value;
        }

        private static bool IsStableAssetId(string id)
        {
            return WorkshopSchema.IsBuiltInAssetId(id) || WorkshopSchema.IsWorkshopAssetId(id);
        }

        private static bool IsGuid(string s)
        {
            return !string.IsNullOrEmpty(s) && Guid.TryParse(s, out _);
        }
    }
}
