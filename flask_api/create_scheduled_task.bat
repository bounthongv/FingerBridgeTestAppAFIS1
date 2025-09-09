@echo off
REM Create scheduled task to run FingerprintAPI on system startup
schtasks /create /tn "FingerprintAPI" /tr "d:\AratekTrustFinger\flask_api\dist\FingerprintAPI\FingerprintAPI.exe" /sc onstart /ru SYSTEM /rl highest /f
echo Scheduled task created successfully.
echo FingerprintAPI will now run automatically on system startup.
pause
