using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RendleLabs.HttpFiles.Services;

namespace RendleLabs.HttpFiles.Controllers
{
    [Route("put")]
    [ApiController]
    public class PutController : ControllerBase
    {
        private readonly IFiles _files;

        public PutController(IFiles files)
        {
            _files = files;
        }

        [HttpPut("{**file}")]
        public async Task<ActionResult> PutText(string file)
        {
            await _files.WriteTextAsync(file, Request.Body);
            return CreatedAtAction("Get", "Cat", new {file}, null);
        }
    }
}