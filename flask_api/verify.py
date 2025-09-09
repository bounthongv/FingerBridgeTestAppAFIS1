"""Fingerprint Verification Module"""

from bridge_client import send_bridge_command

def verify_fingerprint(person_id, finger_index, member="prisoner"):
    """Verify a captured fingerprint against a stored template.

    Args:
        person_id (str): Unique identifier for the person.
        finger_index (int): Index of the finger to verify (1-10).
        member (str): Whether the person is "prisoner" or "suspect".

    Returns:
        dict: A dictionary containing the result of the verification operation.
    """
    command = f"VERIFY {person_id} {finger_index} {member}\n"

    return send_bridge_command(command, timeout=30)

