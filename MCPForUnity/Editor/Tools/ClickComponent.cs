using System.Collections.Generic;
using System.Linq;
using MCPForUnity.Editor.Helpers;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MCPForUnity.Editor.Tools
{
    [McpForUnityTool("click_component", AutoRegister = false)]
    public static class ClickComponent
    {
        public static object HandleCommand(JObject @params)
        {
            if (@params == null)
            {
                return new ErrorResponse("Parameters cannot be null.");
            }

            if (!EditorApplication.isPlaying)
            {
                return new ErrorResponse("click_component requires Play Mode.");
            }

            var p = new ToolParams(@params);
            int? instanceId = p.GetInt("instanceId");
            string hierarchyPath = p.Get("hierarchyPath");
            bool strict = p.GetBool("strict", true);

            bool hasInstanceId = instanceId.HasValue;
            bool hasHierarchyPath = !string.IsNullOrWhiteSpace(hierarchyPath);

            if (hasInstanceId == hasHierarchyPath)
            {
                return new ErrorResponse("Provide exactly one of 'instance_id' or 'hierarchy_path'.");
            }

            GameObject target = hasInstanceId
                ? GameObjectLookup.FindById(instanceId.Value)
                : FindByHierarchyPath(hierarchyPath);

            if (target == null)
            {
                return new ErrorResponse("Target GameObject not found.");
            }

            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return new ErrorResponse("Target is not a uGUI object (missing RectTransform).");
            }

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return new ErrorResponse("No active EventSystem found.");
            }

            var screenPosition = ResolveScreenPosition(target, rectTransform);
            var pointerData = new PointerEventData(eventSystem)
            {
                position = screenPosition,
                button = PointerEventData.InputButton.Left,
                clickCount = 1
            };

            var raycastResults = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, raycastResults);

            bool usedRaycastFallback = raycastResults.Count == 0;
            var hit = usedRaycastFallback ? target : raycastResults[0].gameObject;
            bool hitWithinTarget = hit == target || hit.transform.IsChildOf(target.transform);
            if (strict && !hitWithinTarget)
            {
                return new ErrorResponse(
                    "Strict mode: raycast hit a different object.",
                    new
                    {
                        target = BuildObjectRef(target),
                        hit = BuildObjectRef(hit),
                        raycastHits = raycastResults.Take(5).Select(r => BuildObjectRef(r.gameObject)).ToArray()
                    }
                );
            }

            eventSystem.SetSelectedGameObject(hit, pointerData);

            var downHandler = ExecuteEvents.ExecuteHierarchy(hit, pointerData, ExecuteEvents.pointerDownHandler);
            var upHandler = ExecuteEvents.ExecuteHierarchy(hit, pointerData, ExecuteEvents.pointerUpHandler);
            var clickHandler = ExecuteEvents.ExecuteHierarchy(hit, pointerData, ExecuteEvents.pointerClickHandler);

            return new SuccessResponse(
                "Component click dispatched.",
                new
                {
                    target = BuildObjectRef(target),
                    hit = BuildObjectRef(hit),
                    strict,
                    raycastFallbackUsed = usedRaycastFallback,
                    eventsFired = new
                    {
                        pointerDown = downHandler != null,
                        pointerUp = upHandler != null,
                        pointerClick = clickHandler != null
                    },
                    handlers = new
                    {
                        pointerDown = BuildObjectRef(downHandler),
                        pointerUp = BuildObjectRef(upHandler),
                        pointerClick = BuildObjectRef(clickHandler)
                    }
                }
            );
        }

        private static GameObject FindByHierarchyPath(string hierarchyPath)
        {
            var ids = GameObjectLookup.SearchGameObjects("by_path", hierarchyPath, includeInactive: true, maxResults: 1);
            if (ids == null || ids.Count == 0)
            {
                return null;
            }
            return GameObjectLookup.FindById(ids[0]);
        }

        private static Vector2 ResolveScreenPosition(GameObject target, RectTransform rectTransform)
        {
            var canvas = target.GetComponentInParent<Canvas>();
            Camera camera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                camera = canvas.worldCamera;
            }

            var worldPoint = rectTransform.TransformPoint(rectTransform.rect.center);
            return RectTransformUtility.WorldToScreenPoint(camera, worldPoint);
        }

        private static object BuildObjectRef(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            return new
            {
                name = gameObject.name,
                instanceID = gameObject.GetInstanceID(),
                hierarchyPath = GetHierarchyPath(gameObject.transform)
            };
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            var names = new List<string>();
            var current = transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }
            names.Reverse();
            return string.Join("/", names);
        }
    }
}
