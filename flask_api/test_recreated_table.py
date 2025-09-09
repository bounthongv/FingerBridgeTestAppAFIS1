#!/usr/bin/env python3
"""
Test the recreated table schema
Verify everything works with the new table structure
"""

import pymysql

def test_recreated_table():
    print("=" * 60)
    print("TESTING RECREATED TABLE SCHEMA")
    print("=" * 60)
    
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
        
        # 1. Verify table structure
        print("\n1. Table structure:")
        cursor.execute("DESCRIBE fingerprint_templates")
        columns = cursor.fetchall()
        
        expected_columns = ['id', 'person_id', 'finger_index', 'template', 'image_bmp', 'member']
        found_columns = [col[0] for col in columns]
        
        print("Columns found:")
        for col in columns:
            print(f"   - {col[0]} ({col[1]})")
        
        missing_columns = set(expected_columns) - set(found_columns)
        if missing_columns:
            print(f"❌ Missing columns: {missing_columns}")
        else:
            print("✅ All expected columns present")
        
        # 2. Verify constraints
        print("\n2. Constraints:")
        cursor.execute("""
            SELECT CONSTRAINT_NAME, CONSTRAINT_TYPE 
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
            WHERE TABLE_SCHEMA = 'apis2021_apims' 
            AND TABLE_NAME = 'fingerprint_templates'
        """)
        constraints = cursor.fetchall()
        
        for constraint in constraints:
            name, type_info = constraint
            print(f"   - {name} ({type_info})")
        
        # Check for the correct unique constraint
        unique_constraints = [c[0] for c in constraints if c[1] == 'UNIQUE']
        if 'unique_person_finger_member' in unique_constraints:
            print("✅ Correct unique constraint found")
        else:
            print(f"⚠️  Expected 'unique_person_finger_member' constraint, found: {unique_constraints}")
        
        # 3. Test exact SQL from C# code
        print("\n3. Testing C# INSERT SQL:")
        test_sql = """INSERT INTO fingerprint_templates (person_id, finger_index, image_bmp, template, member)
                      VALUES (%s, %s, %s, %s, %s)
                      ON DUPLICATE KEY UPDATE 
                          image_bmp = VALUES(image_bmp),
                          template = VALUES(template)"""
        
        try:
            cursor.execute(test_sql, ('test_recreated', 1, b'test_image', b'test_template', 'prisoner'))
            conn.commit()
            print("   ✅ INSERT successful")
            
            # Test SELECT queries
            print("\n4. Testing C# SELECT SQLs:")
            
            # Test GetStoredTemplate query
            cursor.execute("SELECT template FROM fingerprint_templates WHERE person_id = %s AND finger_index = %s AND member = %s", 
                          ('test_recreated', 1, 'prisoner'))
            result = cursor.fetchone()
            if result:
                print("   ✅ GetStoredTemplate query successful")
            else:
                print("   ❌ GetStoredTemplate query failed")
            
            # Test LoadBmpFromDatabase query
            cursor.execute("SELECT image_bmp FROM fingerprint_templates WHERE person_id = %s AND finger_index = %s AND member = %s", 
                          ('test_recreated', 1, 'prisoner'))
            result = cursor.fetchone()
            if result:
                print("   ✅ LoadBmpFromDatabase query successful")
            else:
                print("   ❌ LoadBmpFromDatabase query failed")
            
            # Clean up
            cursor.execute("DELETE FROM fingerprint_templates WHERE person_id = 'test_recreated'")
            conn.commit()
            print("   ✅ Cleanup successful")
            
        except Exception as e:
            print(f"   ❌ SQL test failed: {e}")
        
        # 5. Test unique constraint
        print("\n5. Testing unique constraint:")
        try:
            # Insert first record
            cursor.execute(test_sql, ('test_unique', 1, b'image1', b'template1', 'prisoner'))
            
            # Try to insert duplicate (should update, not create new)
            cursor.execute(test_sql, ('test_unique', 1, b'image2', b'template2', 'prisoner'))
            
            # Check only one record exists
            cursor.execute("SELECT COUNT(*) FROM fingerprint_templates WHERE person_id = 'test_unique'")
            count = cursor.fetchone()[0]
            
            if count == 1:
                print("   ✅ Unique constraint working (ON DUPLICATE KEY UPDATE)")
            else:
                print(f"   ❌ Unique constraint issue: found {count} records")
            
            # Test different member values (should create separate records)
            cursor.execute(test_sql, ('test_unique', 1, b'image3', b'template3', 'suspect'))
            cursor.execute("SELECT COUNT(*) FROM fingerprint_templates WHERE person_id = 'test_unique'")
            count = cursor.fetchone()[0]
            
            if count == 2:
                print("   ✅ Different member values create separate records")
            else:
                print(f"   ❌ Member separation issue: found {count} records")
            
            # Clean up
            cursor.execute("DELETE FROM fingerprint_templates WHERE person_id = 'test_unique'")
            conn.commit()
            
        except Exception as e:
            print(f"   ❌ Constraint test failed: {e}")
        
        conn.close()
        
        print("\n" + "=" * 60)
        print("CONCLUSION:")
        print("=" * 60)
        
        if missing_columns:
            print("❌ Table schema incomplete")
        elif 'unique_person_finger_member' not in unique_constraints:
            print("❌ Constraint missing or wrong name")
        else:
            print("✅ Table schema looks perfect!")
            print("✅ Ready to test with bridge application")
        
    except Exception as e:
        print(f"❌ Database connection failed: {e}")

if __name__ == "__main__":
    test_recreated_table()