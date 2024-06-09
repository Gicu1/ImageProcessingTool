using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly ImgurService _imgurService;
    public List<string> Images { get; set; }

    public IndexModel(ImgurService imgurService)
    {
        _imgurService = imgurService;
        Images = new List<string>();
    }

    public async Task OnGetAsync()
    {
        Images = await _imgurService.GetImagesAsync("cats");
    }

}
