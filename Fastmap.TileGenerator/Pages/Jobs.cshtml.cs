using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fastmap.TileGenerator.Pages
{
    public class JobsModel : PageModel
    {
        private AppContext _appContext;
        public JobsModel(AppContext appContext)
        {
            _appContext = appContext;
        }
        public IEnumerable<Models.JobModel> Jobs { get; set; }
        public void OnGet()
        {
            Jobs = _appContext.GetJobs();
        }
    }
}