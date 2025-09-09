"""Fingerprint Capture Module"""

from bridge_client import send_bridge_command

def capture_fingerprint_bmp(person_id, finger_index, member="prisoner"):
    """Capture a fingerprint image in BMP format.

    Args:
        person_id (str): Unique identifier for the person.
        finger_index (int): Index of the finger being captured (1-10).
        member (str): Whether the person is "prisoner" or "suspect".

    Returns:
        dict: A dictionary containing the result of the capture operation.
    """
    command = f"CAPTURE {person_id} {finger_index} {member}\n"
    return send_bridge_command(command, timeout=30)

