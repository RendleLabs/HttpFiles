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
        
        [HttpGet("{**directory}")]
        public ActionResult<FileList> Get(string directory, [FromQuery]string pattern)
        {
            return new FileList {Files = _directories.ListFiles(directory, pattern).ToArray()};
        }
    }
}
