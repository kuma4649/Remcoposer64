using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.StepEditorPanelControl
{
    public class CursorInfo
    {
        /// <summary>
        /// メインカーソル位置（行）
        /// </summary>
        public int Index { get; set; } = 0;

        /// <summary>
        /// メインカーソルの列位置
        /// </summary>
        public int Position { get; set; } = 0;

        /// <summary>
        /// メインカーソルのイベント
        /// </summary>
        public LinkedListNode<MIDIEvent> EventNode { get; set; } = null;

        /// <summary>
        /// 選択範囲の開始行（Shift選択用）
        /// </summary>
        public int SelectionStart { get;
            set;
        } = -1;

        /// <summary>
        /// 選択範囲の終了行（Shift選択用）
        /// </summary>
        public int SelectionEnd { get; set; } = -1;

        /// <summary>
        /// 個別選択（Ctrl選択用）
        /// </summary>
        public HashSet<int> MultiSelectIndices { get; private set; } = new HashSet<int>();

        /// <summary>
        /// 選択状態をクリア
        /// </summary>
        public void ClearSelection()
        {
            SelectionStart = -1;
            SelectionEnd = -1;
            MultiSelectIndices.Clear();
        }

        /// <summary>
        /// 範囲選択が有効かどうか
        /// </summary>
        public bool HasRangeSelection =>
            SelectionStart >= 0 && SelectionEnd >= 0 && SelectionStart != SelectionEnd;

        /// <summary>
        /// 個別選択が有効かどうか
        /// </summary>
        public bool HasMultiSelection =>
            MultiSelectIndices.Count > 0;

        public IEnumerable<int> GetSelectedIndices()
        {
            var result = new HashSet<int>(MultiSelectIndices);

            if (HasRangeSelection)
            {
                int start = Math.Min(SelectionStart, SelectionEnd);
                int end = Math.Max(SelectionStart, SelectionEnd);

                for (int i = start; i <= end; i++)
                    result.Add(i);
            }

            return result;
        }

        public bool IsSelectedRow(int index)
        {
            // メインカーソルは別扱い
            if (index == this.Index)
                return false;

            // Ctrl 個別選択
            if (MultiSelectIndices.Contains(index))
                return true;

            // Shift 範囲選択
            if (SelectionStart >= 0 && SelectionEnd >= 0)
            {
                int start = Math.Min(SelectionStart, SelectionEnd);
                int end = Math.Max(SelectionStart, SelectionEnd);

                if (index >= start && index <= end)
                    return true;
            }

            return false;
        }

    }
}
