using System;
using System.Collections.Generic;
using System.Linq;
using MCPForUnity.Editor.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MCPForUnity.Editor.Tools.GameObjects
{
    /// <summary>
    /// Tool for fetching a recursive tree of GameObjects with key component summaries.
    /// Ideal for understanding complex UI structures in one go.
    /// </summary>
    [McpForUnityTool("fetch_subtree")]
    public static class FetchSubtree
    {
        public static object HandleCommand(JObject @params)
        {
            if (@params == null) return new ErrorResponse("Parameters required.");

            var p = new ToolParams(@params);
            JToken targetToken = @params["target"];
            string searchMethod = p.Get("searchMethod", "by_name");
            int maxDepth = p.GetInt("depth", 3) ?? 3;
            bool includeInactive = p.GetBool("includeInactive", true);

            if (targetToken == null) return new ErrorResponse("'target' parameter is required.");

            GameObject root = GameObjectLookup.FindByTarget(targetToken, searchMethod, includeInactive);
            if (root == null) return new ErrorResponse($"Root GameObject '{targetToken}' not found.");

            try
            {
                var includeComponents = new List<string>();
                var rawInclude = p.GetRaw("include_components") as JArray;
                if (rawInclude != null)
                {
                    includeComponents = rawInclude.Select(x => x.ToString()).ToList();
                }

                bool hasFilter = includeComponents.Count > 0;
                return new SuccessResponse("Fetched subtree", GetNodeData(root, 0, maxDepth, includeInactive, hasFilter ? includeComponents : null, out _));
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Error fetching subtree: {ex.Message}");
            }
        }

        private static object GetNodeData(GameObject go, int currentDepth, int maxDepth, bool includeInactive, List<string> includeComponents, out bool matches)
        {
            var transform = go.transform;

            bool nodeMatches = false;
            if (includeComponents != null)
            {
                for (int i = 0; i < includeComponents.Count; i++)
                {
                    if (go.GetComponent(includeComponents[i]) != null)
                    {
                        nodeMatches = true;
                        break;
                    }
                }
            }

            var children = new List<object>();
            bool anyChildMatches = false;
            if (currentDepth < maxDepth)
            {
                foreach (Transform child in transform)
                {
                    if (!includeInactive && !child.gameObject.activeSelf) continue;

                    var childData = GetNodeData(child.gameObject, currentDepth + 1, maxDepth, includeInactive, includeComponents, out bool childMatches);
                    if (childData != null)
                    {
                        children.Add(childData);
                    }

                    if (childMatches)
                    {
                        anyChildMatches = true;
                    }
                }
            }

            matches = includeComponents == null ? true : (nodeMatches || anyChildMatches);
            if (includeComponents != null && !matches)
            {
                return null;
            }

            bool minimal = includeComponents != null && !nodeMatches;
            var data = new Dictionary<string, object>
            {
                { "name", go.name },
                { "instanceID", go.GetInstanceID() },
                { "activeSelf", go.activeSelf },
                { "activeInHierarchy", go.activeInHierarchy }
            };

            if (includeComponents != null)
            {
                data["matched"] = nodeMatches;
            }

            if (!minimal)
            {
                data["layer"] = LayerMask.LayerToName(go.layer);
                data["tag"] = go.tag;

                if (transform is RectTransform rect)
                {
                    data["rectTransform"] = new
                    {
                        anchoredPosition = new { x = rect.anchoredPosition.x, y = rect.anchoredPosition.y },
                        sizeDelta = new { x = rect.sizeDelta.x, y = rect.sizeDelta.y },
                        anchorMin = new { x = rect.anchorMin.x, y = rect.anchorMin.y },
                        anchorMax = new { x = rect.anchorMax.x, y = rect.anchorMax.y },
                        pivot = new { x = rect.pivot.x, y = rect.pivot.y }
                    };
                }
                else
                {
                    data["transform"] = new
                    {
                        localPosition = new { x = transform.localPosition.x, y = transform.localPosition.y, z = transform.localPosition.z },
                        localEulerAngles = new { x = transform.localEulerAngles.x, y = transform.localEulerAngles.y, z = transform.localEulerAngles.z },
                        localScale = new { x = transform.localScale.x, y = transform.localScale.y, z = transform.localScale.z }
                    };
                }

                var components = new List<object>();
                var image = go.GetComponent<Image>();
                if (image != null) components.Add(GameObjectSerializer.GetComponentData(image));

                var text = go.GetComponent<Text>();
                if (text != null) components.Add(GameObjectSerializer.GetComponentData(text));

                var canvasGroup = go.GetComponent<CanvasGroup>();
                if (canvasGroup != null) components.Add(GameObjectSerializer.GetComponentData(canvasGroup));

                if (components.Count > 0)
                {
                    data["components"] = components;
                }
            }

            if (currentDepth < maxDepth)
            {
                if (children.Count > 0)
                {
                    data["children"] = children;
                }
            }
            else if (transform.childCount > 0)
            {
                data["childCount"] = transform.childCount;
                data["hasMoreChildren"] = true;
            }

            return data;
        }
    }
}
