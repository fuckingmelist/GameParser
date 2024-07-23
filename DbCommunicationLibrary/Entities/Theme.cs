using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCommunicationLibrary.Entities
{
    public class Theme
    {
        public string Title { get; set; }
        public string PageLink { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
