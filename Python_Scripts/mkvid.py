import os
from moviepy.video.io.ImageSequenceClip import ImageSequenceClip
import sys
from datetime import datetime
from logger import *

if __name__ == '__main__':
    try:
        image_folder = "c:\\users\\drslc\\Twitter_Smoke_Bot\\2022071411_NW"
        output = "c:\\users\\drslc\\Twitter_Smoke_Bot\\2022071411_NW\\2022071411_NW.mp4"

        if(len(sys.argv) > 1):
            image_folder = sys.argv[1]
            output = sys.argv[2]

        image_files = [os.path.join(image_folder, img)
                    for img in os.listdir(image_folder)
                    if img.endswith(".png")]
        clip = ImageSequenceClip(image_files, durations=[0.15 for _ in image_files])
        clip.write_videofile(output, fps = 30)

    except Exception as e:
        dt_string = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
        append_log(get_log_file_path("error_log.log", f"{dt_string} - Error in mkvid script: {str(e)}\n\n"))
        raise(e)