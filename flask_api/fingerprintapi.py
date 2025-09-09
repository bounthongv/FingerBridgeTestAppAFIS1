import threading
import ctypes
import time
import os
import sys
from pystray import Icon, Menu, MenuItem
from PIL import Image
from app import app
from werkzeug.serving import make_server

# Global variables for API state and server control
api_running = True
server_thread = None
server = None

class ServerThread(threading.Thread):
    def __init__(self):
        super().__init__(daemon=True)
        self._stop_event = threading.Event()
        self.server = make_server('0.0.0.0', 5001, app)
        
    def run(self):
        self.server.serve_forever()
        
    def shutdown(self):
        self.server.shutdown()

def start_api():
    global server_thread, api_running
    if server_thread and server_thread.is_alive():
        return
    
    server_thread = ServerThread()
    server_thread.start()
    api_running = True

def stop_api():
    global api_running
    if server_thread and server_thread.is_alive():
        server_thread.shutdown()
        api_running = False

def on_exit(icon, item):
    stop_api()
    icon.stop()

def show_about(icon, item):
    """Show About dialog"""
    from PyQt5.QtWidgets import QDialog, QVBoxLayout, QLabel
    from PyQt5.QtCore import Qt
    from PyQt5.QtGui import QFont, QPixmap
    import sys
    
    class AboutDialog(QDialog):
        def __init__(self):
            super().__init__(None)
            # Set window flags to remove the question mark and extra decorations
            self.setWindowFlags(Qt.Window | Qt.WindowTitleHint | Qt.WindowCloseButtonHint | Qt.MSWindowsFixedSizeDialogHint)
            self.setWindowTitle("About")
            self.setFixedSize(250, 100)
            
            layout = QVBoxLayout()
            layout.setSpacing(0)
            layout.setContentsMargins(10, 10, 10, 10)
            
            # Create the APIS text with HTML for colors
            apis_label = QLabel()
            apis_label.setAlignment(Qt.AlignCenter)
            apis_label.setText('<span style="font-size: 24pt; font-weight: bold;">'
                             '<span style="color: #0000FF;">AP</span>'
                             '<span style="color: #FF0000;">I</span>'
                             '<span style="color: #0000FF;">S</span></span>')
            
            # Add copyright text
            copyright_label = QLabel("APIS Co. Ltd. All rights reserved")
            copyright_label.setAlignment(Qt.AlignCenter)
            
            # Add labels to layout
            layout.addWidget(apis_label)
            layout.addWidget(copyright_label)
            
            self.setLayout(layout)
    
    # Create and show the dialog
    from PyQt5.QtWidgets import QApplication
    if not QApplication.instance():
        app = QApplication(sys.argv)
    dialog = AboutDialog()
    dialog.exec_()

def toggle_api(icon, item):
    """Toggle API running state"""
    if api_running:
        stop_api()
    else:
        start_api()
    
    # Update menu to reflect new state
    icon.menu = create_menu()
    icon.update_menu()

def create_menu():
    """Create system tray menu with current state"""
    return Menu(
        MenuItem('About', show_about),
        MenuItem('Pause' if api_running else 'Run', toggle_api),
        MenuItem('Exit', on_exit)
    )

# Determine base path for resources
def resource_path(relative_path):
    """Get absolute path to resource, works for dev and for PyInstaller"""
    try:
        # PyInstaller creates a temp folder and stores path in _MEIPASS
        base_path = sys._MEIPASS
    except Exception:
        base_path = os.path.abspath(".")
    return os.path.join(base_path, relative_path)

# Create icon with initial menu
icon_path = resource_path('fingerprintAPI.ico')
icon = Icon('FingerprintAPI', Image.open(icon_path), 'Fingerprint API', menu=create_menu())

# Start API initially
start_api()

icon.run()
