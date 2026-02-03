from typing import Annotated, Any, Literal

from fastmcp import Context
from mcp.types import ToolAnnotations

from services.registry import mcp_for_unity_tool
from services.tools import get_unity_instance_from_context
from transport.unity_transport import send_with_unity_instance
from transport.legacy.unity_connection import async_send_command_with_retry


@mcp_for_unity_tool(
    name="check_layout",
    description=(
        "Detects screen-space AABB overlaps between direct child RectTransforms of a target. "
        "Useful for validating UI layout quality and catching visual bugs."
    ),
    annotations=ToolAnnotations(
        title="Check Layout Overlaps",
        destructiveHint=False,
    ),
)
async def check_layout(
    ctx: Context,
    target: Annotated[
        str | int, "Target GameObject - instance ID (preferred), name, or path."
    ],
    search_method: Annotated[
        str, "How to find the target GameObject (by_id, by_name, by_path)."
    ] = "by_id",
    include_inactive: Annotated[
        bool, "Whether to include inactive children in the check."
    ] = False,
) -> Any:
    unity_instance = get_unity_instance_from_context(ctx)

    payload = {
        "target": target,
        "searchMethod": search_method,
        "includeInactive": include_inactive,
    }

    return await send_with_unity_instance(
        async_send_command_with_retry,
        unity_instance,
        "check_layout",
        payload,
    )
