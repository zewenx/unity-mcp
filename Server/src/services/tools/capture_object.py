"""
Tool for capturing a screenshot of a specific GameObject.
"""

from typing import Annotated, Any, Literal

from fastmcp import Context
from mcp.types import ToolAnnotations

from services.registry import mcp_for_unity_tool
from services.tools import get_unity_instance_from_context
from transport.unity_transport import send_with_unity_instance
from transport.legacy.unity_connection import async_send_command_with_retry


@mcp_for_unity_tool(
    name="capture_object",
    description=(
        "Captures a screenshot of a specific GameObject by calculating its screen-space bounding box. "
        "Useful for verifying the appearance of individual UI elements without clutter."
    ),
    annotations=ToolAnnotations(
        title="Capture Object",
        destructiveHint=False,
    ),
)
async def capture_object(
    ctx: Context,
    target: Annotated[
        str | int, "GameObject to capture - instance ID (preferred), name, or path."
    ],
    search_method: Annotated[
        Literal["by_id", "by_name", "by_path"], "How to find the target GameObject."
    ] = "by_id",
    file_name: Annotated[str | None, "Optional file name for the screenshot."] = None,
    super_size: Annotated[int, "Multiplier for resolution (default 1)."] = 1,
) -> Any:
    """Capture object screenshot from Unity."""
    unity_instance = get_unity_instance_from_context(ctx)

    payload = {
        "target": target,
        "searchMethod": search_method,
        "fileName": file_name,
        "superSize": super_size,
    }

    return await send_with_unity_instance(
        async_send_command_with_retry,
        unity_instance,
        "capture_object",
        payload,
    )
