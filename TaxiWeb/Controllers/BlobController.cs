using AzureStorageWrapper;
using Contracts.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Blob;

namespace TaxiWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobController : ControllerBase
    {
        private readonly IAuthService authService;

        public BlobController(IAuthService authService)
        {
            this.authService = authService;
        }


        [HttpPost]
        [AllowAnonymous]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [Route("upload-profile-image/{hashedEmail}")]
        public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile file, [FromForm] string fileName)
        {
            object? email = null;
            Request.RouteValues.TryGetValue("hashedEmail", out email);

            if (email == null || file == null || file.Length == 0)
            {
                return BadRequest("Invalid parameters");
            }

            var fileExtenstion = fileName.Split('.')[1];
            var blobName = $"{email}.{fileExtenstion}";

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                AzureStorageWrapper.AzureBlobWrapper blobWrapper = new AzureBlobWrapper("AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;", "profile-images");
                memoryStream.Position = 0;
                var res = await blobWrapper.UploadBlob(blobName, memoryStream);
                if(res == null)
                {
                    return BadRequest("Failed to upload image");
                }
                return Ok(res);
            }
        }
    }
}
