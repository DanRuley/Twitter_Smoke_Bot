import tweepy
import os
import sys

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
        media_category="tweet_video",
        chunked=False,
    )
    response = client.create_tweet(text=tweet_msg, media_ids=[media.media_id])
    print(f"https://twitter.com/user/status/{response.data['id']}")


if __name__ == "__main__":
    fp = "c:\\imgs\\2022071207_NW\\test1.mp4"
    msg = "media upload test"
    
    if(len(sys.argv) > 1):
        fp = sys.argv[1]
        msg = sys.argv[2]

    upload_video_and_tweet(fp, msg)
