using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageProcessingTool.Services;

namespace ImageProcessingTool.Pages
{
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
            }
        }
    }
}
