using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// meme controller der bilder liefert selber implenmtieren... dann braucht man den image path kram nicht


namespace MemeBoard
{
    public class ApiController : Controller
    {
        private readonly MemeRepo repo;

        public ApiController(MemeRepo repo)
        {
            this.repo = repo;
        }
        
        [Route("Api/Upload")]
        [HttpPost()]
        public IActionResult Upload(List<IFormFile> files)
        {
            foreach (var file in files.Where(f => f.ContentType.StartsWith("image/")))
            {
                var target = Path.Combine(this.repo.Path, file.FileName);
                using (var stream = new FileStream(target, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

            return Ok(200);
        }
    }
}
