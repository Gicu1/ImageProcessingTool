using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ImageProcessingTool.Services;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using SixLabors.ImageSharp.Formats;

namespace ImageProcessingTool.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ImgurService _imgurService;
        private readonly ILogger<IndexModel> _logger;

        public List<string> Images { get; set; }

        [BindProperty]
        public string Query { get; set; }

        [BindProperty]
        public string SortFilterOption { get; set; }

        public IndexModel(ImgurService imgurService, ILogger<IndexModel> logger)
        {
            _imgurService = imgurService;
            _logger = logger;
            Images = new List<string>();
        }

        public async Task OnGetAsync()
        {
            Images = await _imgurService.GetImagesAsync("cats");
            ApplySortFilter();
        }

        public async Task OnPostAsync()
        {
            if (!string.IsNullOrWhiteSpace(Query))
            {
                Images = await _imgurService.GetImagesAsync(Query);
                ApplySortFilter();
            }
        }

        private void ApplySortFilter()
        {
            // Filter out GIFs
            Images = Images.Where(img => !img.EndsWith(".gif")).ToList();

            // Apply sorting/filtering based on the selected option
            if (!string.IsNullOrEmpty(SortFilterOption))
            {
                switch (SortFilterOption)
                {
                    case "Alphabetical":
                        Images.Sort();
                        break;
                    case "ReverseAlphabetical":
                        Images.Sort();
                        Images.Reverse();
                        break;
                    // Add more cases for different sorting/filtering options
                    default:
                        break;
                }
            }
        }

        public IActionResult OnGetDownloadImage(string imageUrl)
        {
            var fileName = Path.GetFileName(imageUrl);
            var localFilePath = Path.Combine("wwwroot", "images", fileName);

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(imageUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                    response.Content.CopyToAsync(fileStream).Wait();
                    fileStream.Close();
                }
            }

            var fileBytes = System.IO.File.ReadAllBytes(localFilePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        public IActionResult OnGetDownloadBlackAndWhiteImage(string imageUrl)
        {
            var fileName = Path.GetFileName(imageUrl);
            var localFilePath = Path.Combine("wwwroot", "images", fileName);

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(imageUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                    response.Content.CopyToAsync(fileStream).Wait();
                    fileStream.Close();
                }
            }

            using (var originalImage = new Bitmap(localFilePath))
            {
                var bwImage = new Bitmap(originalImage.Width, originalImage.Height);
                for (int y = 0; y < originalImage.Height; y++)
                {
                    for (int x = 0; x < originalImage.Width; x++)
                    {
                        var originalColor = originalImage.GetPixel(x, y);
                        var grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                        var bwColor = Color.FromArgb(grayScale, grayScale, grayScale);
                        bwImage.SetPixel(x, y, bwColor);
                    }
                }

                var bwFilePath = Path.Combine("wwwroot", "images", "bw_" + fileName);
                bwImage.Save(bwFilePath, ImageFormat.Png);

                var fileBytes = System.IO.File.ReadAllBytes(bwFilePath);
                return File(fileBytes, "image/png", "bw_" + fileName);
            }
        }
    }
}
