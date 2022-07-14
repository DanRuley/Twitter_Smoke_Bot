import os
from moviepy.video.io.ImageSequenceClip import ImageSequenceClip
import sys

if __name__ == '__main__':

    image_folder = "C:\\imgs\\2022071207_NW"
    output = "C:\\imgs\\2022071207_NW\\test1.mp4"

    if(len(sys.argv) > 1):
        image_folder = sys.argv[1]
        output = sys.argv[2]

    image_files = [os.path.join(image_folder, img)
                for img in os.listdir(image_folder)
                if img.endswith(".png")]
    clip = ImageSequenceClip(image_files, durations=[0.15 for _ in image_files])
    clip.write_videofile(output, fps = 60)