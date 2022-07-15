import tweepy
import os
import sys
import re
from datetime import datetime
from logger import get_log_file_path, append_log

consumer_key = os.getenv("API_KEY")
consumer_secret = os.getenv("API_SECRET")
access_token = os.getenv("ACCESS_TOKEN")
access_token_secret = os.getenv("ACCESS_TOKEN_SECRET")

def upload_video_and_tweet(vid_file_path, tweet_msg):
    # api v1 authentication (media upload still only supported by v1)
    auth = tweepy.OAuth1UserHandler(
        consumer_key, consumer_secret, access_token, access_token_secret
    )
    api = tweepy.API(auth)

    # api v2 authentication (basic dev account must use v2 API for posting tweet)
    client = tweepy.Client(
        consumer_key=consumer_key,
        consumer_secret=consumer_secret,
        access_token=access_token,
        access_token_secret=access_token_secret,
    )

    media = api.media_upload(
        vid_file_path,
        media_category="tweet_video"
    )
    response = client.create_tweet(text=tweet_msg, media_ids=[media.media_id])
    print(f"https://twitter.com/user/status/{response.data['id']}")

    fc_id = re.search(r"\\([\d]+_[A-Z]+)\\", vid_file_path).groups()[0]
    append_log(get_log_file_path("run.log"), f"{fc_id}\n")

if __name__ == "__main__":
    try:
        fp =  "C:\\Users\\drslc\\Twitter_Smoke_Bot\\forecasts\\2022071501_SW\\2022071501_SW.mp4"
        msg = "media upload test"
        if(len(sys.argv) > 1):
            fp = sys.argv[1]
            msg = sys.argv[2]

        upload_video_and_tweet(fp, msg)
    except Exception as e:
        dt_string = datetime.now().strftime("%d/%m/%Y %H:%M:%S")
        append_log(get_log_file_path("error_log.log", f"{dt_string} - Error in tweet post script: {str(e)}\n\n"))
        raise(e)
