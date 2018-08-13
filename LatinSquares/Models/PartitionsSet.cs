using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LatinSquares.Models
{
    public class PartitionsSet
    {
        public Partition Rows { get; set; }
        public Partition Columns { get; set; }
        public Partition Symbols { get; set; }

        public PartitionsSet(string rows, string cols, string symbols)
        {
            Rows = new Partition(rows);
            Columns = new Partition(cols);
            Symbols = new Partition(symbols);
        }

        public Dictionary<string, string> AsDictionary(bool useSymbols = false)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("rows", Rows.AsString());
            dict.Add("cols", Columns.AsString());
            dict.Add("symbols", Symbols.AsString());
            return dict;
        }

        public string AsString(bool useSymbols = false)
        {
            return Rows.AsString() + Columns.AsString() + Symbols.AsString();
        }
    }
}