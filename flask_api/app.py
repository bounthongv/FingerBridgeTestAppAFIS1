"""Flask API for Fingerprint Management System

This module provides a RESTful API interface for fingerprint operations including:
- Capturing new fingerprints
- Verifying fingerprints against stored templates
- Retrieving stored fingerprint images
- Matching fingerprints

The API communicates with a fingerprint bridge service via TCP socket and stores
fingerprint templates in a MySQL database.

Author: AratekTrustFinger Team
"""

import logging
import os
import base64
import pymysql
import sys

from flask import Flask, request, jsonify
from capture import capture_fingerprint_bmp
from verify import verify_fingerprint
from match import match_fingerprint
from flask_cors import CORS
from config import DB_HOST, DB_USER, DB_PASSWORD, DB_NAME

# Setup logging
if getattr(sys, 'frozen', False):
    bundle_dir = getattr(sys, '_MEIPASS', '.')
    log_file = os.path.join(bundle_dir, 'app.log')
else:
    log_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'app.log')
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s %(levelname)s %(message)s',
    handlers=[
        logging.FileHandler(log_file, encoding='utf-8'),
        logging.StreamHandler()
    ]
)

app = Flask(__name__)
CORS(app)

# -----------------------------
# ✅ GET SAVED IMAGE (BMP) FROM DB
# -----------------------------
@app.route('/get-image', methods=['GET'])
def get_image():
    """Retrieve a stored fingerprint image from the database.
    
    Query Parameters:
        person_id (str): The unique identifier of the person
        finger_index (str): The index of the finger (1-10)
        status (str): Whether the person is "prisoner" or "suspect" (optional, defaults to "prisoner")
    
    Returns:
        JSON: Contains base64 encoded BMP image or error message
    """
    person_id = request.args.get('person_id')
    finger_index = request.args.get('finger_index')
    status = request.args.get('status', 'prisoner')

    if not person_id or not finger_index:
        return jsonify({"error": "Missing person_id or finger_index"}), 400

    if status not in ['prisoner', 'suspect']:
        return jsonify({"error": "Status must be 'prisoner' or 'suspect'"}), 400

    try:
        conn = pymysql.connect(host=DB_HOST, user=DB_USER, password=DB_PASSWORD, database=DB_NAME)
        cursor = conn.cursor()
        cursor.execute("SELECT image_bmp FROM fingerprint_templates WHERE person_id = %s AND finger_index = %s AND status = %s",
                       (person_id, finger_index, status))
        result = cursor.fetchone()
        conn.close()

        if not result:
            return jsonify({"error": "No image found"}), 404

        image_bmp = result[0]
        encoded_image = base64.b64encode(image_bmp).decode('utf-8')
        return jsonify({"image_base64": encoded_image})
    except Exception as e:
        logging.error(f"Database error in /get-image: {e}")
        return jsonify({"error": "Database connection failed"}), 500

# -----------------------------
# ✅ CAPTURE
# -----------------------------
@app.route('/capture', methods=['POST'])
def capture():
    """Capture a new fingerprint and store it in the database.
    
    Request Body:
        person_id (str): The unique identifier of the person
        finger_index (int): The index of the finger (1-10)
        status (str): Whether the person is "prisoner" or "suspect"
    
    Returns:
        JSON: Contains status, message, and base64 encoded BMP image
    """
    data = request.get_json()
    if not data:
        return jsonify({"status": "error", "message": "Invalid JSON"}), 400

    person_id = data.get('person_id')
    finger_index = int(data.get('finger_index', 1))
    status = data.get('status', 'prisoner')

    if not person_id:
        logging.error("Missing person_id in /capture request")
        return jsonify({"status": "error", "message": "Missing person_id"}), 400

    if status not in ['prisoner', 'suspect']:
        logging.error(f"Invalid status '{status}' in /capture request")
        return jsonify({"status": "error", "message": "Status must be 'prisoner' or 'suspect'"}), 400

    logging.info(f"Calling capture_fingerprint_bmp({person_id}, {finger_index}, {status})")
    result = capture_fingerprint_bmp(person_id, finger_index, status)
    logging.info(f"Bridge response: {result}")
    return jsonify(result)

# -----------------------------
# ✅ VERIFY
# -----------------------------
@app.route('/verify', methods=['POST'])
def verify():
    """Verify a captured fingerprint against stored template.
    
    Request Body:
        person_id (str): The unique identifier of the person
        finger_index (int): The index of the finger (1-10)
        status (str): Whether the person is "prisoner" or "suspect"
    
    Returns:
        JSON: Contains verification result
    """
    data = request.get_json()
    if not data:
        return jsonify({"status": "error", "message": "Invalid JSON"}), 400

    person_id = data.get('person_id')
    finger_index = int(data.get('finger_index', 1))
    status = data.get('status', 'prisoner')

    if not person_id:
        logging.error("Missing person_id in /verify request")
        return jsonify({"status": "error", "message": "Missing person_id"}), 400

    if status not in ['prisoner', 'suspect']:
        logging.error(f"Invalid status '{status}' in /verify request")
        return jsonify({"status": "error", "message": "Status must be 'prisoner' or 'suspect'"}), 400

    logging.info(f"Calling verify_fingerprint({person_id}, {finger_index}, {status})")
    result = verify_fingerprint(person_id, finger_index, status)
    logging.info(f"Bridge response: {result}")
    return jsonify(result)

# -----------------------------
# ✅ MATCH
# -----------------------------
@app.route('/match', methods=['POST'])
def match():
    """Match a captured fingerprint against all stored templates."""
    logging.info("Calling match_fingerprint()")
    result = match_fingerprint()
    logging.info(f"Bridge response: {result}")
    return jsonify(result)

# -----------------------------
# ✅ RUN SERVER
# -----------------------------
if __name__ == '__main__':
    try:
        logging.info("Starting Flask API service on http://0.0.0.0:5001")
        app.run(host='0.0.0.0', port=5001, debug=False)
    except Exception as e:
        logging.exception("Exception occurred while running the Flask API service: %s", e)
