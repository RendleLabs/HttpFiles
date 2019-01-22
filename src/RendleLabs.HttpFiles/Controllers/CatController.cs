using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RendleLabs.HttpFiles.Models;
using RendleLabs.HttpFiles.Services;

namespace RendleLabs.HttpFiles.Controllers
{
    [Route("cat")]
    [ApiController]
    public class CatController : ControllerBase
    {
        private readonly IFiles _files;

        public CatController(IFiles files)
        {
            _files = files;
        }

        [HttpGet("{**file}")]
        public async Task<ActionResult<string>> Get(string file)
        {
            Response.SendFileAsync()
            var text = await _files.ReadAsync(file);
            return text ?? (ActionResult<string>) NotFound();
        }
    }
}