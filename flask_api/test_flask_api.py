#!/usr/bin/env python3
"""
Test Flask API endpoint
Run this AFTER the socket test
"""

import requests
import json
import time

def test_flask_capture():
    print("=" * 50)
    print("TESTING FLASK API")
    print("=" * 50)
    
    url = "http://localhost:5000/capture"
    payload = {
        "person_id": "test_debug",
        "finger_index": 1,
        "status": "prisoner"
    }
    
    print(f"URL: {url}")
    print(f"Payload: {json.dumps(payload, indent=2)}")
    print("Sending request...")
    
    try:
        start_time = time.time()
        response = requests.post(url, json=payload, timeout=35)
        duration = time.time() - start_time
        
        print(f"Response time: {duration:.2f} seconds")
        print(f"Status code: {response.status_code}")
        
        if response.status_code == 200:
            result = response.json()
            print("Response JSON:")
            print(json.dumps(result, indent=2))
            
            # Analyze the response
            if result.get("status") == "success":
                print("\n✅ SUCCESS: Capture worked!")
            elif result.get("status") == "error":
                error_msg = result.get("message", "Unknown error")
                print(f"\n❌ ERROR: {error_msg}")
                
                if "device not connected" in error_msg.lower():
                    print("🔧 Fix: Check fingerprint device connection")
                elif "socket" in error_msg.lower():
                    print("🔧 Fix: Check bridge application")
                else:
                    print("🔧 Fix: Check bridge application console")
        else:
            print(f"HTTP Error: {response.status_code}")
            print(f"Response: {response.text}")
            
    except requests.exceptions.ConnectionError:
        print("❌ Cannot connect to Flask API")
        print("🔧 Fix: Make sure Flask app is running on port 5000")
    except requests.exceptions.Timeout:
        print("❌ Request timed out")
        print("🔧 Fix: Check if bridge app is responding")
    except Exception as e:
        print(f"❌ Unexpected error: {e}")

if __name__ == "__main__":
    test_flask_capture()