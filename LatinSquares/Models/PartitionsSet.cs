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
            Rows = new Partition(rows, 0);
            Columns = new Partition(cols, 1);
            Symbols = new Partition(symbols, 2);
        }

        public Dictionary<string, string> AsDictionary(bool useSymbols = false)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("rows", Rows.AsString());
            dict.Add("cols", Columns.AsString());
            dict.Add("symbols", Symbols.AsString(true));
            return dict;
        }

        public string AsString()
        {
            return Rows.AsString() + Columns.AsString() + Symbols.AsString(true);
        }

        public PartitionsSet WithNewOrder(int a, int b, int c)
        {
            return new PartitionsSet(a == 0 ? Rows.AsString() : (a == 1 ? Columns.AsString() : Symbols.AsString(true)) ,
                b == 0 ? Rows.AsString() : (b == 1 ? Columns.AsString() : Symbols.AsString(true)),
                c == 0 ? Rows.AsString() : (c == 1 ? Columns.AsString() : Symbols.AsString(true))
                );
        }
    }
}