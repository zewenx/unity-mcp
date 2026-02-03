"""
Tool for modifying multiple GameObjects in a single operation.
Expands into a batch_execute call for efficiency.
"""

from typing import Annotated, Any

from fastmcp import Context
from mcp.types import ToolAnnotations

from services.registry import mcp_for_unity_tool
from services.tools import get_unity_instance_from_context
from transport.unity_transport import send_with_unity_instance
from transport.legacy.unity_connection import async_send_command_with_retry


@mcp_for_unity_tool(
    name="modify_batch",
    description=(
        "Modifies multiple GameObjects with the same set of properties in a single call. "
        "Dramatically more efficient than individual modify calls. "
        "Example: setting color to red for a list of button IDs."
    ),
    annotations=ToolAnnotations(
        title="Modify Batch",
        destructiveHint=False,
    ),
)
async def modify_batch(
    ctx: Context,
    targets: Annotated[
        list[str | int], "List of target GameObject IDs, names, or paths."
    ],
    properties: Annotated[
        dict[str, Any],
        "Properties to apply to all targets. Example: {'active': false} or {'componentProperties': {...}}",
    ],
) -> Any:
    """Expand modify_batch into a batch_execute payload."""
    unity_instance = get_unity_instance_from_context(ctx)

    if not targets:
        return {"success": False, "message": "No targets provided."}

    commands = []
    for target in targets:
        cmd_params = properties.copy()
        cmd_params["target"] = target
        cmd_params["action"] = "modify"

        commands.append({"tool": "manage_gameobject", "params": cmd_params})

    payload = {"commands": commands, "failFast": False}

    return await send_with_unity_instance(
        async_send_command_with_retry,
        unity_instance,
        "batch_execute",
        payload,
    )
