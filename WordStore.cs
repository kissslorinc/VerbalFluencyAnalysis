using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VerbalFluencyAnalysis
{
    internal class WordStore : Dictionary<string, WordMetric>
    {
        private List<string> keyList = null;
        public int IndexOf(string key)
        {
            keyList ??= Keys.ToList();
            return keyList.IndexOf(key);
        }
    }
}
