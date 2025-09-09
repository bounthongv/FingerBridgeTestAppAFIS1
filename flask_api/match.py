"""Fingerprint Matching Module"""

from bridge_client import send_bridge_command

def match_fingerprint():
    """Match a captured fingerprint against all stored templates.

    Returns:
        dict: A dictionary containing the result of the matching operation.
    """
    command = "MATCH\n"
    # Matching can take longer, so we use a longer timeout.
    return send_bridge_command(command, timeout=60)


