@echo off
REM Build script for Fingerprint API application

REM Ensure Python virtual environment is activated
if not defined VIRTUAL_ENV (
    echo Activating Python virtual environment...
    call venv\Scripts\activate
)

REM Install required packages
echo Installing required packages...
pip install -r requirements.txt

REM Run PyInstaller to create executable
echo Building executable...
pyinstaller FingerprintAPI.spec

echo Build completed successfully. The executable is in the 'dist' folder.
