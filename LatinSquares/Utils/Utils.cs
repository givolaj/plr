using LatinSquares.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security;
using System.Web;
using static LatinSquares.Models.DbModels;

namespace LatinSquares
{
    public static class Utils
    {
        public readonly static string[] SYMBOLS = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n" ,"o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

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

        internal static void SaveRectanlgeToDb(Rectangle sq, int rows, int cols, int symbols, int count, string type)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate {    
                    try
                    {
                        using (ApplicationDbContext db = new ApplicationDbContext())
                        {
                            DbRectangle r = new DbRectangle()
                            {
                                Rows = rows,
                                Cols = cols,
                                Symbols = symbols,
                                Count = count,
                                Type = type,
                                Content = sq.GetPlainTextString()
                            };
                            db.SaveRectangleIfNotInDb(r);
                        }
                    }
                    catch (Exception) { }
            }, null);         
        }

        internal static void SaveRectanlgeToDb(string content, string type)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    using (ApplicationDbContext db = new ApplicationDbContext())
                    {
                        var contentRows = content.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries); ;
                        int rows = contentRows.Count();
                        int cols = contentRows[0].Split(' ').Count();
                        int[] symbolsArr = new int[SYMBOLS.Count()];
                        int count = rows * cols;

                        foreach (var contentRow in contentRows)
                        {
                            for (int i = 0; i < contentRow.Length; i++)
                            {
                                if (contentRow[i].ToString() == Rectangle.EMPTY) count--;
                                if (Char.IsLetterOrDigit(contentRow[i]))
                                {
                                    symbolsArr[Array.FindIndex(SYMBOLS, x => x == contentRow[i].ToString())]++;
                                }
                            }
                        }
                        int symbols = symbolsArr.Where(x => x > 0).Count();

                        DbRectangle r = new DbRectangle()
                        {
                            Rows = rows,
                            Cols = cols,
                            Symbols = symbols,
                            Count = count,
                            Type = type,
                            Content = content
                        };
                        db.SaveRectangleIfNotInDb(r);
                    }
                }
                catch (Exception) { }
            }, null);
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
            var min = Math.Max(Math.Max(rows, cols), symbols);
            if (howManyFullSlots < min) howManyFullSlots = min;
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
                KillProcessAndChildrens(proc.Id);
                return output;
            }
        }

        private static void KillProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
        }

        public static string Format(string input)
        {
            string newInput = "";
            string[] lines = input.Split('\n');
            foreach (var line in lines)
            {
                int rows = (int) Math.Sqrt(line.Length);
                int cols = rows;
                string newLine = line.Replace('0', Rectangle.EMPTY[0]);
                string rectString = "";
                for (int i = 0; i < rows; i++)
                {
                    string newRow = "[" + string.Join(" ", newLine.Substring(i*cols, cols).ToCharArray().Select(x => x + "").ToArray()) + "]\n";
                    rectString += newRow;
                }
                newInput += rectString + "\n\n";
            }
            return newInput;
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

        internal static string GetRectangleFromDB(int rows, int cols, int symbols, int count, string type, int number)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var rects = db.Rectangles
                    .Where(x => x.Type == type)
                    .Where(x => x.Rows == rows)
                    .Where(x => x.Cols == cols)
                    .Where(x => x.Symbols == symbols)
                    .Where(x => x.Count == count).ToList();

                if (rects == null || rects.Count() == 0) return "no such PLR in out db.. :(";

                string result = "";
                while (number-- > 0 && rects.Count() > 0)
                {
                    int index = new Random().Next(rects.Count());
                    result += rects.ToList()[index].Content + (number == 0 ? "" : "\n\n");
                    rects.RemoveAt(index);
                }


                  
                return result;
            }
        }

        public static T[] GetRow<T>(T[,] matrix, int row)
        {
            var columns = matrix.GetLength(1);
            var array = new T[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = matrix[row, i];
            return array;
        }

        public static T[,] TransposeRowsAndColumns<T>(this T[,] arr)
        {
            int rowCount = arr.GetLength(0);
            int columnCount = arr.GetLength(1);
            T[,] transposed = new T[columnCount, rowCount];
            if (rowCount == columnCount)
            {
                transposed = (T[,])arr.Clone();
                for (int i = 1; i < rowCount; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        T temp = transposed[i, j];
                        transposed[i, j] = transposed[j, i];
                        transposed[j, i] = temp;
                    }
                }
            }
            else
            {
                for (int column = 0; column < columnCount; column++)
                {
                    for (int row = 0; row < rowCount; row++)
                    {
                        transposed[column, row] = arr[row, column];
                    }
                }
            }
            return transposed;
        }
    }
}