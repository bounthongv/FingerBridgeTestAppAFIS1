#!/usr/bin/env python3
"""
Debug the exact SQL execution to see what's going wrong
This will show us exactly what's happening when the SQL is executed
"""

import pymysql

def debug_sql_execution():
    print("=" * 60)
    print("DEBUG SQL EXECUTION")
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
        
        # 1. Show current table structure
        print("\n1. Current table structure:")
        cursor.execute("DESCRIBE fingerprint_templates")
        columns = cursor.fetchall()
        
        column_names = []
        print("Columns in fingerprint_templates:")
        for col in columns:
            column_names.append(col[0])
            print(f"   - {col[0]} ({col[1]})")
        
        # 2. Test the exact same SQL that the C# code is using
        print(f"\n2. Testing the exact SQL from C# code:")
        
        # This is the exact SQL from MainForm.cs line 1744
        test_sql = """INSERT INTO fingerprint_templates (person_id, finger_index, image_bmp, template, member)
                      VALUES (%s, %s, %s, %s, %s)
                      ON DUPLICATE KEY UPDATE 
                          image_bmp = VALUES(image_bmp),
                          template = VALUES(template)"""
        
        print(f"SQL: {test_sql}")
        
        try:
            # Try to execute with test data
            cursor.execute(test_sql, ('test_debug', 1, b'test_image', b'test_template', 'prisoner'))
            conn.commit()
            print("   ✅ SQL execution SUCCESSFUL!")
            
            # Clean up
            cursor.execute("DELETE FROM fingerprint_templates WHERE person_id = 'test_debug'")
            conn.commit()
            
        except Exception as e:
            print(f"   ❌ SQL execution FAILED: {e}")
            
            # Let's check if the problem is with the column name
            if 'member' in str(e):
                print(f"\n   🔍 'member' column issue detected")
                print(f"   Available columns: {column_names}")
                
                # Try with different possible column names
                alternative_columns = ['status', 'type', 'category', 'role']
                for alt_col in alternative_columns:
                    if alt_col in column_names:
                        print(f"   💡 Found alternative column: {alt_col}")
                        
                        # Test with alternative column
                        alt_sql = test_sql.replace('member', alt_col)
                        try:
                            cursor.execute(alt_sql, ('test_debug2', 1, b'test_image', b'test_template', 'prisoner'))
                            conn.commit()
                            print(f"   ✅ Alternative SQL with '{alt_col}' works!")
                            cursor.execute(f"DELETE FROM fingerprint_templates WHERE person_id = 'test_debug2'")
                            conn.commit()
                            break
                        except Exception as alt_e:
                            print(f"   ❌ Alternative SQL with '{alt_col}' failed: {alt_e}")
        
        # 3. Check for any triggers, constraints, or special configurations
        print(f"\n3. Checking for triggers or constraints:")
        cursor.execute("SHOW TRIGGERS LIKE 'fingerprint_templates'")
        triggers = cursor.fetchall()
        if triggers:
            print("   Found triggers:")
            for trigger in triggers:
                print(f"   - {trigger[0]}")
        else:
            print("   No triggers found")
        
        # 4. Check table constraints
        cursor.execute("""
            SELECT CONSTRAINT_NAME, CONSTRAINT_TYPE 
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
            WHERE TABLE_SCHEMA = 'apis2021_apims' 
            AND TABLE_NAME = 'fingerprint_templates'
        """)
        constraints = cursor.fetchall()
        if constraints:
            print("   Found constraints:")
            for constraint in constraints:
                print(f"   - {constraint[0]} ({constraint[1]})")
        else:
            print("   No constraints found")
        
        conn.close()
        
    except Exception as e:
        print(f"❌ Database connection failed: {e}")

if __name__ == "__main__":
    debug_sql_execution()