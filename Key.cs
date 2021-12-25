using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XiaowuStudioFileOperation
{
    public class Key
    {
        public static string wujiayang2007KeyB64Encode()
        {
            string str = "wujiayang2007";
            for (int i = 0; i < 5; i++)
            {
                str = Utils.B64.Base64Encode(str);
            }
            return str;
        }
    }
}
