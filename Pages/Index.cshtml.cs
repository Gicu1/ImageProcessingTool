using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ImageProcessingTool.Services;
using Microsoft.Extensions.Logging;

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
    }
}
