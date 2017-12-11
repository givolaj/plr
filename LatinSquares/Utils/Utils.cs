using LatinSquares.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;

namespace LatinSquares
{
    public static class Utils
    {
        public readonly static string[] SYMBOLS = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n" ,"o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        private static Random random = null;
        public static Random GetGlobalRandomGenerator()
        {
            if (random == null)
                random = new Random();
            return random;
        }

        public static Rectangle GetEmptyRectangle(int rows, int cols)
        {
            return new Rectangle(rows, cols);
        }

        public static Rectangle GetRandomUnitRectangle(int rows, int cols, int symbols)
        {
            Rectangle sq = new Rectangle(rows, cols);
            int maxSize = Math.Max(Math.Max(rows, cols), symbols);
            List<int> xList = new List<int>();
            List<int> yList = new List<int>();
            List<string> sList = new List<string>();
            Random r = GetGlobalRandomGenerator();
            for (int i = 0; i < maxSize; i++)
            {
                if (i < rows)
                    xList.Add(i);
                else
                    xList.Add(r.Next(0, rows));
            }
            for (int i = 0; i < maxSize; i++)
            {
                if (i < cols)
                    yList.Add(i);
                else
                    yList.Add(r.Next(0, cols));
            }
            for (int i = 0; i < maxSize; i++)
            {
                if (i < symbols)
                    sList.Add(SYMBOLS[i]);
                else
                    sList.Add(SYMBOLS[r.Next(0, symbols)]);
            }

            while(sList.Count > 0)
            {
                int rowIndex = r.Next(0, xList.Count);
                int colIndex = r.Next(0, yList.Count);
                int sIndex = r.Next(0, sList.Count);

                if (sq.CanSetValue(sList[sIndex], xList[rowIndex], yList[colIndex]))
                {
                    sq.Set(sList[sIndex], xList[rowIndex], yList[colIndex]);
                    xList.RemoveAt(rowIndex);
                    yList.RemoveAt(colIndex);
                    sList.RemoveAt(sIndex);
                }
            }

            return sq;
        }

        public static Rectangle GetRectangle(int rows, int cols, int symbols, int howManyFullSlots)
        {
            Rectangle sq = GetRandomUnitRectangle(rows, cols, symbols);
            Random r = GetGlobalRandomGenerator();
            FillRectangle(sq, r, symbols, howManyFullSlots - Math.Max(rows, cols));
            return sq;
        }

        public static bool FillRectangle(Rectangle sq, Random r, int symbols, int howManyFullSlots)
        {
            if (howManyFullSlots == 0) return true;

            int symbolIndex = r.Next(symbols);
            int row, col;
            GetRandomIndex(r, sq.GetRowsNumber(), sq.GetColumnsNumber(), out row, out col);
            int initialRow = row,
                initialCol = col;
            do
            {
                int _symbols = symbols;
                while (!sq.CanSetValue(SYMBOLS[(symbolIndex + _symbols) % symbols], row, col) && _symbols-- > 0);
                if (_symbols < 0)
                {
                    sq.NextIndex(row, col, out row, out col);
                    if (col == -1) return false;
                }
                else
                {
                    sq.Set(SYMBOLS[(symbolIndex + _symbols) % symbols], row, col);
                    bool success = FillRectangle(sq, r, symbols, howManyFullSlots - 1);
                    if (success)
                        return true;
                    else
                    {
                        sq.Set(Rectangle.EMPTY, row, col);
                        symbolIndex = r.Next(symbols);
                        GetRandomIndex(r, sq.GetRowsNumber(), sq.GetColumnsNumber(), out row, out col);
                        initialRow = row;
                        initialCol = col;
                        sq.NextIndex(row, col, out row, out col);
                    }
                }
            } while (col != initialCol || row != initialRow);
            return false;
        }

        public static Rectangle GetFullRectangle(int rows, int cols, int symbols, int count)
        {
            int dim = rows > cols ? rows : cols;
            dim = dim > symbols ? dim : symbols;
            Cube c = new Cube(dim);
            c.initAsUnitCube();
            c.runJacobsonMatthewsSteps(1000);
            Rectangle sq = c.toRectangle(rows, cols);
            sq.RemoveValuesToMatchCount(count);
            return sq;
        }

        public static void GetRandomIndex(Random r, int rowsMax, int colsMax, out int row, out int col)
        {
            row = r.Next(rowsMax);
            col = r.Next(colsMax);
        }

        public static class CommandLine
        {
            public static string Execute(string executable,
                string arguments)
            {
                string output = "";
                ProcessStartInfo psi =
                new ProcessStartInfo(executable);
                psi.RedirectStandardOutput = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = false;
                psi.Arguments = arguments;
                Process proc = Process.Start(psi);
                while (proc.StandardOutput.Peek() > -1)
                {
                    output += proc.StandardOutput.ReadLine() + "\n";
                }
                proc.WaitForExit();
                return output;
            }
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
    }
}