using System;
using UnityEngine;

namespace MCPForUnity.Runtime.Helpers
{
    public static class LayoutUtility
    {
        public static bool TryGetScreenRect(RectTransform rectTransform, out Rect rect, Camera overrideCamera = null)
        {
            rect = default;
            if (rectTransform == null)
            {
                return false;
            }

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            Camera cam = overrideCamera;
            bool isOverlay = false;

            if (canvas != null)
            {
                isOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;
                if (!isOverlay)
                {
                    cam = cam != null ? cam : canvas.worldCamera;
                }
            }

            if (!isOverlay && cam == null)
            {
                cam = Camera.main;
            }

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector2 min = Vector2.one * float.MaxValue;
            Vector2 max = Vector2.one * float.MinValue;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 screenPoint;
                if (isOverlay)
                {
                    screenPoint = corners[i];
                }
                else
                {
                    screenPoint = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
                }

                min = Vector2.Min(min, screenPoint);
                max = Vector2.Max(max, screenPoint);
            }

            float width = max.x - min.x;
            float height = max.y - min.y;
            if (width <= 0f || height <= 0f)
            {
                return false;
            }

            rect = new Rect(min.x, min.y, width, height);
            return true;
        }

        public static float GetOverlapArea(Rect a, Rect b)
        {
            float xMin = Mathf.Max(a.xMin, b.xMin);
            float yMin = Mathf.Max(a.yMin, b.yMin);
            float xMax = Mathf.Min(a.xMax, b.xMax);
            float yMax = Mathf.Min(a.yMax, b.yMax);

            float w = xMax - xMin;
            float h = yMax - yMin;
            if (w <= 0f || h <= 0f)
            {
                return 0f;
            }

            return w * h;
        }

        public static float GetOverlapRatio(Rect a, Rect b)
        {
            float area = GetOverlapArea(a, b);
            float denom = a.width * a.height;
            if (denom <= 0f)
            {
                return 0f;
            }

            return area / denom;
        }
    }
}
