using System;
using System.Collections.Generic;
using System.Text;

namespace IR
{
    public static class ExtensionMethods
    {
        public static bool IsInteger(this string self)
        {
            return int.TryParse(self, out int result);
        }
    }
}
