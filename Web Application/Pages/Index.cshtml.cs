using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    [BindProperty]
    public IFormFile ImageFile { get; set; }

    [BindProperty]
    public string CroppedImageData { get; set; }

    public string JsonResponse { get; set; }

    private readonly IHttpClientFactory _clientFactory;

    public IndexModel(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ImageFile != null && ImageFile.Length > 0)
        {
            using (var stream = new MemoryStream())
            {
                await ImageFile.CopyToAsync(stream);
                var byteArray = stream.ToArray();
            }
        }

        if (!string.IsNullOrEmpty(CroppedImageData))
        {
            var base64Data = CroppedImageData.Substring(CroppedImageData.IndexOf(",") + 1);
            var byteArray = Convert.FromBase64String(base64Data);

            using (var client = _clientFactory.CreateClient())
            {
                var content = new ByteArrayContent(byteArray);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await client.PostAsync("https://torchserve-mobilenetv3.onrender.com/predictions/mv3l_aug", content);

                if (response.IsSuccessStatusCode)
                {
                    JsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(JsonResponse);
                }
                else
                {
                    JsonResponse = "Error processing the cropped image.";
                    Console.WriteLine(JsonResponse);
                }
            }
        }

        return new JsonResult(new { JsonResponse = JsonResponse });
    }


}
