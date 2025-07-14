using Microsoft.Maui.Controls;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using Plugin.Maui.Audio;
using System.Text.RegularExpressions;
using YoutubeExplode.Common;
using System.Linq.Expressions;

namespace Moonoto.Views;

public partial class HomePage : ContentPage
{
    private readonly YoutubeClient _youtube;
    private readonly HttpClient _http;
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _audioPlayer;

    public HomePage()
    {
        InitializeComponent();

        _youtube = new YoutubeClient();
        _http = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

        _audioManager = AudioManager.Current;
    }

    public class SearchResult
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string VideoId { get; set; } = "";
    }


private async void OnSearch(object sender, EventArgs e)
{
    string query = SearchBar.Text?.Trim() ?? "";
    if (string.IsNullOrWhiteSpace(query)) return;
    query = query + " 'topic'";

    var searchResults = await _youtube.Search.GetVideosAsync(query).CollectAsync(10);

    var results = searchResults.Select(video => new SearchResult
    {
        VideoId = video.Id.Value,
        Title = video.Title,
        Author = video.Author.ChannelTitle
    }).ToList();

    ResultsList.ItemsSource = results;
}


    async void OnResultSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

        try
        {
            // Assuming your selected item contains a video ID string property, adjust as needed
            var videoIdString = (e.SelectedItem as YourVideoItem)?.VideoId;
            if (string.IsNullOrEmpty(videoIdString))
                return;

            // Create a VideoId object from the string (YoutubeExplode type)
            var videoId = YoutubeExplode.Videos.VideoId.Parse(videoIdString);

            // This call is asynchronous and must be awaited to avoid blocking UI thread
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);

            // Now you can use streamManifest to get streams or other info
            // For example, get the best video stream:
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            // TODO: Update your UI with stream info here
            // Remember to update UI on the main thread (this method is already on main thread after await)
            // e.g. DisplayStreamInfo(streamInfo);

        }
        catch (Exception ex)
        {
            // Handle exceptions gracefully, e.g., show alert
            await DisplayAlert("Error", $"Failed to get stream info: {ex.Message}", "OK");
        }
        finally
        {
            // Deselect item after processing
            (sender as ListView).SelectedItem = null;
        }
    }
    public class YourVideoItem
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
    }
}

