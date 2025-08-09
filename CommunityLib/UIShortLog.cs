using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommunityLib
{
    public static class UIShortLog
    {
        // Call this with the parent UI panel Transform you’re patching.
        public static void DumpPanelSummary(Transform panelRoot)
        {
            if (panelRoot == null)
            {
                Console.WriteLine("[UIShortLog] panelRoot == null");
                return;
            }

            Console.WriteLine($"=== UI Short Dump @ {DateTime.Now:HH:mm:ss}  root={panelRoot.name} ===");

            // Walk all RectTransforms under panelRoot (including itself)
            foreach (var rt in panelRoot.GetComponentsInChildren<RectTransform>(true))
            {
                var path = MakeStablePath(rt, panelRoot);
                bool isCandidate = LooksLikeListContainer(rt, out var why);

                // Build compact tag list (only what helps identify targets)
                var tags = BuildTags(rt);

                // Optional: a nearby label to orient yourself (no TMPro hard ref)
                string label = GetOwnLabel(rt);
                if (string.IsNullOrEmpty(label))
                    label = GetOwnLabel(rt.parent); // try parent, often holds the section header

                var line = $"{(isCandidate ? "* " : "  ")}{path}  children={rt.childCount}  {tags}";
                if (!string.IsNullOrEmpty(label))
                    line += $"  LBL=\"{Trunc(label, 50)}\"";
                if (isCandidate)
                    line += $"  // {why}";
                Console.WriteLine(line);
            }

            Console.WriteLine($"=== end ===");
        }

        private static string BuildTags(Transform t)
        {
            var tags = new List<string>(6);

            if (t.GetComponent<VerticalLayoutGroup>()) tags.Add("[VLG]");
            if (t.GetComponent<HorizontalLayoutGroup>()) tags.Add("[HLG]");
            if (t.GetComponent<GridLayoutGroup>()) tags.Add("[GRID]");
            if (t.GetComponent<ContentSizeFitter>()) tags.Add("[CSF]");
            if (t.GetComponent<ScrollRect>()) tags.Add("[SR]");
            if (t.GetComponent<RectMask2D>() || t.GetComponent<Mask>()) tags.Add("[MASK]");
            if (t.GetComponent<Scrollbar>()) tags.Add("[SB]");

            // Active flags help when panels have pooled/inactive segments
            if (!t.gameObject.activeInHierarchy) tags.Add("[inactive]");

            return string.Join("", tags);
        }

        // Heuristic: a list container is a layouted rect with several children, not already a ScrollRect content
        private static bool LooksLikeListContainer(RectTransform t, out string why)
        {
            why = null;
            if (!t.gameObject.activeInHierarchy) return false;

            bool hasLayout = t.GetComponent<VerticalLayoutGroup>() ||
                             t.GetComponent<HorizontalLayoutGroup>() ||
                             t.GetComponent<GridLayoutGroup>();
            if (!hasLayout) return false;

            if (t.childCount < 3) return false; // tune if needed

            var sr = t.GetComponentInParent<ScrollRect>(true);
            if (sr != null && sr.content == t) return false; // already scroll content

            why = $"layout=yes, kids={t.childCount}, srContent=no";
            return true;
        }

        // Stable-ish path: Name[indexAmongSameName] from panelRoot → t
        private static string MakeStablePath(Transform t, Transform stopAt)
        {
            var segs = new List<string>(8);
            var cur = t;
            while (cur != null && cur != stopAt.parent)
            {
                var p = cur.parent;
                int idxByName = 0;
                if (p != null)
                {
                    int k = 0;
                    for (int i = 0; i < p.childCount; i++)
                    {
                        var c = p.GetChild(i);
                        if (c.name == cur.name)
                        {
                            if (c == cur) { idxByName = k; break; }
                            k++;
                        }
                    }
                }
                segs.Add($"{cur.name}[{idxByName}]");
                if (cur == stopAt) break;
                cur = p;
            }
            segs.Reverse();
            return string.Join("/", segs);
        }

        private static string GetOwnLabel(Transform t)
        {
            if (t == null) return null;

            var ut = t.GetComponent<Text>();
            if (ut && !string.IsNullOrWhiteSpace(ut.text)) return ut.text.Trim();

            // No hard dependency on TMPro: query by name + reflection
            var tmp = t.GetComponent("TMP_Text");
            if (tmp != null)
            {
                var prop = tmp.GetType().GetProperty("text");
                if (prop != null)
                {
                    var s = prop.GetValue(tmp) as string;
                    if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                }
            }
            return null;
        }

        private static string Trunc(string s, int max)
            => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
