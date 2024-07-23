using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCommunicationLibrary.Entities
{
    public class Game
    {
        public string Title { get; set; }
        public double AvgRating { get; set; }
        public string Date { get; set; }
        public List<string> Genre { get; set; } = new List<string>();
        public List<string> Platforms { get; set; } = new List<string>();
        public string ImgSrc { get; set; }
        public string PageLink { get; set; }
        public List<Theme> Themes { get; set; } = new List<Theme>();
    }
}
