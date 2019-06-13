using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fastmap.TileGenerator
{
    [Route("api/[controller]")]
    public class JobController : Controller
    {
        private readonly AppContext _appContext;
        public JobController(AppContext appContext)
        {
            _appContext = appContext;
        }
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Models.JobModel> Get()
        {
            return _appContext.GetJobs();
        }
    }
}
