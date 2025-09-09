#!/usr/bin/env python3
"""
Verify the exact schema of the remote database
This will tell us definitively what columns exist
"""

import pymysql

def verify_remote_database_schema():
    print("=" * 60)
    print("VERIFYING REMOTE DATABASE SCHEMA")
    print("=" * 60)
    
    # Use the actual database configuration from config.sys
    config = {
        'host': 'apis.com.la',
        'user': 'admin', 
        'password': 'Sql_admin@#2024',
        'database': 'apis2021_apims'
    }
    
    try:
        conn = pymysql.connect(**config)
        print("✅ Connected to remote database")
        
        cursor = conn.cursor()
        
        # Get the exact table structure
        print("\n1. Table structure:")
        cursor.execute("DESCRIBE fingerprint_templates")
        columns = cursor.fetchall()
        
        print("Column details:")
        has_status = False
        has_member = False
        has_updated_at = False
        
        for col in columns:
            field = col[0] or ''
            type_info = col[1] or ''
            null_info = col[2] or ''
            key_info = col[3] or ''
            default_info = col[4] or ''
            print(f"   {field:15} | {type_info:15} | {null_info:10} | {key_info:10} | {default_info:10}")
            if col[0] == 'status':
                has_status = True
            elif col[0] == 'updated_at':
                has_updated_at = True
        
        print(f"\n2. Column existence check:")
        print(f"   'status' column exists: {has_status}")
        print(f"   'member' column exists: {has_member}")
        print(f"   'updated_at' column exists: {has_updated_at}")
        
        # Test INSERT with correct column name
        print(f"\n3. Testing INSERT operation:")
        if has_member:
            print("   Testing with 'member' column...")
            try:
                cursor.execute("""
                    INSERT INTO fingerprint_templates (person_id, finger_index, member, image_bmp) 
                    VALUES ('test_schema_check', 1, 'prisoner', 'test_data')
                    ON DUPLICATE KEY UPDATE image_bmp = VALUES(image_bmp)
                """)
                conn.commit()
                print("   ✅ INSERT with 'member' column successful")
                
                # Clean up
                cursor.execute("DELETE FROM fingerprint_templates WHERE person_id = 'test_schema_check'")
                conn.commit()
                
            except Exception as e:
                print(f"   ❌ INSERT with 'member' failed: {e}")
        
        elif has_status:
            print("   Testing with 'status' column...")
            try:
                cursor.execute("""
                    INSERT INTO fingerprint_templates (person_id, finger_index, status, image_bmp) 
                    VALUES ('test_schema_check', 1, 'prisoner', 'test_data')
                    ON DUPLICATE KEY UPDATE image_bmp = VALUES(image_bmp)
                """)
                conn.commit()
                print("   ✅ INSERT with 'status' column successful")
                
                # Clean up
                cursor.execute("DELETE FROM fingerprint_templates WHERE person_id = 'test_schema_check'")
                conn.commit()
                
            except Exception as e:
                print(f"   ❌ INSERT with 'status' failed: {e}")
        
        conn.close()
        
        return {
            'has_status': has_status,
            'has_member': has_member,
            'has_updated_at': has_updated_at
        }
        
    except Exception as e:
        print(f"❌ Database connection failed: {e}")
        return None

if __name__ == "__main__":
    result = verify_remote_database_schema()
    
    print("\n" + "=" * 60)
    print("CONCLUSION:")
    print("=" * 60)
    
    if result:
        if result['has_member']:
            print("✅ Remote database uses 'member' column")
            print("   → Bridge application should use 'member' parameter")
            print("   → Current bridge code should be correct")
        elif result['has_status']:
            print("✅ Remote database uses 'status' column")
            print("   → Bridge application needs to use 'status' parameter")
        else:
            print("❌ Neither 'status' nor 'member' column found!")
            print("   → Database schema needs to be updated")
        
        if not result['has_updated_at']:
            print("⚠️  'updated_at' column missing")
            print("   → Remove 'updated_at = CURRENT_TIMESTAMP' from queries")
    else:
        print("❌ Could not verify database schema")
        print("   → Check database connection and credentials")