using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;

namespace LatinSquares.Models
{
    public class Partition
    {
        public Dictionary<int, List<int>> Groups { get; set; }

        public Partition(string pStr, int type = 0)
        {
            Groups = new Dictionary<int, List<int>>();
            var parts = pStr.Replace("[", "").Replace("]", "")
                .Split(new string[] { "}," }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Count(); i++)
            {
                List<int> p = new List<int>();
                string[] items = parts[i].Replace("{", "").Replace("}", "").Split(',');
                foreach (var it in items)
                {
                    var index = Array.IndexOf(Utils.SYMBOLS, it.Trim());
                    if (index == -1)
                        p.Add(it.AsInt());
                    else p.Add(index);
                }
                Groups.Add(i + 1, p);
            }
        }

        public string AsString()
        {
            string rowsString = "[";
            foreach (var p in Groups)
            {
                rowsString += "{";
                foreach (int n in p.Value)
                {
                    rowsString += (n + 1) + ",";
                }
                rowsString = rowsString.Substring(0, rowsString.Length - 1) + "},";
            }
            rowsString = rowsString.Substring(0, rowsString.Length - 1) + "]";
            return rowsString;
        }
    }
}