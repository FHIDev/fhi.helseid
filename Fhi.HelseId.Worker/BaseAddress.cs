using System;

namespace Fhi.HelseId.Worker
{
    public class BaseAddressUtil
    {
        public static Uri ToUri(string baseadress)
        {
            return new Uri(baseadress.TrimEnd('/'));
        }
    }
}
