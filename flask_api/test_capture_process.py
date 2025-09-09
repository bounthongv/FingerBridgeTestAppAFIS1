#!/usr/bin/env python3
"""
Test to diagnose why /capture API returns 'error, no valid response'
This ONLY tests socket communication with bridge application.
The bridge app will handle remote database connection automatically.
"""

import socket
import time

def test_capture_process():
    print("=" * 60)
    print("TESTING CAPTURE PROCESS - DIAGNOSE 'NO VALID RESPONSE'")
    print("Bridge app connects to remote database (apis.com.la) automatically")
    print("=" * 60)
    
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.settimeout(35)  # Longer timeout for capture
            s.connect(('127.0.0.1', 8123))
            print("✅ Socket connected to bridge")
            
            # Send the exact command that Flask API sends
            command = "CAPTURE test_web_capture 1 prisoner\n"
            print(f"Sending command: {command.strip()}")
            print("Waiting for response...")
            
            s.sendall(command.encode())
            
            # Monitor response in real-time
            data = b""
            start_time = time.time()
            last_chunk_time = start_time
            
            while time.time() - start_time < 30:  # 30 second total timeout
                try:
                    chunk = s.recv(1024)
                    if not chunk:
                        break
                    
                    data += chunk
                    current_time = time.time()
                    print(f"[{current_time - start_time:.1f}s] Received {len(chunk)} bytes")
                    last_chunk_time = current_time
                    
                    # Show partial response as it comes in
                    try:
                        decoded = data.decode('utf-8', errors='ignore')
                        lines = decoded.splitlines()
                        if lines:
                            print(f"   Last line: {lines[-1][:100]}...")
                    except:
                        pass
                    
                    # Check for completion markers
                    if any(marker in decoded for marker in ['BMP:', '✅', '❌', 'ERROR']):
                        print("   Detected completion marker")
                        break
                        
                except socket.timeout:
                    elapsed = time.time() - last_chunk_time
                    if elapsed > 5:  # No data for 5 seconds
                        print(f"   No data received for {elapsed:.1f} seconds")
                        break
                    continue
            
            total_time = time.time() - start_time
            response = data.decode('utf-8', errors='ignore')
            
            print(f"\nCapture completed in {total_time:.1f} seconds")
            print(f"Total response size: {len(data)} bytes")
            print("=" * 40)
            print("FULL RESPONSE:")
            print("=" * 40)
            print(response if response else "[EMPTY RESPONSE]")
            print("=" * 40)
            
            # Analyze what went wrong
            if not response.strip():
                print("\n❌ PROBLEM: Empty response from bridge")
                print("   This explains the 'no valid response' error")
                print("   → Check VS Code console for bridge application errors")
                print("   → Check if fingerprint device is connected")
                
            elif "❌ Fingerprint device not connected" in response:
                print("\n❌ PROBLEM: Device not connected")
                print("   → Connect fingerprint scanner")
                print("   → Check device drivers")
                
            elif "❌ Failed to save fingerprint" in response:
                print("\n❌ PROBLEM: Database save failed")
                print("   → Check database connection")
                print("   → Check database schema")
                
            elif "❌ Capture failed" in response:
                print("\n❌ PROBLEM: Fingerprint capture failed")
                print("   → Check device hardware")
                print("   → Check if finger was placed on scanner")
                
            elif "✅" in response and "Successfully captured" in response:
                print("\n✅ SUCCESS: Capture worked!")
                print("   The issue might be elsewhere in the API chain")
                
            else:
                print("\n🤔 UNEXPECTED RESPONSE")
                print("   → Check the response above for clues")
            
            return response
            
    except ConnectionRefused:
        print("❌ Cannot connect to bridge on port 8123")
        print("   → Make sure bridge application is running")
        return ""
    except socket.timeout:
        print("❌ Socket timeout - bridge not responding")
        print("   → Bridge might be stuck or busy")
        return ""
    except Exception as e:
        print(f"❌ Socket error: {e}")
        return ""

if __name__ == "__main__":
    response = test_capture_process()
    
    print("\n" + "=" * 60)
    print("NEXT STEPS:")
    print("=" * 60)
    
    if not response.strip():
        print("🔧 FIX: Bridge is not responding properly")
        print("   1. Check VS Code console for errors")
        print("   2. Check if device handle is initialized")
        print("   3. Restart bridge application")
    elif "device not connected" in response.lower():
        print("🔧 FIX: Connect fingerprint device")
        print("   1. Plug in USB fingerprint scanner")
        print("   2. Check device drivers")
        print("   3. Restart bridge application")
    elif "✅" in response:
        print("✅ Capture works via socket!")
        print("   → Problem might be in Flask API or web application")
    else:
        print("🔧 Check the response analysis above")