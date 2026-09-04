using System;
using System.Collections.Generic;

namespace Workshop
{
    /// <summary>
    /// 편집 중인 맵의 라이브 상태 + Undo/Redo. 순수 데이터 모델이며 씬/UI에 의존하지 않는다.
    /// 모든 변경은 명령(EditCommand)을 통해 적용되어 되돌릴 수 있다. 씬 바인딩/선택/기즈모는
    /// 이 위에 올라가는 별도 컨트롤러가 담당한다(계획 §12-B).
    ///
    /// 저장/검증 시에는 <see cref="BuildData"/>로 WorkshopMapData 스냅샷을 얻어 기존
    /// 직렬화/검증 파이프라인(Phase A)에 그대로 넘긴다.
    /// </summary>
    public sealed class WorkshopMapDocument
    {
        private readonly List<MapObject> objects = new();
        private readonly EditHistory history = new();

        public string LocalMapId { get; private set; }
        public string DisplayName { get; set; }
        public string Difficulty { get; set; } = "Normal";
        public string ThemeId { get; set; } = string.Empty;

        // 이 맵이 올라가는 베이스 씬(빈 문자열이면 빈 맵). 생성 시 정해지고 이후에는 바꾸지 않는다
        // — 베이스가 바뀌면 이미 배치한 오브젝트의 좌표 기준이 통째로 어긋나기 때문.
        public string BaseSceneId { get; private set; } = string.Empty;

        public MapBounds Bounds { get; set; } = new MapBounds();

        // 마지막 저장 이후 변경 여부. 저장 성공 시 MarkSaved()로 해제한다.
        public bool IsDirty { get; private set; }

        public IReadOnlyList<MapObject> Objects => objects;
        public EditHistory History => history;
        public int Count => objects.Count;

        // ---- 생성 ----

        public static WorkshopMapDocument NewEmpty(string displayName)
        {
            return NewEmpty(displayName, string.Empty);
        }

        /// <summary>베이스 씬을 지정해 새 맵을 만든다. baseSceneId가 비면 기존과 동일한 빈 맵.</summary>
        public static WorkshopMapDocument NewEmpty(string displayName, string baseSceneId)
        {
            return new WorkshopMapDocument
            {
                LocalMapId = Guid.NewGuid().ToString(),
                DisplayName = string.IsNullOrEmpty(displayName) ? "New Map" : displayName,
                Difficulty = "Normal",
                ThemeId = string.Empty,
                BaseSceneId = baseSceneId ?? string.Empty,
                Bounds = new MapBounds { center = new[] { 0f, 0f, 0f }, size = new[] { 200f, 100f, 200f } }
            };
        }

