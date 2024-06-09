using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    private readonly ImgurService _imgurService;
    public List<string> Images { get; set; }

    [BindProperty]
    public string Query { get; set; }

    public IndexModel(ImgurService imgurService)
    {
        _imgurService = imgurService;
        Images = new List<string>();
    }

    public async Task OnGetAsync()
    {
        // Default search on page load
        Images = await _imgurService.GetImagesAsync("cats");
    }

    public async Task OnPostAsync()
    {
        if (!string.IsNullOrWhiteSpace(Query))
        {
            Images = await _imgurService.GetImagesAsync(Query);
            // Save images to local storage
            await SaveImagesAsync(Images);
        }
    }

    private async Task SaveImagesAsync(List<string> imageUrls)
    {
        foreach (var imageUrl in imageUrls)
        {
            using (var response = await _imgurService.DownloadImageAsync(imageUrl))
            {
                var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
                var filePath = Path.Combine("wwwroot", "images", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
        }
    }
}
