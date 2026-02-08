"""
Monkey patch to fix JSON Schema for Google Function Calling compatibility.

Apply this patch AFTER FastMCP is imported but BEFORE any tools are registered.

Problem: Google Function Calling requires that when using anyOf, it must be the ONLY
field in the schema object. No sibling fields like 'description', 'title', etc. are
allowed alongside anyOf. FastMCP/Pydantic generates schemas that violate this rule.

Solution: This patch modifies the compress_schema function to also fix anyOf schemas
by moving sibling fields into each anyOf element.
"""

from typing import Any


def _fix_google_anyof(schema: Any) -> Any:
    """
    Recursively fix JSON Schema to comply with Google Function Calling requirements.

    Google Function Calling has stricter requirements than standard JSON Schema:
    - When using anyOf, it must be the ONLY field in the schema object
    - No sibling fields like 'description', 'title', etc. allowed alongside anyOf

    This function moves sibling fields into each anyOf element.
    """
    if not isinstance(schema, dict):
        return schema

    # Check if this node has anyOf with sibling fields
    if "anyOf" in schema:
        any_of = schema.get("anyOf", [])
        sibling_fields = {k: v for k, v in schema.items() if k != "anyOf"}

        # If there are sibling fields alongside anyOf, move them into each element
        if sibling_fields and isinstance(any_of, list):
            for element in any_of:
                if isinstance(element, dict):
                    # Add sibling fields to each anyOf element (if not already present)
                    for key, value in sibling_fields.items():
                        if key not in element:
                            element[key] = value

            # Return only anyOf (Google Function Calling requirement)
            return {"anyOf": any_of}

    # Recursively process all nested schemas
    for key, value in list(schema.items()):
        if isinstance(value, dict):
            schema[key] = _fix_google_anyof(value)
        elif isinstance(value, list):
            schema[key] = [
                _fix_google_anyof(item) if isinstance(item, dict) else item
                for item in value
            ]

    return schema


# Track if patch has been applied
_patch_applied = False


def apply_schema_patch():
    """
    Apply the monkey patch to fastmcp.utilities.json_schema.compress_schema.

    Must be called AFTER FastMCP is imported but BEFORE any tools are registered.
    Safe to call multiple times - will only apply once.
    """
    global _patch_applied

    if _patch_applied:
        return

    try:
        from fastmcp.utilities import json_schema

        # Save original function
        _original_compress_schema = json_schema.compress_schema

        def patched_compress_schema(
            schema: dict,
            prune_params: list[str] | None = None,
            prune_defs: bool = True,
            prune_additional_properties: bool = True,
            prune_titles: bool = False,
        ) -> dict:
            """Patched compress_schema that also fixes Google Function Calling compatibility."""
            # Call original function
            result = _original_compress_schema(
                schema,
                prune_params=prune_params,
                prune_defs=prune_defs,
                prune_additional_properties=prune_additional_properties,
                prune_titles=prune_titles,
            )

            # Apply Google Function Calling fix
            return _fix_google_anyof(result)

        # Apply monkey patch
        json_schema.compress_schema = patched_compress_schema

        # Also patch compress_schema at module level for direct imports
        import fastmcp.utilities.json_schema as json_schema_module

        json_schema_module.compress_schema = patched_compress_schema

        _patch_applied = True
        print("[Schema Patch] Applied Google Function Calling compatibility fix")

    except Exception as e:
        print(f"[Schema Patch] Error applying patch: {e}")
