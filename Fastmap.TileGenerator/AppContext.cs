using Fastmap.TileGenerator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fastmap.TileGenerator
{
    public class AppContext
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        List<Models.JobModel> _jobs = null;
        System.Timers.Timer _saveJobTimer = new System.Timers.Timer(1000 * 60);
        private bool Closed = false;

        public AppContext(ILogger<AppContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var jobfile = "job.json";
            if (File.Exists(jobfile))
            {
                _jobs = JsonConvert.DeserializeObject<List<Models.JobModel>>(File.ReadAllText("job.json"));
            }

            #region seeding
            if (_jobs == null)
            {
                _jobs = new List<JobModel>();                
                _jobs.Add(new Models.JobModel(106.000000, 10.000000, 107.000000, 11.000000, fromLevel: 0, toLevel: 10));
                _jobs.Add(new Models.JobModel(106.000000, 10.000000, 107.000000, 11.000000, fromLevel: 0, toLevel: 18));
            }
            #endregion
        }

        internal IEnumerable<JobModel> GetJobs()
        {
            return _jobs;
        }

        public void Start()
        {
            _saveJobTimer.Elapsed += _saveJobTimer_Elapsed;
            _saveJobTimer.Start();

            new Thread(() => ProcessJob()).Start();

            _logger.LogInformation("Application started!");
        }

        private void _saveJobTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SaveJobs();
        }

        private void ProcessJob()
        {
            while (!Closed)
            {                
                foreach (var job in _jobs.Where(a => !a.Completed))
                {
                    //get un-finished joblevel
                    var joblevel = job.JobLevels.FirstOrDefault(a => !a.IsDone());
                    if (joblevel != null)
                    {
                        ProcessTile(joblevel.CurrentX, joblevel.CurrentY, joblevel.Z);
                        job.NextTile();
                        break;
                    }
                }
            }            
        }

        private void ProcessTile(int x, int y, int z)
        {
            //TOOD: create request to tile service
            var url = string.Format(_configuration["TileServiceUrl"], z, x, y);
            _logger.LogInformation(url);
            Thread.Sleep(100);
        }

        public void Stop()
        {
            Closed = true;
            SaveJobs();
        }

        private void SaveJobs()
        {
            _logger.LogInformation("save current job to file");
            var jobfile = "job.json";
            File.WriteAllText(jobfile, JsonConvert.SerializeObject(_jobs, Formatting.Indented));
            _logger.LogInformation("Application stopped!");
        }


        /// <summary>
        /// with tile size = 256
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="zoom"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void LonLatToTile(double lon, double lat, int zoom, out int x, out int y)
        {
            x = (int)((lon + 180.0) / 360.0 * (1 << zoom));
            y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));
        }
        /// <summary>
        /// with tile size = 256
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="zoom"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void LonLatToPixel(double lon, double lat, int zoom, out int x, out int y)
        {
            x = (int)(((lon + 180.0) / 360.0 * (1 << zoom)) * 256);
            y = (int)(((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom)) * 256);
        }
        /// <summary>
        /// with tile size = 256
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        public static void PixelToLonLat(int zoom, int x, int y, out double lon, out double lat)
        {
            double tilex = (double)x / 256;
            double tiley = (double)y / 256;
            double n = Math.PI - ((2.0 * Math.PI * tiley) / Math.Pow(2.0, zoom));

            lon = (float)((tilex / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            lat = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
        }

    }
}
