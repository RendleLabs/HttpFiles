using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RendleLabs.HttpFiles.Models;
using RendleLabs.HttpFiles.Services;

namespace RendleLabs.HttpFiles.Controllers
{
    [Route("ls")]
    [ApiController]
    public class ListController : ControllerBase
    {
        private readonly IDirectories _directories;

        public ListController(IDirectories directories)
        {
            _directories = directories;
        }

        [HttpGet("{*directory}")]
        public async Task<ActionResult<FileList>> Get(string directory, [FromQuery]string pattern)
        {
            var result = await _directories.ListFilesAsync(directory, pattern);
            return result ?? (ActionResult<FileList>) NotFound();
        }
    }
}
