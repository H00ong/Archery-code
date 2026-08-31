#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI.EditorTools
{
    /// <summary>
    /// Lobby 스탯 패널에서 길어질 수 있는 "Stat Text"를 ScrollView로 감싸고,
    /// 그 옆에 기존 Detail Popup Button을 HorizontalLayoutGroup으로 나란히 배치한다.
    ///
    /// 결과 구조:
    /// StatRow (HorizontalLayoutGroup)
    ///  ├─ StatScrollView (ScrollRect, LayoutElement flexibleWidth=1, 고정 높이)
    ///  │   └─ Viewport (RectMask2D)
    ///  │       └─ Content (VerticalLayoutGroup + ContentSizeFitter)
    ///  │           └─ [기존 Stat Text]   ← 넘치면 세로로 스크롤됨
    ///  └─ [기존 Detail Popup Button] (LayoutElement 50x50)   ← 스크롤 옆에 고정
    ///
    /// 사용법:
    /// 1. Hierarchy에서 "Current/Growth/Equipped Stat Text"와 그에 대응하는
    ///    "... Detail Popup Button"을 함께(둘 다) 선택. (버튼 없이 Stat Text만 선택해도 됨)
    /// 2. 메뉴 Tools > Lobby > Wrap Selected Stat Text In Scroll Row 실행.
    /// 3. 만들어진 StatScrollView의 고정 높이(LayoutElement.preferredHeight)를 원하는 만큼 조정.
    /// 4. View 인스펙터의 기존 Stat Text 슬롯은 오브젝트만 옮긴 것이므로 참조가 유지된다.
    /// Character/Weapon/Armor/Shoe x Current/Growth/Equipped 각 위치에서 반복하면 된다.
    /// </summary>
    public static class MagicEffectRowBuilder
    {
        private const float ButtonSize = 50f;
        private const float DefaultScrollHeight = 200f;

        [MenuItem("Tools/Lobby/Wrap Selected Stat Text In Scroll Row")]
        public static void WrapSelectedStatText()
        {
            // 선택된 오브젝트들 중에서 Stat Text(TMP)와 Detail 버튼을 찾는다.
            TextMeshProUGUI statText = null;
            GameObject button = null;
            foreach (var go in Selection.gameObjects)
            {
                if (go == null) continue;
                if (statText == null && go.GetComponent<TextMeshProUGUI>() != null)
                    statText = go.GetComponent<TextMeshProUGUI>();
                else if (button == null && go.GetComponent<Button>() != null)
                    button = go;
            }

            if (statText == null)
            {
                EditorUtility.DisplayDialog("Stat Scroll Row 생성 실패",
                    "Hierarchy에서 스크롤로 감쌀 Stat Text(TextMeshProUGUI)를 선택하세요.\n" +
                    "옆에 붙일 Detail Popup Button도 함께 선택하면 자동으로 나란히 배치됩니다.",
                    "확인");
                return;
            }

            var statRect = statText.GetComponent<RectTransform>();
            Transform parent = statRect.parent;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Stat Scroll Row 생성 실패", "선택한 Stat Text에 부모 오브젝트가 없습니다.", "확인");
                return;
            }

            int siblingIndex = statRect.GetSiblingIndex();
            Vector2 anchoredPos = statRect.anchoredPosition;
            Vector2 statSize = statRect.sizeDelta;
            float scrollHeight = statSize.y > 1f ? statSize.y : DefaultScrollHeight;
            float rowWidth = statSize.x > 1f ? statSize.x + (button != null ? ButtonSize + 10f : 0f) : 500f;

            Undo.SetCurrentGroupName("Wrap Stat Text In Scroll Row");
            int undoGroup = Undo.GetCurrentGroup();

            // ── Row (HorizontalLayoutGroup 컨테이너) ──
            var row = new GameObject("StatRow", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(row, "Create Row");
            var rowRect = (RectTransform)row.transform;
            rowRect.SetParent(parent, false);
            rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = anchoredPos;
            rowRect.sizeDelta = new Vector2(rowWidth, scrollHeight);
            rowRect.SetSiblingIndex(siblingIndex);

            var hLayout = row.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.UpperLeft;
            hLayout.spacing = 10f;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;

            // ── ScrollView ──
            var scrollViewGO = new GameObject("StatScrollView", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(scrollViewGO, "Create ScrollView");
            scrollViewGO.transform.SetParent(row.transform, false);

            var scrollViewLayoutElement = scrollViewGO.AddComponent<LayoutElement>();
            scrollViewLayoutElement.flexibleWidth = 1f;
            scrollViewLayoutElement.preferredHeight = scrollHeight;

            var scrollRect = scrollViewGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;

            // ── Viewport ──
            var viewportGO = new GameObject("Viewport", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(viewportGO, "Create Viewport");
            viewportGO.transform.SetParent(scrollViewGO.transform, false);
            var viewportRect = (RectTransform)viewportGO.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportRect.pivot = new Vector2(0f, 1f);
            viewportGO.AddComponent<RectMask2D>();

            scrollRect.viewport = viewportRect;

            // ── Content ──
            var contentGO = new GameObject("Content", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(contentGO, "Create Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentRect = (RectTransform)contentGO.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            var vLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            vLayout.childAlignment = TextAnchor.UpperLeft;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            var contentFitter = contentGO.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            // ── 기존 Stat Text를 Content 안으로 이동 (참조 유지) ──
            Undo.SetTransformParent(statRect, contentGO.transform, "Move Stat Text Into Content");
            statRect.anchorMin = new Vector2(0f, 1f);
            statRect.anchorMax = new Vector2(1f, 1f);
            statRect.pivot = new Vector2(0.5f, 1f);
            statRect.anchoredPosition = Vector2.zero;
            // 높이는 VerticalLayoutGroup + ContentSizeFitter가 텍스트 길이에 맞춰 제어한다.
            statText.enableWordWrapping = true;

            // ── 기존 Detail 버튼을 Row 안(스크롤 옆)으로 이동 ──
            if (button != null)
            {
                var buttonRect = button.GetComponent<RectTransform>();
                Undo.SetTransformParent(button.transform, row.transform, "Move Button Into Row");
                var buttonLayoutElement = button.GetComponent<LayoutElement>();
                if (buttonLayoutElement == null)
                    buttonLayoutElement = Undo.AddComponent<LayoutElement>(button);
                buttonLayoutElement.preferredWidth = ButtonSize;
                buttonLayoutElement.preferredHeight = ButtonSize;
                buttonLayoutElement.flexibleWidth = 0f;
                if (buttonRect != null)
                    buttonRect.anchoredPosition = Vector2.zero;
                button.transform.SetAsLastSibling();
            }

            Undo.CollapseUndoOperations(undoGroup);

            Selection.activeGameObject = row;

            string buttonMsg = button != null
                ? "Detail 버튼도 스크롤 옆으로 이동했습니다."
                : "Detail 버튼은 함께 선택하지 않아 이동하지 않았습니다. 필요하면 StatRow 안으로 직접 드래그하세요.";
            EditorUtility.DisplayDialog("Stat Scroll Row 생성 완료",
                $"'{statText.name}'를 '{scrollViewGO.name}' 안으로 감쌌습니다.\n{buttonMsg}\n\n" +
                "다음 단계:\n" +
                "1. StatScrollView 의 LayoutElement.preferredHeight 로 스크롤 표시 높이를 조정하세요.\n" +
                "2. View 인스펙터의 Stat Text 슬롯은 오브젝트만 옮긴 것이므로 참조가 유지됩니다.\n" +
                "3. Detail Popup(오버레이)이 이 레이아웃 그룹 밑에 있다면 LayoutElement의 Ignore Layout을 켜세요.",
                "확인");
        }
    }
}
#endif
