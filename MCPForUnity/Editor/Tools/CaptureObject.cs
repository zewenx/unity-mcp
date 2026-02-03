using System;
using System.Linq;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Runtime.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MCPForUnity.Editor.Tools
{
    /// <summary>
    /// Tool for capturing a screenshot of a specific GameObject.
    /// Automatically calculates the screen-space bounding box for UI elements.
    /// </summary>
    [McpForUnityTool("capture_object")]
    public static class CaptureObject
    {
        public static object HandleCommand(JObject @params)
        {
            if (@params == null) return new ErrorResponse("Parameters required.");

            var p = new ToolParams(@params);
            JToken targetToken = @params["target"];
            string searchMethod = p.Get("searchMethod", "by_id");
            string fileName = p.Get("fileName");
            int superSize = p.GetInt("superSize", 1) ?? 1;

            if (targetToken == null) return new ErrorResponse("'target' parameter is required.");

            GameObject target = GameObjectLookup.FindByTarget(targetToken, searchMethod, true);
            if (target == null) return new ErrorResponse($"Target GameObject '{targetToken}' not found.");

            try
            {
                var rectTransform = target.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    return new ErrorResponse("Target must have a RectTransform for area capture.");
                }

                Canvas canvas = target.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    return new ErrorResponse("Target must be part of a Canvas hierarchy.");
                }

                Camera cam = canvas.worldCamera;
                if (cam == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    return new ErrorResponse("Canvas worldCamera is missing for non-overlay canvas.");
                }

                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                Vector2 min = Vector2.one * float.MaxValue;
                Vector2 max = Vector2.one * float.MinValue;

                foreach (var corner in corners)
                {
                    Vector2 screenPoint;
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        screenPoint = corner;
                    }
                    else
                    {
                        screenPoint = RectTransformUtility.WorldToScreenPoint(cam, corner);
                    }
                    min = Vector2.Min(min, screenPoint);
                    max = Vector2.Max(max, screenPoint);
                }

                Rect screenRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
                
                if (cam == null)
                {
                    cam = Camera.main ?? UnityEngine.Object.FindObjectsOfType<Camera>().FirstOrDefault();
                }

                if (cam == null)
                {
                    return new ErrorResponse("No camera found for rendering the screenshot.");
                }

                var result = ScreenshotUtility.CaptureRectFromCameraToAssetsFolder(cam, screenRect, fileName, superSize);

                return new SuccessResponse("Captured object screenshot", new
                {
                    fullPath = result.FullPath,
                    assetsPath = result.AssetsRelativePath,
                    screenRect = new { x = screenRect.x, y = screenRect.y, width = screenRect.width, height = screenRect.height }
                });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Error capturing object: {ex.Message}");
            }
        }
    }
}
