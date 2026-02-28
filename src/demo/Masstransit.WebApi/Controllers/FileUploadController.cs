using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.Masstransit.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FileUploadController : ControllerBase
{

    /// <summary>
    /// This is an example of file Upload using API
    /// Use it as a reference.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="fileTag"></param>
    /// <returns></returns>
    [Route("")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostUploadAndEvaluate([FromForm(Name = "docs")] List<IFormFile> files, [FromQuery] string fileTag)
    {
        if (string.IsNullOrWhiteSpace(fileTag))
        {
            return BadRequest("fileTag cannot be null or empty");
        }

        if (files is null || !files.Any())
        {
            return BadRequest("files cannot be null or empty");
        }

        await Task.CompletedTask;
        return Ok("done");
    }
}
