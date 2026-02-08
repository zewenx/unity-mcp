from typing import Annotated, Any

from fastmcp import Context
from mcp.types import ToolAnnotations

from services.registry import mcp_for_unity_tool
from services.tools import get_unity_instance_from_context
from services.tools.utils import coerce_bool, coerce_int
from transport.unity_transport import send_with_unity_instance
from transport.legacy.unity_connection import async_send_command_with_retry


@mcp_for_unity_tool(
    description="Clicks a uGUI component in Play Mode via Unity EventSystem. Provide exactly one locator: instance_id or hierarchy_path.",
    annotations=ToolAnnotations(
        title="Click Component",
        destructiveHint=False,
    ),
)
async def click_component(
    ctx: Context,
    instance_id: Annotated[int | str, "Target GameObject instance ID."] | None = None,
    hierarchy_path: Annotated[str, "Target hierarchy path, e.g. Canvas/Panel/Button."]
    | None = None,
    strict: Annotated[
        bool | str, "If true, fail when raycast top hit is outside the target subtree."
    ]
    | None = None,
) -> dict[str, Any]:
    has_instance_id = instance_id is not None
    has_hierarchy_path = bool(hierarchy_path and hierarchy_path.strip())
    if has_instance_id == has_hierarchy_path:
        return {
            "success": False,
            "message": "Provide exactly one of 'instance_id' or 'hierarchy_path'.",
        }

    unity_instance = get_unity_instance_from_context(ctx)

    params: dict[str, Any] = {
        "strict": coerce_bool(strict, default=True),
    }

    if has_instance_id:
        normalized_id = coerce_int(instance_id, default=None)
        if normalized_id is None:
            return {
                "success": False,
                "message": "'instance_id' must be an integer.",
            }
        params["instanceId"] = normalized_id
    else:
        params["hierarchyPath"] = hierarchy_path.strip()

    response = await send_with_unity_instance(
        async_send_command_with_retry,
        unity_instance,
        "click_component",
        params,
    )

    if isinstance(response, dict):
        return response
    return {"success": False, "message": str(response)}
