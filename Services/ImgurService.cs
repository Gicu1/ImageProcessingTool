using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using ImageProcessingTool.Models;

public class ImgurService
{
    private readonly HttpClient _client;
    private readonly ImgurOptions _options;
    private readonly ILogger<ImgurService> _logger;

    public ImgurService(HttpClient client, IOptions<ImgurOptions> options, ILogger<ImgurService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<string>> GetImagesAsync(string query)
    {
        _client.DefaultRequestHeaders.Add("Authorization", "Client-ID " + _options.ClientId);
        var response = await _client.GetAsync($"https://api.imgur.com/3/gallery/search?q={query}");
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        // Log the response
        _logger.LogInformation("Imgur API response: {Response}", responseJson);

        var responseObject = JsonDocument.Parse(responseJson);

        var imageUrls = new List<string>();
        foreach (var item in responseObject.RootElement.GetProperty("data").EnumerateArray())
        {
            if (item.TryGetProperty("images", out var images))
            {
                foreach (var image in images.EnumerateArray())
                {
                    if (image.TryGetProperty("link", out var linkProperty))
                    {
                        imageUrls.Add(linkProperty.GetString());
                    }
                }
            }
        }

        return imageUrls;
    }

    public async Task<HttpResponseMessage> DownloadImageAsync(string imageUrl)
    {
        return await _client.GetAsync(imageUrl);
    }
}
