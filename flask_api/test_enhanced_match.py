#!/usr/bin/env python3
"""
Test the enhanced /match endpoint
This tests the new structured response with person_id, finger_index, member fields
"""

import requests
import json
import time

def test_enhanced_match_endpoint():
    print("=" * 60)
    print("TESTING ENHANCED /match ENDPOINT")
    print("=" * 60)
    
    url = "http://localhost:5001/match"
    
    print(f"Testing URL: {url}")
    print("Sending POST request...")
    
    try:
        start_time = time.time()
        response = requests.post(url, json={}, timeout=65)  # Match can take longer
        duration = time.time() - start_time
        
        print(f"Response time: {duration:.2f} seconds")
        print(f"Status code: {response.status_code}")
        
        if response.status_code == 200:
            result = response.json()
            print("\n" + "=" * 40)
            print("RESPONSE JSON:")
            print("=" * 40)
            print(json.dumps(result, indent=2))
            
            # Analyze the enhanced response
            print("\n" + "=" * 40)
            print("RESPONSE ANALYSIS:")
            print("=" * 40)
            
            # Check backward compatibility fields
            print("📋 Backward Compatibility Fields:")
            print(f"   status: {result.get('status', 'MISSING')}")
            print(f"   message: {result.get('message', 'MISSING')}")
            print(f"   bmp_base64: {'Present' if result.get('bmp_base64') else 'Missing'}")
            
            # Check new structured fields
            print("\n🆕 New Structured Fields:")
            if 'person_id' in result:
                print(f"   person_id: {result['person_id']}")
            else:
                print("   person_id: Not available (no match or error)")
                
            if 'finger_index' in result:
                print(f"   finger_index: {result['finger_index']}")
            else:
                print("   finger_index: Not available (no match or error)")
                
            if 'member' in result:
                print(f"   member: {result['member']}")
            else:
                print("   member: Not available (no match or error)")
                
            if 'score' in result:
                print(f"   score: {result['score']}")
            else:
                print("   score: Not available (no match or error)")
            
            # Overall assessment
            print("\n" + "=" * 40)
            print("ASSESSMENT:")
            print("=" * 40)
            
            if result.get("status") == "match":
                print("✅ SUCCESS: Match found!")
                if all(field in result for field in ['person_id', 'finger_index', 'member', 'score']):
                    print("✅ All enhanced fields present")
                else:
                    print("⚠️  Some enhanced fields missing")
            elif result.get("status") == "no_match":
                print("ℹ️  INFO: No match found (this is normal)")
                print("✅ Enhanced endpoint working correctly")
            elif result.get("status") == "error":
                error_msg = result.get("message", "Unknown error")
                print(f"❌ ERROR: {error_msg}")
                
                if "device not connected" in error_msg.lower():
                    print("   → Connect fingerprint scanner")
                elif "no valid response" in error_msg.lower():
                    print("   → Check bridge application is running")
                else:
                    print("   → Check the error message above")
            else:
                print(f"🤔 UNEXPECTED STATUS: {result.get('status')}")
                
        else:
            print(f"\n❌ HTTP Error: {response.status_code}")
            print(f"Response: {response.text}")
            
    except requests.exceptions.ConnectionError:
        print("❌ Error: Could not connect to the API server")
        print("   → Make sure the Flask API is running on http://localhost:5001")
    except requests.exceptions.Timeout:
        print("❌ Error: Request timed out")
        print("   → Match operation may have taken too long")
    except Exception as e:
        print(f"❌ Error: {str(e)}")

if __name__ == "__main__":
    test_enhanced_match_endpoint()
    
    print("\n" + "=" * 60)
    print("NEXT STEPS:")
    print("=" * 60)
    print("1. Ensure fingerprint scanner is connected")
    print("2. Ensure bridge application is running")
    print("3. Place finger on scanner when prompted")
    print("4. Verify the enhanced response includes new fields")
    print("5. Test with both existing and new fingerprints")