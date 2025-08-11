using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommunityLib
{
    public static class UIScrollWrap
    {
        // marker so we don’t double-wrap
        private sealed class WrappedScrollMarker : MonoBehaviour { }

        private static int _frameSeen = -1;

        private static readonly List<string> _wrappedThisFrame = new List<string>();

        /// <summary>
        /// Wraps an existing panel (e.g., "Subsettlements") in a ScrollView *without* changing its GameObject.
        /// The original panel becomes the ScrollRect.content (keeps its VLG/CSF). A new wrapper with the same
        /// name takes its place in the parent. LayoutElement is copied so the parent LayoutGroup sizes it the same.
        /// </summary>
        public static bool WrapPanelAtRawPath(
            Transform screenRoot,
            string rawPath,
            bool vertical = true,
            bool horizontal = false,
            bool copyBackgroundToWrapper = false,     // if true, moves Image from content to wrapper so it doesn't scroll
            bool preferFlexibleHeight = false,        // if true, wrapper gets flexibleHeight=1 instead of locking preferredHeight
            float scrollbarWidth = 12f,
            float scrollbarMargin = 2f)
        {
            if (!screenRoot)
            {
                Console.WriteLine("[UIScrollWrap] screenRoot is null.");
                return false;
            }

            int frameCount = Time.frameCount;
            /*if (frameCount != _frameSeen)
            {
                _wrappedThisFrame.Clear();
                _frameSeen = frameCount;
            }
            else*/ if (_wrapped.Contains($"{screenRoot.GetInstanceID()}|{rawPath}"))
            {
                Console.WriteLine($"[UIScrollWrap] '{screenRoot.GetInstanceID()}|{rawPath}' is alrady wrapped. Skipping.");
                return false;
            }

            var target = screenRoot.Find(rawPath) as RectTransform;
            if (!target)
            {
                Console.WriteLine($"[UIScrollWrap] path not found: {rawPath}");
                return false;
            }

            // Already wrapped?
            var parentSR = target.parent ? target.parent.GetComponent<ScrollRect>() : null;
            if (parentSR && parentSR.GetComponent<WrappedScrollMarker>())
            {
                _wrapped.Add($"{screenRoot.GetInstanceID()}|{rawPath}");
                Console.WriteLine($"[UIScrollWrap] '{screenRoot.GetInstanceID()}|{rawPath}' is alrady wrapped. Skipping.");
                return true;
            }

            try
            {
                _wrapped.Add($"{screenRoot.GetInstanceID()}|{rawPath}");
                PerformWrapping(target, vertical, horizontal, copyBackgroundToWrapper, preferFlexibleHeight, scrollbarWidth, scrollbarMargin);
                Console.WriteLine($"[UIScrollWrap] wrapped: {rawPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UIScrollWrap] ERROR wrapping {screenRoot.GetInstanceID()}|{rawPath}: {ex}");
                return false;
            }
        }

        private static void DoWrap(
            RectTransform contentPanel,
            bool vertical,
            bool horizontal,
            bool copyBgToWrapper,
            bool preferFlexibleHeight,
            float sbWidth,
            float sbMargin)
        {
            var parent = contentPanel.parent as RectTransform;
            if (!parent) throw new InvalidOperationException("Target has no RectTransform parent.");

            int sibling = contentPanel.GetSiblingIndex();
            string originalName = contentPanel.gameObject.name;

            // Snapshot size & any LayoutElement on the original (this is what the parent LayoutGroup used to see)
            var originalSize = contentPanel.rect.size;
            var originalLE = contentPanel.GetComponent<LayoutElement>();

            // 1) Create wrapper in the same parent/slot, with the SAME name as original (path stability)
            var wrapperGO = new GameObject(originalName, typeof(RectTransform));
            var wrapper = (RectTransform)wrapperGO.transform;
            wrapper.SetParent(parent, false);
            wrapper.SetSiblingIndex(sibling);
            wrapper.gameObject.AddComponent<WrappedScrollMarker>();

            // Optional: copy anchors/pivot from original (mostly ignored by LayoutGroups, but harmless)
            wrapper.anchorMin = contentPanel.anchorMin;
            wrapper.anchorMax = contentPanel.anchorMax;
            wrapper.pivot = contentPanel.pivot;
            wrapper.anchoredPosition = contentPanel.anchoredPosition;
            wrapper.sizeDelta = contentPanel.sizeDelta;

            // 2) Rename original so there’s no name collision under wrapper; it becomes the content
            contentPanel.gameObject.name = originalName + "_Content";

            // 3) Build ScrollRect on wrapper
            var scrollRect = wrapper.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 30f;

            scrollRect.horizontalScrollbar = null;
            scrollRect.verticalScrollbar = null;
            foreach (var sb in wrapper.GetComponentsInChildren<Scrollbar>(true))
            {
                UnityEngine.Object.Destroy(sb.gameObject);
            }

            // 4) LayoutElement on wrapper so parent LayoutGroup sizes it as before
            var le = wrapper.gameObject.AddComponent<LayoutElement>();
            if (originalLE != null)
            {
                CopyLayoutElement(originalLE, le);
                // The original LE is no longer used by the parent; you can keep or remove it.
                // Keeping it doesn't hurt, but to avoid confusion:
                // UnityEngine.Object.Destroy(originalLE);
            }
            else
            {
                if (preferFlexibleHeight)
                {
                    le.flexibleHeight = 1f; // fill available height in parent VLG
                }
                else
                {
                    le.preferredHeight = Mathf.Max(0f, originalSize.y);
                }
                // Width typically controlled by parent; make it cooperative:
                le.flexibleWidth = 1f;
            }

            // 5) Create Viewport under wrapper (fills wrapper), with RectMask2D
            var viewport = NewChild(wrapper, "Viewport");
            viewport.anchorMin = new Vector2(0, 0);
            viewport.anchorMax = new Vector2(1, 1);
            viewport.pivot = new Vector2(0, 1);
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.anchoredPosition = Vector2.zero;
            viewport.sizeDelta = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();

            // 6) Reparent original panel under Viewport, set as ScrollRect.content
            contentPanel.SetParent(viewport, false);
            // As content, we want top-left growth and full width
            contentPanel.anchorMin = new Vector2(0, 1);
            contentPanel.anchorMax = new Vector2(1, 1);
            contentPanel.pivot = new Vector2(0, 1);
            contentPanel.anchoredPosition = Vector2.zero;
            contentPanel.sizeDelta = Vector2.zero;

            EnsureContentAutoSize(contentPanel);

            scrollRect.viewport = viewport;
            scrollRect.content = contentPanel;

            // 7) Optionally move a background Image to wrapper so it doesn't scroll
            if (copyBgToWrapper)
            {
                var bg = contentPanel.GetComponent<Image>();
                if (bg)
                {
                    var wrapperBg = wrapper.gameObject.AddComponent<Image>();
                    wrapperBg.sprite = bg.sprite;
                    wrapperBg.type = bg.type;
                    wrapperBg.color = bg.color;
                    // Make the content background transparent so only wrapper bg shows
                    bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0f);
                }
            }

            // 8) Add vertical scrollbar (optional) that auto-hides and shrinks viewport when visible
            if (vertical)
            {
                var sb = CreateScrollbar(wrapper, sbWidth, sbMargin);
                scrollRect.verticalScrollbar = sb;
                scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                scrollRect.verticalScrollbarSpacing = sbMargin;
            }

            // 9) Final rebuild so sizes are valid immediately
            LayoutRebuilder.ForceRebuildLayoutImmediate(wrapper);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }

        private static void CopyLayoutElement(LayoutElement src, LayoutElement dst)
        {
            dst.ignoreLayout = src.ignoreLayout;
            dst.minWidth = src.minWidth;
            dst.minHeight = src.minHeight;
            dst.preferredWidth = src.preferredWidth;
            dst.preferredHeight = src.preferredHeight;
            dst.flexibleWidth = src.flexibleWidth;
            dst.flexibleHeight = src.flexibleHeight;
            dst.layoutPriority = src.layoutPriority;
        }

        private static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Scrollbar CreateScrollbar(RectTransform wrapper, float width, float margin)
        {
            var sbGo = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            sbGo.transform.SetParent(wrapper, false);

            var rt = (RectTransform)sbGo.transform;
            // dock right
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.offsetMin = new Vector2(-(width + margin), 0);
            rt.offsetMax = new Vector2(-margin, 0);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var track = sbGo.GetComponent<Image>();
            track.color = new Color(0, 0, 0, 0.35f);

            var sb = sbGo.GetComponent<Scrollbar>();
            sb.direction = Scrollbar.Direction.BottomToTop;

            var sliding = new GameObject("SlidingArea", typeof(RectTransform));
            sliding.transform.SetParent(rt, false);
            var srt = (RectTransform)sliding.transform;
            srt.anchorMin = new Vector2(0, 0);
            srt.anchorMax = new Vector2(1, 1);
            srt.offsetMin = new Vector2(1, 1);
            srt.offsetMax = new Vector2(-1, -1);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(srt, false);
            var hrt = (RectTransform)handle.transform;
            hrt.anchorMin = new Vector2(0, 0);
            hrt.anchorMax = new Vector2(1, 0.2f);
            hrt.offsetMin = Vector2.zero;
            hrt.offsetMax = Vector2.zero;

            var handleImg = handle.GetComponent<Image>();
            handleImg.color = new Color(1, 1, 1, 0.9f);

            sb.targetGraphic = handleImg;
            sb.handleRect = hrt;
            return sb;
        }

        private static void EnsureContentAutoSize(RectTransform content)
        {
            // Ensure a VerticalLayoutGroup exists (you already had one)
            var vlg = content.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
            // Keep whatever spacing/padding/alignment it already had; we’re not touching those.

            var fitter = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

    }
}