        public static WorkshopMapDocument FromData(WorkshopMapData data)
        {
            var doc = new WorkshopMapDocument();

            if (data == null)
            {
                doc.LocalMapId = Guid.NewGuid().ToString();
                return doc;
            }

            doc.LocalMapId = string.IsNullOrEmpty(data.localMapId) ? Guid.NewGuid().ToString() : data.localMapId;
            doc.DisplayName = data.displayName;
            doc.Difficulty = string.IsNullOrEmpty(data.difficulty) ? "Normal" : data.difficulty;
            doc.ThemeId = data.themeId ?? string.Empty;
            doc.BaseSceneId = data.baseSceneId ?? string.Empty;
            doc.Bounds = data.bounds ?? new MapBounds();

            MapObject[] src = data.objects ?? Array.Empty<MapObject>();

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] != null)
                    doc.objects.Add(WorkshopMapClone.CloneObject(src[i]));
            }

            return doc;
        }

        /// <summary>저장/검증용 스냅샷. 문서 내부와 참조를 공유하지 않는다.</summary>
        public WorkshopMapData BuildData()
        {
            var data = new WorkshopMapData
            {
                schemaVersion = WorkshopSchema.CurrentSchemaVersion,
                localMapId = LocalMapId,
                displayName = DisplayName,
                difficulty = Difficulty,
                themeId = ThemeId,
                baseSceneId = BaseSceneId,
                bounds = new MapBounds
                {
                    center = WorkshopMapClone.CloneFloats(Bounds?.center) ?? new[] { 0f, 0f, 0f },
                    size = WorkshopMapClone.CloneFloats(Bounds?.size) ?? new[] { 0f, 0f, 0f }
                },
                objects = new MapObject[objects.Count]
            };

            for (int i = 0; i < objects.Count; i++)
                data.objects[i] = WorkshopMapClone.CloneObject(objects[i]);

            return data;
        }

        // ---- 조회 ----

        public int IndexOf(string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
                return -1;

            for (int i = 0; i < objects.Count; i++)
            {
                if (string.Equals(objects[i].objectId, objectId, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        public MapObject Find(string objectId)
        {
            int idx = IndexOf(objectId);
            return idx >= 0 ? objects[idx] : null;
        }

        // ---- 편집 연산(모두 명령 기록) ----

        public MapObject AddObject(string assetId, float[] position, float[] rotation, float[] scale)
        {
            var obj = new MapObject
            {
                objectId = Guid.NewGuid().ToString(),
                assetId = assetId,
                position = WorkshopMapClone.CloneFloats(position) ?? new[] { 0f, 0f, 0f },
                rotation = WorkshopMapClone.CloneFloats(rotation) ?? new[] { 0f, 0f, 0f },
                scale = WorkshopMapClone.CloneFloats(scale) ?? new[] { 1f, 1f, 1f },
                properties = Array.Empty<MapObjectProperty>()
            };

            var change = new EditChange(obj.objectId, null, WorkshopMapClone.CloneObject(obj), objects.Count);
            Commit(new EditCommand("Add " + assetId, new List<EditChange> { change }));
            return Find(obj.objectId);
        }

        public void DeleteObjects(IEnumerable<string> objectIds)
        {
            if (objectIds == null)
                return;

            var changes = new List<EditChange>();

            foreach (string id in objectIds)
            {
                int idx = IndexOf(id);

                if (idx < 0)
                    continue;

                changes.Add(new EditChange(id, WorkshopMapClone.CloneObject(objects[idx]), null, idx));
            }

            if (changes.Count == 0)
                return;

            Commit(new EditCommand("Delete", changes));
        }

        /// <summary>여러 오브젝트의 트랜스폼을 한 번의 되돌림 단위로 커밋한다(이동/회전/크기 공통).</summary>
        public void SetTransforms(IEnumerable<TransformEdit> edits)
        {
            if (edits == null)
                return;

            var changes = new List<EditChange>();

            foreach (TransformEdit e in edits)
            {
                int idx = IndexOf(e.objectId);

                if (idx < 0)
                    continue;

                MapObject before = WorkshopMapClone.CloneObject(objects[idx]);
                MapObject after = WorkshopMapClone.CloneObject(objects[idx]);

                if (e.position != null) after.position = WorkshopMapClone.CloneFloats(e.position);
                if (e.rotation != null) after.rotation = WorkshopMapClone.CloneFloats(e.rotation);
                if (e.scale != null) after.scale = WorkshopMapClone.CloneFloats(e.scale);

                changes.Add(new EditChange(e.objectId, before, after, idx));
            }

            if (changes.Count == 0)
                return;

            Commit(new EditCommand("Transform", changes));
        }

        /// <summary>배치 시 프리팹 기본 앵커로 초기화(되돌림 이력 없이 직접 설정).</summary>
        public void InitAnchors(string objectId, MapAnchor[] anchors)
        {
            int idx = IndexOf(objectId);

            if (idx < 0)
                return;

            objects[idx].anchors = WorkshopMapClone.CloneAnchors(anchors);
            IsDirty = true;
        }

        /// <summary>보조 앵커를 커밋한다(레이저 시작/종료 위치 편집).</summary>
        public void SetAnchors(string objectId, MapAnchor[] anchors)
        {
            int idx = IndexOf(objectId);

            if (idx < 0)
                return;

            MapObject before = WorkshopMapClone.CloneObject(objects[idx]);
            MapObject after = WorkshopMapClone.CloneObject(objects[idx]);
            after.anchors = WorkshopMapClone.CloneAnchors(anchors);

            Commit(new EditCommand("SetAnchors", new List<EditChange> { new EditChange(objectId, before, after, idx) }));
        }

        /// <summary>속성 upsert. value가 null이면 해당 키를 제거한다.</summary>
        public void SetProperty(string objectId, string key, string type, string value)
        {
            int idx = IndexOf(objectId);

            if (idx < 0 || string.IsNullOrEmpty(key))
                return;

            MapObject before = WorkshopMapClone.CloneObject(objects[idx]);
            MapObject after = WorkshopMapClone.CloneObject(objects[idx]);

            var props = new List<MapObjectProperty>(after.properties ?? Array.Empty<MapObjectProperty>());
            int found = props.FindIndex(p => p != null && string.Equals(p.key, key, StringComparison.Ordinal));

            if (value == null)
            {
                if (found >= 0)
                    props.RemoveAt(found);
            }
            else if (found >= 0)
            {
                props[found] = new MapObjectProperty { key = key, type = type, value = value };
            }
            else
            {
                props.Add(new MapObjectProperty { key = key, type = type, value = value });
            }

            after.properties = props.ToArray();
            Commit(new EditCommand("SetProperty", new List<EditChange> { new EditChange(objectId, before, after, idx) }));
        }

        /// <summary>선택 오브젝트를 offset만큼 옮긴 복제본을 새 GUID로 추가한다.</summary>
        public List<string> DuplicateObjects(IEnumerable<string> objectIds, float[] offset)
        {
            var newIds = new List<string>();

            if (objectIds == null)
                return newIds;

            var changes = new List<EditChange>();
            int appendIndex = objects.Count;

            foreach (string id in objectIds)
            {
                int idx = IndexOf(id);

                if (idx < 0)
                    continue;

                MapObject dup = WorkshopMapClone.CloneObject(objects[idx]);
                dup.objectId = Guid.NewGuid().ToString();
                dup.position = AddOffset(dup.position, offset);

                changes.Add(new EditChange(dup.objectId, null, WorkshopMapClone.CloneObject(dup), appendIndex++));
                newIds.Add(dup.objectId);
            }

            if (changes.Count > 0)
                Commit(new EditCommand("Duplicate", changes));

            return newIds;
        }

        // ---- Undo/Redo ----

        public bool Undo()
        {
            EditCommand cmd = history.PopUndo();

            if (cmd == null)
                return false;

            ApplyBackward(cmd);
            IsDirty = true;
            return true;
        }

        public bool Redo()
        {
            EditCommand cmd = history.PopRedo();

            if (cmd == null)
                return false;

            ApplyForward(cmd);
            IsDirty = true;
            return true;
        }

        public void MarkSaved()
        {
            IsDirty = false;
        }

        // ---- 내부 적용 엔진 ----

        private void Commit(EditCommand cmd)
        {
            if (cmd == null || cmd.IsEmpty)
                return;

            ApplyForward(cmd);
            history.Record(cmd);
            IsDirty = true;
        }

        private void ApplyForward(EditCommand cmd)
        {
            for (int i = 0; i < cmd.changes.Count; i++)
            {
                EditChange c = cmd.changes[i];

                if (c.after == null)
                    RemoveById(c.objectId);
                else
                    Upsert(c.after, c.index);
            }
        }

        private void ApplyBackward(EditCommand cmd)
        {
            for (int i = cmd.changes.Count - 1; i >= 0; i--)
            {
                EditChange c = cmd.changes[i];

                if (c.before == null)
                    RemoveById(c.objectId); // 추가였으므로 되돌리면 제거
                else
                    Upsert(c.before, c.index);
            }
        }

        // 존재하면 교체, 없으면 index 위치에 삽입. 항상 딥클론을 저장한다.
        private void Upsert(MapObject snapshot, int index)
        {
            MapObject clone = WorkshopMapClone.CloneObject(snapshot);
            int existing = IndexOf(clone.objectId);

            if (existing >= 0)
            {
                objects[existing] = clone;
                return;
            }

            int at = index < 0 ? 0 : (index > objects.Count ? objects.Count : index);
            objects.Insert(at, clone);
        }

        private void RemoveById(string objectId)
        {
            int idx = IndexOf(objectId);

            if (idx >= 0)
                objects.RemoveAt(idx);
        }

        private static float[] AddOffset(float[] pos, float[] offset)
        {
            float[] result = WorkshopMapClone.CloneFloats(pos) ?? new[] { 0f, 0f, 0f };

            if (offset != null)
            {
                for (int i = 0; i < result.Length && i < offset.Length; i++)
                    result[i] += offset[i];
            }

            return result;
        }
    }

    /// <summary>한 오브젝트의 트랜스폼 변경 요청. null인 축은 변경하지 않는다.</summary>
    public readonly struct TransformEdit
    {
        public readonly string objectId;
        public readonly float[] position;
        public readonly float[] rotation;
        public readonly float[] scale;

        public TransformEdit(string objectId, float[] position, float[] rotation, float[] scale)
        {
            this.objectId = objectId;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}
