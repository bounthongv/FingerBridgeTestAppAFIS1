# Flask API System Tray Implementation Guide

This documentation covers how to run a Python Flask API as a Windows service with a system tray icon, including implementation details and packaging instructions.

## Table of Contents

1. Required Libraries
2. System Tray Implementation (fingerprintapi.py)
3. PyInstaller Configuration (FingerprintAPI.spec)
4. Building the Executable
5. Deployment

## 1. Required Libraries

Install these libraries in your Python environment:

```bash
pip install Flask Flask-Cors PyMySQL pystray Pillow werkzeug
```

Key libraries and their roles:

- `pystray`: System tray icon and menu management
- `Pillow`: Image handling for tray icon
- `werkzeug`: Web server control for start/stop functionality
- `ctypes`: Windows API integration for message boxes

## 2. System Tray Implementation (fingerprintapi.py)

### Core Components:

```python
import threading
import ctypes
import os
import sys
from pystray import Icon, Menu, MenuItem
from PIL import Image
from app import app
from werkzeug.serving import make_server

# Resource path handling for PyInstaller
def resource_path(relative_path):
    try:
        base_path = sys._MEIPASS  # PyInstaller temp folder
    except Exception:
        base_path = os.path.abspath(".")
    return os.path.join(base_path, relative_path)

# Server thread management
class ServerThread(threading.Thread):
    def __init__(self):
        super().__init__(daemon=True)
        self.server = make_server('0.0.0.0', 5001, app)

    def run(self):
        self.server.serve_forever()

    def shutdown(self):
        self.server.shutdown()

# System tray functions
def create_menu():
    return Menu(
        MenuItem('About', show_about),
        MenuItem('Pause' if api_running else 'Run', toggle_api),
        MenuItem('Exit', on_exit)
    )

# Initialize tray icon
icon_path = resource_path('fingerprintAPI.ico')
icon = Icon('FingerprintAPI', Image.open(icon_path),
            'Fingerprint API', menu=create_menu())
```

### Key Implementation Notes:

- Use `resource_path()` for asset loading in both dev and production
- The `ServerThread` class enables clean server start/stop
- Menu items are dynamically updated based on API state
- API runs in a daemon thread to avoid blocking the main thread

## 3. PyInstaller Configuration (FingerprintAPI.spec)

### Essential Configuration:

```python
# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    ['fingerprintapi.py'],  # Entry point
    pathex=[],
    binaries=[],
    datas=[
        ('fingerprintAPI.ico', '.'),  # Include icon
        ('app.py', '.'),  # Include Flask app
        ('capture.py', '.'),  # Include API modules
        ('match.py', '.'),
        ('verify.py', '.')
    ],
    hiddenimports=[
        'pymysql', 'flask_cors', 'capture', 'verify', 'match',
        'pystray', 'PIL', 'pystray._win32', 'pystray._darwin', 'pystray._xorg'
    ],
    # ... other configuration
)

exe = EXE(
    # ... other parameters
    console=False,  # Run without console window
    icon='fingerprintAPI.ico'  # Set application icon
)
```

### Key Configuration Notes:

- Set `console=False` to hide the terminal window
- Include all necessary Python files and assets in `datas`
- Specify all required hidden imports
- Configure application icon

## 4. Building the Executable

### Build Script (build.bat):

```batch
@echo off
REM Activate virtual environment
if not defined VIRTUAL_ENV (
    call venv\Scripts\activate
)

REM Install dependencies
pip install -r requirements.txt

REM Build executable
pyinstaller FingerprintAPI.spec
```

### Build Process:

1. Place all files in a directory:
   - Python files: app.py, fingerprintapi.py, etc.
   - Assets: fingerprintAPI.ico
   - Configuration: FingerprintAPI.spec, build.bat, requirements.txt
2. Run the build script:

   ```bash
   .\build.bat
   ```

3. The executable will be in the `dist` folder

## 5. Deployment

### Running the Application:

- Execute `dist\FingerprintAPI.exe`
- The application will:
  - Start as a background process
  - Appear as a system tray icon
  - Run the Flask API on port 5001

### Features:

- **System Tray Menu**:

  - About: Shows copyright information
  - Pause/Run: Toggles API server state
  - Exit: Terminates the application

- **Automatic Startup**:
  - Use Windows Task Scheduler to run at startup
  - Reference: `add_startup.bat` and `create_scheduled_task.bat`

### Best Practices:

- Test API functionality after installation
- Verify tray icon appears and menu functions work
- For production:
  - Code sign the executable
  - Use a proper Windows service wrapper for better reliability
