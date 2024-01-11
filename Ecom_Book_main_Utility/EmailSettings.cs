using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom_Book_main_Utility
{
    public class EmailSettings
    {
        public string PrimaryDomain { get; set; }
        public int PrimaryPort { get; set; }
        public string SecondaryDomain { get; set; }
                     
        public int SecondaryPort { get; set; }
        public string UserNameEmail { get; set; }
        public string UserNamePassword { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string CCEmail { get; set; }

    }
}
