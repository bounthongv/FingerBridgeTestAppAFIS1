==============================
Fingerprint API - README.txt
==============================

Project Name:  Fingerprint API Bridge
Executable:    FingerprintAPI.exe
Platform:      Windows 64-bit (No Python required)
Version:       1.0.0
Build Date:    June 2025

-----------------------------------
📦 ABOUT THIS APPLICATION
-----------------------------------

This standalone application exposes a REST API bridge between your fingerprint scanner (via desktop bridge)
and your web-based frontend or integration system. It is built with Flask and bundled using PyInstaller.

There is NO need to install Python, libraries, or virtual environments.

-----------------------------------
🚀 HOW TO USE
-----------------------------------

1. **Installation:**
   - Copy `FingerprintAPI.exe` from the `/dist` folder to any directory (e.g. `C:\FingerprintAPI`)
   - No installation is needed — it’s a portable single-file executable.

2. **Launching the API:**
   - Double-click `FingerprintAPI.exe` to start the server.
   - A console window will appear and show:
     ```
     Running on http://127.0.0.1:5000/
     ```
   - Keep this window running while the system is in use.

3. **Available Endpoints:**
   - `POST /capture` → Capture a fingerprint
   - `POST /verify` → Verify a fingerprint by person ID and finger index
   - `POST /match`  → Identify a person from a captured fingerprint

   All endpoints accept JSON payloads and return JSON responses.

4. **Example JSON Request for `/verify`:**
   ```json
   {
     "person_id": "2",
     "finger_index": 3
   }
````

---

## ⚠️ IMPORTANT NOTES

* Requires the fingerprint scanner and Fingerprint Bridge (C# Desktop App) to be running.

* The device must be connected and powered before launching the API.

* Firewall: Make sure that port 5000 is open if accessed externally.

---

## 🔧 TROUBLESHOOTING

* If nothing happens when you double-click:
  • Right-click → Run as Administrator
  • Make sure antivirus/firewall is not blocking the executable.

* If port 5000 is busy:
  • You can edit `app.py` and change:

  ```python
  app.run(host="127.0.0.1", port=5000)
  ```

---

## 📂 FILE STRUCTURE

You only need to deliver the following:

* `FingerprintAPI.exe`   ← main API application
* `README.txt`            ← this instruction guide (optional)

---

## 👨‍💻 SUPPORT & MAINTENANCE

This application was developed by:
APIS Co. Ltd.

