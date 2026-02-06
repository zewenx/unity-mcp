using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MCPForUnity.Runtime.Helpers
{
    public static class LayoutUtility
    {
        private static readonly Vector3[] s_worldCorners = new Vector3[4];
        private static readonly List<RectMask2D> s_rectMasks = new List<RectMask2D>(8);

        public static bool TryGetScreenRect(RectTransform rt, out Rect rect, Camera overrideCamera = null)
        {
            rect = default;
            if (rt == null)
            {
                return false;
            }

            if (!TryResolveCamera(rt, overrideCamera, out Camera camera, out bool allowNullCamera))
            {
                return false;
            }

            rt.GetWorldCorners(s_worldCorners);

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;

            for (int i = 0; i < s_worldCorners.Length; i++)
            {
                Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(allowNullCamera ? null : camera, s_worldCorners[i]);
                minX = Mathf.Min(minX, screenPoint.x);
                minY = Mathf.Min(minY, screenPoint.y);
                maxX = Mathf.Max(maxX, screenPoint.x);
                maxY = Mathf.Max(maxY, screenPoint.y);
            }

            rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        public static float GetOverlapRatio(Rect a, Rect b)
        {
            float area = Mathf.Abs(a.width * a.height);
            if (area <= 0f)
            {
                return 0f;
            }

            float overlap = GetOverlapArea(a, b);
            return Mathf.Clamp01(overlap / area);
        }

        public static float GetOverlapArea(Rect a, Rect b)
        {
            float minX = Mathf.Max(a.xMin, b.xMin);
            float minY = Mathf.Max(a.yMin, b.yMin);
            float maxX = Mathf.Min(a.xMax, b.xMax);
            float maxY = Mathf.Min(a.yMax, b.yMax);

            float width = maxX - minX;
            float height = maxY - minY;
            if (width <= 0f || height <= 0f)
            {
                return 0f;
            }

            return width * height;
        }

        public static bool TryGetScreenClipRect(RectTransform rt, out Rect clipRect, Camera overrideCamera = null)
        {
            clipRect = default;
            if (rt == null)
            {
                return false;
            }

            s_rectMasks.Clear();
            rt.GetComponentsInParent(true, s_rectMasks);

            if (s_rectMasks.Count == 0)
            {
                if (rt.GetComponentInParent<Mask>(true) != null)
                {
                    return false;
                }

                return false;
            }

            bool hasRect = false;
            Rect combined = default;
            for (int i = 0; i < s_rectMasks.Count; i++)
            {
                RectMask2D rectMask = s_rectMasks[i];
                if (rectMask == null || !rectMask.isActiveAndEnabled)
                {
                    continue;
                }

                if (!TryGetScreenRect(rectMask.rectTransform, out Rect maskRect, overrideCamera))
                {
                    continue;
                }

                if (!hasRect)
                {
                    combined = maskRect;
                    hasRect = true;
                }
                else
                {
                    combined = Intersect(combined, maskRect);
                }
            }

            if (!hasRect)
            {
                return false;
            }

            clipRect = combined;
            return true;
        }

        private static bool TryResolveCamera(RectTransform rt, Camera overrideCamera, out Camera camera, out bool allowNullCamera)
        {
            camera = overrideCamera;
            allowNullCamera = false;

            Canvas canvas = rt.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    allowNullCamera = true;
                    return true;
                }

                if (camera == null)
                {
                    camera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
                }
            }
            else
            {
                camera = camera ?? Camera.main;
            }

            return camera != null;
        }

        private static Rect Intersect(Rect a, Rect b)
        {
            float minX = Mathf.Max(a.xMin, b.xMin);
            float minY = Mathf.Max(a.yMin, b.yMin);
            float maxX = Mathf.Min(a.xMax, b.xMax);
            float maxY = Mathf.Min(a.yMax, b.yMax);
            if (maxX <= minX || maxY <= minY)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }
    }
}
