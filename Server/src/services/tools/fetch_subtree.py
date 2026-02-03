from typing import Annotated, Any, Literal

from fastmcp import Context
from mcp.types import ToolAnnotations

from services.registry import mcp_for_unity_tool
from services.tools import get_unity_instance_from_context
from transport.unity_transport import send_with_unity_instance
from transport.legacy.unity_connection import async_send_command_with_retry


@mcp_for_unity_tool(
    name="fetch_subtree",
    description=(
        "Fetches a recursive tree of GameObjects starting from a root target. "
        "Includes summaries of key UI components (Image, Text, CanvasGroup) "
        "and RectTransform data. Ideal for exploring complex UI hierarchies in a single call."
    ),
    annotations=ToolAnnotations(
        title="Fetch Subtree",
        destructiveHint=False,
    ),
)
async def fetch_subtree(
    ctx: Context,
    target: Annotated[
        str | int, "Root GameObject - instance ID (preferred), name, or path."
    ],
    depth: Annotated[int, "Maximum recursion depth."] = 3,
    search_method: Annotated[
        str, "How to find the root GameObject (by_id, by_name, by_path)."
    ] = "by_id",
    include_inactive: Annotated[
        bool, "Whether to include inactive GameObjects."
    ] = True,
    include_components: Annotated[
        list[str] | None,
        "Optional whitelist of component type names. Only includes nodes matching these components or their descendants.",
    ] = None,
) -> Any:
    unity_instance = get_unity_instance_from_context(ctx)

    payload = {
        "target": target,
        "depth": depth,
        "searchMethod": search_method,
        "includeInactive": include_inactive,
    }

    if include_components is not None:
        payload["include_components"] = include_components

    return await send_with_unity_instance(
        async_send_command_with_retry,
        unity_instance,
        "fetch_subtree",
        payload,
    )
