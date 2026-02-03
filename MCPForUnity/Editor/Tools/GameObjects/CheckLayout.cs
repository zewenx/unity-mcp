using System;
using System.Collections.Generic;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Runtime.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MCPForUnity.Editor.Tools.GameObjects
{
    [McpForUnityTool("check_layout")]
    public static class CheckLayout
    {
        public static object HandleCommand(JObject @params)
        {
            if (@params == null)
            {
                return new ErrorResponse("Parameters required.");
            }

            var p = new ToolParams(@params);
            JToken targetToken = @params["target"];
            string searchMethod = p.Get("searchMethod", "by_name");
            bool includeInactive = p.GetBool("includeInactive", false);

            if (targetToken == null)
            {
                return new ErrorResponse("'target' parameter is required.");
            }

            GameObject target = GameObjectLookup.FindByTarget(targetToken, searchMethod, includeInactive);
            if (target == null)
            {
                return new ErrorResponse($"Target GameObject '{targetToken}' not found.");
            }

            try
            {
                var childRects = CollectChildRects(target.transform, includeInactive);
                var overlaps = GetOverlaps(childRects);
                return new SuccessResponse("Checked layout overlaps", overlaps);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckLayout] Error checking layout: {ex.Message}");
                return new ErrorResponse($"Error checking layout: {ex.Message}");
            }
        }

        private static List<ChildRect> CollectChildRects(Transform parent, bool includeInactive)
        {
            var results = new List<ChildRect>();
            if (parent == null)
            {
                return results;
            }

            int childCount = parent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                GameObject childObject = child.gameObject;
                if (!includeInactive && !childObject.activeInHierarchy)
                {
                    continue;
                }

                if (!(child is RectTransform rectTransform))
                {
                    continue;
                }

                if (!LayoutUtility.TryGetScreenRect(rectTransform, out Rect rect))
                {
                    continue;
                }

                results.Add(new ChildRect(childObject, rect));
            }

            return results;
        }

        private static List<object> GetOverlaps(List<ChildRect> childRects)
        {
            var overlaps = new List<object>();
            if (childRects == null || childRects.Count < 2)
            {
                return overlaps;
            }

            int count = childRects.Count;
            for (int i = 0; i < count - 1; i++)
            {
                ChildRect a = childRects[i];
                for (int j = i + 1; j < count; j++)
                {
                    ChildRect b = childRects[j];
                    float overlapArea = LayoutUtility.GetOverlapArea(a.Rect, b.Rect);
                    if (overlapArea <= 0f)
                    {
                        continue;
                    }

                    float overlapRatio = LayoutUtility.GetOverlapRatio(a.Rect, b.Rect);
                    overlaps.Add(new
                    {
                        nodeA = a.InstanceId,
                        nodeB = b.InstanceId,
                        nodeAName = a.Name,
                        nodeBName = b.Name,
                        overlapRatio,
                        overlapArea
                    });
                }
            }

            return overlaps;
        }

        private readonly struct ChildRect
        {
            public ChildRect(GameObject gameObject, Rect rect)
            {
                GameObject = gameObject;
                Rect = rect;
            }

            public GameObject GameObject { get; }
            public Rect Rect { get; }

            public int InstanceId => GameObject != null ? GameObject.GetInstanceID() : 0;
            public string Name => GameObject != null ? GameObject.name : string.Empty;
        }
    }
}
