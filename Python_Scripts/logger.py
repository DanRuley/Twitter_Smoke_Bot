import os
import re

def append_log(logfile, msg):
    with open(logfile, "a") as log:
        log.write(msg)
    
def get_log_file_path(logfile):
    return re.sub(r"[A-Za-z_]+$", logfile, os.path.dirname(os.path.abspath(__file__)))