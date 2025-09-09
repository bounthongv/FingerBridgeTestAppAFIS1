#!/usr/bin/env python3
"""
Test script to validate the /capture and /verify endpoints with status parameter
"""

import requests
import json

# Test the /capture endpoint with the new status parameter
def test_capture_with_status():
    base_url = "http://localhost:5001"
    capture_url = f"{base_url}/capture"
    
    # Test cases
    test_cases = [
        {
            "name": "Test with prisoner status",
            "data": {
                "person_id": "TEST001",
                "finger_index": 1,
                "status": "prisoner"
            }
        },
        {
            "name": "Test with suspect status",
            "data": {
                "person_id": "TEST002", 
                "finger_index": 2,
                "status": "suspect"
            }
        },
        {
            "name": "Test without status (should default to prisoner)",
            "data": {
                "person_id": "TEST003",
                "finger_index": 3
            }
        },
        {
            "name": "Test with invalid status",
            "data": {
                "person_id": "TEST004",
                "finger_index": 4,
                "status": "invalid_status"
            }
        }
    ]
    
    print("=" * 60)
    print("Testing /capture endpoint with status parameter")
    print("=" * 60)
    
    for i, test_case in enumerate(test_cases, 1):
        print(f"\n{i}. {test_case['name']}")
        print("-" * 40)
        
        try:
            response = requests.post(
                capture_url,
                json=test_case['data'],
                headers={'Content-Type': 'application/json'},
                timeout=30
            )
            
            print(f"Status Code: {response.status_code}")
            
            if response.headers.get('content-type', '').startswith('application/json'):
                result = response.json()
                print(f"Response: {json.dumps(result, indent=2)}")
            else:
                print(f"Response: {response.text}")
                
        except requests.exceptions.ConnectionError:
            print("❌ Error: Could not connect to the API server")
            print("   Make sure the Flask API is running on http://localhost:5001")
        except requests.exceptions.Timeout:
            print("❌ Error: Request timed out")
        except Exception as e:
            print(f"❌ Error: {str(e)}")

if __name__ == "__main__":
    test_capture_with_status()
    test_verify_with_status()

# Test the /verify endpoint with the new status parameter
def test_verify_with_status():
    base_url = "http://localhost:5001"
    verify_url = f"{base_url}/verify"
    
    # Test cases
    test_cases = [
        {
            "name": "Verify with prisoner status",
            "data": {
                "person_id": "TEST001",
                "finger_index": 1,
                "status": "prisoner"
            }
        },
        {
            "name": "Verify with suspect status",
            "data": {
                "person_id": "TEST002", 
                "finger_index": 2,
                "status": "suspect"
            }
        },
        {
            "name": "Verify without status (should default to prisoner)",
            "data": {
                "person_id": "TEST003",
                "finger_index": 3
            }
        },
        {
            "name": "Verify with invalid status",
            "data": {
                "person_id": "TEST004",
                "finger_index": 4,
                "status": "invalid_status"
            }
        }
    ]
    
    print("\n" + "=" * 60)
    print("Testing /verify endpoint with status parameter")
    print("=" * 60)
    
    for i, test_case in enumerate(test_cases, 1):
        print(f"\n{i}. {test_case['name']}")
        print("-" * 40)
        
        try:
            response = requests.post(
                verify_url,
                json=test_case['data'],
                headers={'Content-Type': 'application/json'},
                timeout=30
            )
            
            print(f"Status Code: {response.status_code}")
            
            if response.headers.get('content-type', '').startswith('application/json'):
                result = response.json()
                print(f"Response: {json.dumps(result, indent=2)}")
            else:
                print(f"Response: {response.text}")
                
        except requests.exceptions.ConnectionError:
            print("❌ Error: Could not connect to the API server")
            print("   Make sure the Flask API is running on http://localhost:5001")
        except requests.exceptions.Timeout:
            print("❌ Error: Request timed out")
        except Exception as e:
            print(f"❌ Error: {str(e)}")