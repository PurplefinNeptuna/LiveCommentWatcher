using System;
using System.IO;
using System.Threading;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace YoutubeCommentWatcher {
	class LiveCommentWatcher {
		private static YouTubeService youtubeService;
		[STAThread]
		static void Main() {
			Console.WriteLine("Youtube Live Comment Watcher\n");

			string apiKey = File.ReadAllText(@"ytapikey.txt");

			Console.WriteLine("Enter Youtube video ID:");
			string videoID = Console.ReadLine();

			string path = @"chatlog\chatlog.txt";
			File.WriteAllText(path, "");

			Console.WriteLine("\nCOMMENTS\n=========================\n");

			youtubeService = new YouTubeService(new BaseClientService.Initializer() {
				ApiKey = apiKey,
				ApplicationName = "CommentWatcher"
			});

			try {
				string activeLiveChatID = GetLiveChatID(videoID);
				if(activeLiveChatID != null) {
					string nextToken = null;
					int delay = 0;
					while(true) {
						(nextToken,delay) = CommentWatcher(activeLiveChatID, nextToken);
						Thread.Sleep(delay);
					}
				}
				else {
					Console.WriteLine("Can't find liveChatID");
				}
			}
			catch(AggregateException ex) {
				foreach(var e in ex.InnerExceptions) {
					Console.WriteLine("Error: " + e.Message);
				}
			}

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}

		public static string GetLiveChatID(string videoID) {
			var videoList = youtubeService.Videos.List("liveStreamingDetails");
			videoList.Id = videoID;
			videoList.Fields = "items/liveStreamingDetails/activeLiveChatId";

			var videoListResponse = videoList.Execute();

			foreach(var v in videoListResponse.Items) {
				string liveChatID = v.LiveStreamingDetails.ActiveLiveChatId;
				if(liveChatID != null && liveChatID.Length != 0) {
					return liveChatID;
				}
			}

			return null;
		}

		private static (string, int) CommentWatcher(string chatID, string nextToken) {
			var liveChatRequest = youtubeService.LiveChatMessages.List(chatID, "snippet, authorDetails");
			liveChatRequest.PageToken = nextToken;
			liveChatRequest.Fields = "items(authorDetails(displayName), snippet(displayMessage)), nextPageToken, pollingIntervalMillis";

			var liveChatResponse = liveChatRequest.Execute();
			var chats = liveChatResponse.Items;
			foreach(var chat in chats) {
				string author = chat.AuthorDetails.DisplayName;
				string message = chat.Snippet.DisplayMessage;
				Console.WriteLine($"{author}: {message}");
				string path = @"chatlog\chatlog.txt";
				File.AppendAllLines(path, new[] { $"{author}: {message}" });
			}

			//await CommentWatcher(chatID, liveChatResponse.NextPageToken, (int)liveChatResponse.PollingIntervalMillis);
			return (liveChatResponse.NextPageToken, (int)liveChatResponse.PollingIntervalMillis);
		}
	}
}
