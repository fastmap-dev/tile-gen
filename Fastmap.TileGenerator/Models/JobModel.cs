using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fastmap.TileGenerator.Models
{
    public class JobModel
    {
        public string Name { get; set; }
        public bool Completed { get; set; }
        public List<JobLevelModel> JobLevels { get; set; }
        public JobModel()
        {            
        }

        public JobModel(double lon1, double lat1, double lon2, double lat2, int fromLevel, int toLevel)
        {
            Name = $"Job: [{lon1},{lat1}] => [{lon2},{lat2}], level: [{fromLevel}, {toLevel}]";
            JobLevels = new List<JobLevelModel>();
            for(var level = fromLevel;level < toLevel; level++)
            {
                AppContext.LonLatToTile(lon1, lat1, level, out var tile1x, out var tile1y);
                AppContext.LonLatToTile(lon2, lat2, level, out var tile2x, out var tile2y);
                JobLevels.Add(new JobLevelModel(
                    x: Math.Min(tile1x, tile2x),
                    y: Math.Min(tile1y, tile2y),
                    z: level,
                    width: Math.Abs(tile1x - tile2x) + 1,
                    height: Math.Abs(tile1y - tile2y) + 1
                    ));
            }
            if (!JobLevels.Any(a => !a.IsDone())) Completed = true;
            //TODO: convert =>
        }        

        internal void NextTile()
        {
            var joblevel = JobLevels.FirstOrDefault(a => !a.IsDone());
            if (joblevel != null)
            {
                joblevel.NextTile();
            }
            if (!JobLevels.Any(a => !a.IsDone())) Completed = true;
        }

        public double GetJobPercentage()
        {
            var totalTile = JobLevels.Sum(a => a.GetTileCount());
            var finishedTile = JobLevels.Sum(a => a.GetCompletedTileCount());
            if (totalTile > 0)
            {
                return (double)finishedTile / (double)totalTile ;
            }
            return 0;
        }
    }

    public class JobLevelModel
    {
        public JobLevelModel(int x, int y, int z, int width, int height)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            CurrentX = x;
            CurrentY = y;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int CurrentX { get; set; }
        public int CurrentY { get; set; }

        public bool IsDone()
        {
            return !((CurrentX >= X) && (CurrentX < X + Width) && (CurrentY >= Y) && (CurrentY < Y + Height));
        }

        internal decimal GetCompletedTileCount()
        {
            return (CurrentX - X) * Height + CurrentY;
        }

        internal decimal GetTileCount()
        {
            return Width * Height;
        }

        internal void NextTile()
        {
            if (CurrentY < Y + Width - 1) CurrentY++;
            else
            {
                CurrentX++;
                CurrentY = Y;
            }
        }
    }
}
