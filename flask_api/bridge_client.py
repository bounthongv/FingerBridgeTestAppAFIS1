"""Client for communicating with the Fingerprint Bridge Service.
"""

import socket
import time
from config import BRIDGE_HOST, BRIDGE_PORT

def send_bridge_command(command, timeout=30):
    """Sends a command to the fingerprint bridge service and gets the response.

    Args:
        command (str): The command string to send.
        timeout (int): The socket timeout in seconds.

    Returns:
        dict: A dictionary containing the status, message, bmp_base64 and structured fields.
    """
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.settimeout(timeout)
            s.connect((BRIDGE_HOST, BRIDGE_PORT))
            s.sendall(command.encode())

            start_time = time.time()
            data = b""
            while True:
                chunk = s.recv(4096)
                if not chunk:
                    break
                data += chunk
            duration = time.time() - start_time
            print(f"[Bridge] Response received in {duration:.2f} sec")

            decoded = data.decode("utf-8", errors="ignore")
            lines = decoded.splitlines()

            result_message = "No valid response from bridge"
            base64_bmp = ""
            
            # Enhanced parsing for structured fields
            person_id = None
            finger_index = None
            member = None
            score = None

            for line in lines:
                clean_line = line.strip().lstrip("\ufeff")
                print(f"[Bridge] {clean_line}")
                
                if clean_line.startswith("BMP:"):
                    base64_bmp = clean_line[4:]
                elif clean_line.startswith("PERSON_ID:"):
                    person_id = clean_line[10:].strip()
                elif clean_line.startswith("FINGER_INDEX:"):
                    try:
                        finger_index = int(clean_line[13:].strip())
                    except ValueError:
                        pass
                elif clean_line.startswith("MEMBER:"):
                    member = clean_line[7:].strip()
                elif clean_line.startswith("SCORE:"):
                    try:
                        score = float(clean_line[6:].strip())
                    except ValueError:
                        pass
                elif "✅" in clean_line or "❌" in clean_line or clean_line.upper().startswith("OK") or "ERROR" in clean_line.upper():
                    result_message = clean_line

            status_str = "error"
            if "✅" in result_message or "OK" in result_message.upper():
                status_str = "success"
            if "❌" in result_message: # No match is not an error
                status_str = "no_match"
            if "match" in result_message.lower():
                if "✅" in result_message:
                    status_str = "match"
                elif "❌" in result_message:
                    status_str = "no_match"

            # Build enhanced response with backward compatibility
            response = {
                "status": status_str,
                "message": result_message,
                "bmp_base64": base64_bmp
            }
            
            # Add structured fields when available (for enhanced match endpoint)
            if person_id is not None:
                response["person_id"] = person_id
            if finger_index is not None:
                response["finger_index"] = finger_index
            if member is not None:
                response["member"] = member
            if score is not None:
                response["score"] = score
                
            return response

    except socket.timeout:
        return {"status": "error", "message": f"Socket timeout after {timeout} seconds"}
    except socket.error as e:
        return {"status": "error", "message": f"Socket error: {str(e)}"}
    except Exception as e:
        return {"status": "error", "message": f"An unexpected error occurred: {str(e)}"}
