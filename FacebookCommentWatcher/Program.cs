using System;
using System.IO;
using EvtSource;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Sender{
	public string name;
	public string id;
}

public class ChatData{
	public Sender from;
	public DateTime create_time;
	public string message;
	public string id;
}

public class Program {
	public static void Main() {
		Console.WriteLine("Hello World");
		Console.WriteLine("Enter VideoID:");

		string path = @"chatlog.txt";
		File.WriteAllText(path, "");

		string videoID = Console.ReadLine();
		Console.WriteLine($"\nGetting Data From {videoID}...");
		string accessToken = File.ReadAllText(@"token.txt");
		string link = $"https://streaming-graph.facebook.com/{videoID}/live_comments?access_token={accessToken}&comment_rate=one_per_two_seconds&fields=from{{name}},created_time,message";

		var evt = new EventSourceReader(new Uri(link)).Start();
		evt.MessageReceived += (object sender, EventSourceMessageEventArgs e) => Decrypt(e);
		evt.Disconnected += async (object sender, DisconnectEventArgs e) => {
			Console.WriteLine($"Retry: {e.ReconnectDelay} - Error: {e.Exception}");
			await Task.Delay(e.ReconnectDelay);
			evt.Start(); // Reconnect to the same URL
		};
		while(evt.IsDisposed == false) {

		}
	}

	public static void Decrypt(EventSourceMessageEventArgs e) {
		ChatData chat = JsonConvert.DeserializeObject<ChatData>(e.Message);
		Console.WriteLine(e.Message);

		if(chat.from == null) {
			chat.from = new Sender() { name = "Facebook User"};
		}

		if(chat.from.name == "Ilham Aryasuta J"|| chat.from.name == "Purplefin Neptuna") {
			chat.from.name = "PPfinNeptuna";
		}

		string newChat = $"{chat.from.name}: {chat.message}";
		Console.WriteLine(newChat);
		string path = @"chatlog.txt";
		File.AppendAllLines(path, new[] { newChat });
	}
}
