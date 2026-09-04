using System.Collections.Generic;

namespace Workshop
{
    /// <summary>
    /// 오브젝트 하나에 대한 상태 변화. before==null이면 추가, after==null이면 삭제, 둘 다 있으면 수정.
    /// before/after는 딥클론 스냅샷이라 적용/되돌림이 반복돼도 오염되지 않는다.
    /// </summary>
    public sealed class EditChange
    {
        public readonly string objectId;
        public readonly MapObject before; // null이면 이 변경으로 오브젝트가 새로 생김
        public readonly MapObject after;  // null이면 이 변경으로 오브젝트가 삭제됨
        public readonly int index;        // 재삽입 시 원래 위치 복원용(삭제 되돌림)

        public EditChange(string objectId, MapObject before, MapObject after, int index)
        {
            this.objectId = objectId;
            this.before = before;
            this.after = after;
            this.index = index;
        }
    }

    /// <summary>여러 오브젝트 변경을 하나의 되돌림 단위로 묶는 명령.</summary>
    public sealed class EditCommand
    {
        public readonly string label;
        public readonly List<EditChange> changes;

        public EditCommand(string label, List<EditChange> changes)
        {
            this.label = label;
            this.changes = changes ?? new List<EditChange>();
        }

        public bool IsEmpty => changes.Count == 0;
    }

    /// <summary>
    /// Undo/Redo 스택. 새 명령을 push하면 redo 스택은 비워진다(표준 편집기 동작).
    /// 실제 데이터 적용은 IEditTarget 구현(문서)이 담당한다.
    /// </summary>
    public sealed class EditHistory
    {
        private readonly List<EditCommand> undoStack = new();
        private readonly List<EditCommand> redoStack = new();
        private readonly int capacity;

        public EditHistory(int capacity = 200)
        {
            this.capacity = capacity < 1 ? 1 : capacity;
        }

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;
        public int UndoCount => undoStack.Count;
        public int RedoCount => redoStack.Count;

        public string NextUndoLabel => CanUndo ? undoStack[undoStack.Count - 1].label : null;
        public string NextRedoLabel => CanRedo ? redoStack[redoStack.Count - 1].label : null;

        /// <summary>이미 문서에 적용된 명령을 기록한다.</summary>
        public void Record(EditCommand command)
        {
            if (command == null || command.IsEmpty)
                return;

            undoStack.Add(command);
            redoStack.Clear();

            if (undoStack.Count > capacity)
                undoStack.RemoveAt(0);
        }

        public EditCommand PopUndo()
        {
            if (!CanUndo)
                return null;

            EditCommand cmd = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            redoStack.Add(cmd);
            return cmd;
        }

        public EditCommand PopRedo()
        {
            if (!CanRedo)
                return null;

            EditCommand cmd = redoStack[redoStack.Count - 1];
            redoStack.RemoveAt(redoStack.Count - 1);
            undoStack.Add(cmd);
            return cmd;
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }
}
