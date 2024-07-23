using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCommunicationLibrary.Entities
{
    public class Comment
    {
        public string Text { get; set; }
        public string UserNickname { get; set; }
        public string DateTime { get; set; }
    }
}
