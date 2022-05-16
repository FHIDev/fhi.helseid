using System;

namespace Fhi.HelseId.Common
{
    public class BaseAddressUtil
    {
        public static Uri ToUri(string baseadress)
        {
            return new Uri(baseadress);
        }
    }
}
