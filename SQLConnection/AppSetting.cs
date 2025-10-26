using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLConnection
{
    public class AppSetting
    {
        public static string ConnectionString()
        {
            return "Data Source = data.db;Version = 3;";
        }
    }
}
