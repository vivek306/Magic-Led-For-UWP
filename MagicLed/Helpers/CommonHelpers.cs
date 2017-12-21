using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLed.Helpers
{
    public class CommonHelpers
    {
        public const byte MAX_DELAY = 0x1f;
        public const int DISCOVERY_PORT = 48899;
        public const int MAX_BUFFER_SIZE = 1024;
        public const int CONNECT_PORT = 5577;
        public const int TIMEOUT = 2500;
        public static bool IsValidPattern(byte pattern)
        {
            if (pattern < 0x25 || pattern > 0x38)
                return false;
            else return true;
        }

    }
}
