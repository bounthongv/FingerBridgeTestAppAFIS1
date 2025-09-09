#!/usr/bin/env python3
"""
Basic socket test to check bridge communication
Run this script and tell me what output you get
"""

import socket
import time

def test_socket_connection():
    print("=" * 50)
    print("TESTING BASIC SOCKET CONNECTION")
    print("=" * 50)
    
    try:
        print("1. Attempting to connect to 127.0.0.1:8123...")
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.settimeout(10)
            s.connect(('127.0.0.1', 8123))
            print("✅ Connection successful!")
            
            # Send a simple test command
            command = "CAPTURE test_person 1 prisoner\n"
            print(f"2. Sending command: {command.strip()}")
            s.sendall(command.encode())
            
            # Wait for response
            print("3. Waiting for response...")
            data = b""
            start_time = time.time()
            
            while time.time() - start_time < 20:  # 20 second timeout
                try:
                    chunk = s.recv(1024)
                    if not chunk:
                        break
                    data += chunk
                    print(f"   Received {len(chunk)} bytes")
                    
                    # Check if we have a complete response
                    decoded = data.decode('utf-8', errors='ignore')
                    if any(marker in decoded for marker in ['ERROR', 'BMP:', '✅', '❌']):
                        break
                        
                except socket.timeout:
                    continue
            
            # Show results
            response = data.decode('utf-8', errors='ignore')
            print(f"\n4. Response received ({len(data)} bytes):")
            print("-" * 30)
            print(response)
            print("-" * 30)
            
            return True, response
            
    except ConnectionRefused:
        print("❌ Connection refused - bridge server not running on port 8123")
        return False, ""
    except socket.timeout:
        print("❌ Connection timeout")
        return False, ""
    except Exception as e:
        print(f"❌ Error: {e}")
        return False, ""

if __name__ == "__main__":
    success, response = test_socket_connection()
    
    print("\n" + "=" * 50)
    print("NEXT STEPS:")
    print("=" * 50)
    
    if success:
        if "device not connected" in response.lower():
            print("🔧 Issue: Fingerprint device not connected")
            print("   → Check USB connection")
            print("   → Check device drivers")
        elif "error" in response.lower():
            print("🔧 Issue: Bridge application error")
            print("   → Check VS Code console for detailed error")
        elif "✅" in response or "successfully" in response.lower():
            print("🎉 Capture worked! Issue might be elsewhere")
        else:
            print("🤔 Unexpected response - check VS Code console")
    else:
        print("🔧 Issue: Bridge server communication problem")
        print("   → Verify bridge app is running in VS Code")
        print("   → Check if port 8123 is in use")