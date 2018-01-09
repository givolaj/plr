using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LatinSquares.Models
{
    public class Rectangle
    {
        public string[,] values;
        public static string EMPTY = ".";

        public Rectangle(int rows, int cols)
        {
            values = new string[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    values[i, j] = EMPTY;
                }
            }
        }

        public bool HasValueInRow(string value, int row)
        {
            for (int i = 0; i < GetColumnsNumber(); i++)
            {
                if (values[row, i] == value) return true;
            }
            return false;
        }

        internal int BiggestDimension()
        {
            int rows = GetRowsNumber();
            int cols = GetColumnsNumber();
            int symbols = SymbolCount();
            if (symbols > rows && symbols > cols) return symbols;
            else if (rows > symbols && rows > cols) return rows;
            else return cols;
        }

        public bool HasValueInCol(string value, int col)
        {
            for (int i = 0; i < GetRowsNumber(); i++)
            {
                if (values[i, col] == value) return true;
            }
            return false;
        }


        public bool CanSetValue(string value, int row, int col)
        {
            if (value == EMPTY) return true;
            return values[row, col] == EMPTY && !HasValueInRow(value, row) && !HasValueInCol(value, col);
        }

        public void Set(string value, int row, int col)
        {
            values[row, col] = value;
        }

        public override string ToString()
        {
            string s = "<pre><code>";
            for (int i = 0; i < GetRowsNumber(); i++)
            {
                s += "[";
                for (int j = 0; j < GetColumnsNumber(); j++)
                {
                    s += "<span class='matrix-cell row-" + i + " col-" + j + "'>" + values[i, j] + "</span>";
                    if (j != GetColumnsNumber() - 1)
                        s += " ";
                }
                s += "]<br />";
            }
            s += "</code></pre>";
            return s;
        }

        public string GetPlainTextString()
        {
            string s = "";
            for (int i = 0; i < GetRowsNumber(); i++)
            {
                s += "[";
                for (int j = 0; j < GetColumnsNumber(); j++)
                {
                    s += values[i, j];
                    if (j != GetColumnsNumber() - 1)
                        s += " ";
                }
                s += "]\n";
            }
            s += "\n\n";
            return s;
        }

        public void NextIndex(int row, int col, out int outRow, out int outCol)
        {
            if (col == GetColumnsNumber() - 1)
            {
                if (row == GetRowsNumber() - 1)
                {
                    outRow = outCol = 0;
                }
                else
                {
                    outRow = row + 1;
                    outCol = 0;
                }
            }
            else
            {
                outRow = row;
                outCol = col + 1;
            }
        }

        public void NextEmptyIndex(int row, int col, out int outRow, out int outCol)
        {
            int total = GetColumnsNumber() * GetRowsNumber();
            while (values[row, col] != EMPTY && total-- > 0)
                NextIndex(row, col, out row, out col);
            if (total <= 0)
                outCol = outRow = -1;
            else
            {
                outCol = col;
                outRow = row;
            }
        }

        public void RemoveValuesToMatchCount(int count)
        {
            Random rnd = new Random();
            int iterations = values.GetLength(0) * values.GetLength(1) - count;
            int iterationsCount = 0;
            while (iterations-- > 0)
            {

                if (iterationsCount++ > 10000) break;
                List<string> triplets = GetTripletsList();
                int r = rnd.Next(triplets.Count);
                string[] indices = triplets[r].Split(',');
                int i = int.Parse(indices[0]);
                int j = int.Parse(indices[1]);
                if (!CanRemoveWithoutEmptyLines(i,j))
                {
                    iterations++;
                    continue;
                }
                values[i, j] = EMPTY;
            }
        }

        private List<string> GetTripletsList()
        {
            var list = new List<string>();
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if (values[i, j] != EMPTY)
                    {
                        list.Add(i + "," + j + "," + values[i,j]);
                    }
                }
            }
            return list;
        }

        private bool CanRemoveWithoutEmptyLines(int i, int j)
        {
            bool resultRow = false, resultCol = false;
            for (int k = 0; k < values.GetLength(0); k++)
            {
                if (k == i) continue;
                if (values[k, j] != EMPTY) { resultRow = true; break; }
            }

            for (int k = 0; k < values.GetLength(1); k++)
            {
                if (k == j) continue;
                if (values[i, k] != EMPTY) { resultCol = true; break; }
            }

            return resultRow && resultCol;
        }

        public int GetColumnsNumber()
        {
            return values.GetLength(1);
        }

        public int GetRowsNumber()
        {
            return values.GetLength(0);
        }

        public override bool Equals(object obj)
        {
            var item = obj as Rectangle;
            if (item == null)
            {
                return false;
            }
            return this.GetPlainTextString().Equals(item.GetPlainTextString());
        }

        public override int GetHashCode()
        {
            return this.values.GetHashCode();
        }

        public int GetNumberOfOccurencesOfSymbol(int x, int y)
        {
            int count = 0;
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if (values[i, j] == values[x, y]) count++;
                }
            }
            return count;
        }

        public int GetColumnNonEmptySymbolCount(int col)
        {
            int count = 0;
            for (int i = 0; i < values.GetLength(0); i++)
            {
                if (values[i, col] != EMPTY) count++;
            }
            return count;
        }

        public int GetRowNonEmptySymbolCount(int row)
        {
            int count = 0;
            for (int j = 0; j < values.GetLength(1); j++)
            {
                if (values[row, j] != EMPTY) count++;
            }
            return count;
        }

        public int SymbolCount()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if(!list.Contains(values[i, j]) && values[i, j] != EMPTY)
                        list.Add(values[i, j]);
                }
            }
            return list.Count;
        }

    }
}