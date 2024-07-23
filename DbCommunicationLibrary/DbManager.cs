using DbCommunicationLibrary.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCommunicationLibrary
{
    public class DbManager
    {
        public DbManager()
        {
            GamelinkTable = new GamelinkTable();
            GameTable = new GameTable();
        }
        public GamelinkTable GamelinkTable { get; set; }
        public GameTable GameTable { get; set; }
    }
}
