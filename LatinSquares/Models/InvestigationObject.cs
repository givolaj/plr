using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.WebPages;

namespace LatinSquares.Models
{
    public class InvestigationObject
    {
        private Rectangle square;
        private Cube cube;
        private string[,] tripletValues;
        private Dictionary<string, string[,]> comparisonMatrices;
        public Dictionary<string, List<string>> partitions;

        public InvestigationObject(string squareString)
        {
            square = ValidateAndReturnSquareString(squareString);
            if (square == null)
            {
                throw new Exception("this square string doesn't represent a valid latin square");
            }
            cube = new Cube(square);

            comparisonMatrices = new Dictionary<string, string[,]>();
            comparisonMatrices.Add("rows", GetComparisonMatrix(square.values));
            comparisonMatrices.Add("cols" ,GetComparisonMatrix(new Cube(square).
                GetCubeWithColumnsAsRowsTranspose().toRectangle(square.GetColumnsNumber(),
                square.GetRowsNumber()).values));
            comparisonMatrices.Add("symbols", GetComparisonMatrix(new Cube(square).
                GetCubeWithSymbolsAsRowsTranspose().toRectangle(square.GetColumnsNumber(),
                square.GetRowsNumber()).values));

            tripletValues = GetTriplietsFromSquare(square);

            partitions = new Dictionary<string, List<string>>();
            partitions.Add("rows", new List<string>() { GetPartitionsForRows(tripletValues) });
            partitions.Add("cols", new List<string>() {
                //GetPartitionsForRows(GetTripletsInOrder(tripletValues, 1,0,2)) });
                GetPartitionsForRows(
                GetTriplietsFromSquare(new Cube(square).
                GetCubeWithColumnsAsRowsTranspose().toRectangle(square.GetColumnsNumber(),
                square.GetRowsNumber())), true) });
            partitions.Add("symbols", new List<string>() { GetPartitionsForRows(
                GetTriplietsFromSquare(new Cube(square).
                GetCubeWithSymbolsAsRowsTranspose().toRectangle(square.SymbolCount(),
                square.GetColumnsNumber())), true) });

            partitions.Add("rowsG", new List<string>() { GetPartitionsForRows(comparisonMatrices["rows"]) });
            partitions.Add("colsG", new List<string>() { GetPartitionsForRows(comparisonMatrices["cols"]) });
            partitions.Add("symbolsG", new List<string>() { GetPartitionsForRows(comparisonMatrices["symbols"]) });
        }

        public bool IsPartitionTrivial(bool G = false)
        {
            string pattern = @"\{[0-9a-zA-Z^\)]+\}";
            int a = Regex.Matches(partitions[G ? "rowsG" : "rows"][0], pattern).Count == square.GetRowsNumber() ? 1 : 0;
            int b = Regex.Matches(partitions[G ? "colsG" : "cols"][0], pattern).Count == square.GetColumnsNumber() ? 1 : 0;
            int c = Regex.Matches(partitions[G ? "symbolsG" : "symbols"][0], pattern).Count == square.SymbolCount() ? 1 : 0;
            return (a + b + c) >= 2;
        }

        private string[,] GetTriplietsFromSquare(Rectangle square)
        {
            var tripletValues = new string[square.GetRowsNumber(), square.GetColumnsNumber()];
            for (int i = 0; i < tripletValues.GetLength(0); i++)
            {
                for (int j = 0; j < tripletValues.GetLength(1); j++)
                {
                    if (square.values[i, j] == Rectangle.EMPTY)
                        tripletValues[i, j] = "   -   ";
                    else
                        tripletValues[i, j] = "(" +
                            square.GetRowNonEmptySymbolCount(i) + "," +
                            square.GetColumnNonEmptySymbolCount(j) + "," +
                            square.GetNumberOfOccurencesOfSymbol(i, j) + ")";
                }
            }
            return tripletValues;
        }

        private string[,] GetTripletsInOrder(string[,] tripletValues, int a, int b, int c)
        {
            string[,] colsTripletValues = new string[tripletValues.GetLength(1), tripletValues.GetLength(0)];
            for (int i = 0; i < tripletValues.GetLength(0); i++)
            {
                for (int j = 0; j < tripletValues.GetLength(1); j++)
                {
                    string triplet = tripletValues[i, j];
                    if (!triplet.Contains("-"))
                    {
                        var parts = triplet.Replace("(", "").Replace(")", "").Split(',');
                        triplet = "(" + parts[a] + "," + parts[b] + "," + parts[c] + ")";
                    }
                    colsTripletValues[j, i] = triplet;
                }
            }
            return colsTripletValues;
        }

        private string GetPartitionsForRows(string [,] tripletValues, bool useSymbols = false)
        {
            var rows = new List<string[]>();
            for (int i = 0; i < tripletValues.GetLength(0); i++)
            {
                rows.Add(Utils.GetRow<string>(tripletValues, i).OrderBy(x => x).ToArray());
            }
            var rowsPartitions = new Dictionary<int, List<int>>();
            for (int i = 0; i < rows.Count; i++)
            {
                rowsPartitions.Add(i, new List<int>());
                var r = rows[i];
                for (int j = 0; j < rows.Count; j++)
                {
                    if (r.SequenceEqual(rows[j]))
                    {
                        rowsPartitions[i].Add(j);
                    }
                }
            }
            List<int> done = new List<int>();
            string rowsString = "[";
            foreach (var p in rowsPartitions)
            {
                if (done.Contains(p.Key)) continue;
                done.AddRange(p.Value);
                rowsString += "{";
                foreach (int n in p.Value)
                {
                    rowsString += (useSymbols ? Utils.SYMBOLS[n] : ((n + 1) + "")) + ",";
                }
                rowsString = rowsString.Substring(0, rowsString.Length - 1) + "},";
            }
            rowsString = rowsString.Substring(0, rowsString.Length - 1) + "]";
            return rowsString;
        }

        public string AsString(string type)
        {
            if (type == "square")
            {
                return GenerateSquareString();
            }
            if (type == "cleanSquare")
            {
                return square.GetPlainTextString();
            }
            else if (type == "triplets")
            {
                return GenerateTripletsString();
            }
            else if (type == "tripletsIndexed")
            {
                return GenerateIndexedTripletsString();
            }
            else if (type == "all")
            {
                var obj = new
                {
                    square = GenerateSquareString(),
                    triplets = GenerateTripletsString(),
                    indexedTriplets = GenerateIndexedTripletsString(),
                    partitions = partitions,
                    comparisonMatrixRows = GetMatrixString(comparisonMatrices["rows"], square.GetRowsNumber()),
                    comparisonMatrixCols = GetMatrixString(comparisonMatrices["cols"], square.GetColumnsNumber()),
                    comparisonMatrixSymbols = GetMatrixString(comparisonMatrices["symbols"], square.SymbolCount())
                };
                return JsonConvert.SerializeObject(obj);
            }
            else
            {
                return "didn't recognize the type";
            }
        }

        private object GetMatrixString(string[,] values, int squareSize)
        {
            string mat = "<table class='table table-striped table-hover'><tr><th></th>";
            for (int i = 0; i < squareSize; i++)
            {
                mat += "<th>(" + i + ")</th>";
            }
            mat += "<tr>";

            for (int i = 0; i < values.GetLength(0); i++)
            {
                mat += "<tr><th>(" + i + ")</th>";
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    mat += "<td>" + values[i, j] + "</td>";
                }
                mat += "<tr>";
            }
            return mat + "</table>";
        }

        private string GenerateIndexedTripletsString()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            for (int i = 0; i < tripletValues.GetLength(0); i++)
            {
                for (int j = 0; j < tripletValues.GetLength(1); j++)
                {
                    if (!map.ContainsKey(tripletValues[i, j]))
                    {
                        if(tripletValues[i, j] == "   -   ")
                            map.Add(tripletValues[i, j], Rectangle.EMPTY);
                        else
                            map.Add(tripletValues[i, j], Utils.SYMBOLS[map.Count + 1]);
                    }
                }
            }

            string tripletsString = "<pre><code>";
            for (int i = 0; i < tripletValues.GetLength(0); i++)
            {
                tripletsString += "[";
                for (int j = 0; j < tripletValues.GetLength(1); j++)
                {
                    tripletsString += "<span class='matrix-cell row-" + i + " col-" + j + "'>" + map[tripletValues[i, j]] + "</span>";
                    if (j != tripletValues.GetLength(1) - 1)
                        tripletsString += " ";
                }
                tripletsString += "]<br />";
            }
            tripletsString += "</code></pre>";
            return tripletsString;
        }

        private string GenerateTripletsString()
        {
            string tripletsString = "<pre><code>";
            for (int i = 0; i < tripletValues.GetLength(0); i++)
            {
                tripletsString += "[";
                for (int j = 0; j < tripletValues.GetLength(1); j++)
                {
                    tripletsString += "<span class='matrix-cell row-" + i + " col-" + j + "'>" + tripletValues[i, j] + "</span>";
                    if (j != tripletValues.GetLength(1) - 1)
                        tripletsString += " ";
                }
                tripletsString += "]<br />";
            }
            tripletsString += "</code></pre>";
            return tripletsString;
        }

        private string GenerateSquareString()
        {
            return square.ToString();
        }

        public static Rectangle ValidateAndReturnSquareString(string squareString)
        {
            if (string.IsNullOrEmpty(squareString)) return null;
            string[] lines = squareString.Split(']');
            Rectangle sq = new Rectangle(lines.Count()-1, lines[0].Split(' ').Count());
            for(int i=0;i<lines.Count()-1;i++)
            {
                string l = lines[i].Replace("[", "").Replace("]","");
                string[] cells = l.Split(' ');
                for (int j = 0; j < cells.Count(); j++)
                {
                    if (sq.CanSetValue(cells[j], i, j))
                        sq.Set(cells[j], i, j);
                    else
                        return null;
                }
            }
            return sq;
        }

        private int IndexForValuesRow(string[,] values, int rowIndex, string value)
        {
            for (int i = 0; i < values.GetLength(1); i++)
            {
                if (values[rowIndex, i] == value) return i;
            }
            return -1;
        }

        private List<string> GetEdgesForTwoRows(string[,] values, int index1, int index2)
        {
            List<string> edges = new List<string>();
            for (int i = 0; i < values.GetLength(1); i++)
            {
                if (values[index1, i] != Rectangle.EMPTY)
                {
                    if(values[index2, i] != Rectangle.EMPTY)
                        edges.Add(i + "-" + i);
                    int index = IndexForValuesRow(values, index2, values[index1, i]);
                    if (index != -1)
                    {
                        edges.Add(i + "-" + index); 
                    }
                }
            }
            return edges;
        }

        private void GetPathForEdge(string edge, List<string> edges, List<string> path)
        {
            path.Add(edge);
            int up, down;
            string[] parts = edge.Split('-');
            up = parts[0].AsInt();
            down = parts[1].AsInt();
            foreach (var e in edges)
            {
                if (path.Contains(e)) continue;
                if (up.ToString() == e[0].ToString() || down.ToString() == e[2].ToString())
                    GetPathForEdge(e, edges, path);
            }
        }

        private List<string> GetGraphForTwoRows(string[,] values, int index1, int index2)
        {
            List<List<string>> graphRaw = new List<List<string>>();
            var edges = GetEdgesForTwoRows(values, index1, index2);
            while (edges.Count > 0)
            {
                var path = new List<string>();
                GetPathForEdge(edges[0], edges, path);
                foreach (var e in path)
                {
                    edges.Remove(e);
                }
                graphRaw.Add(path);
            }
            var graph = new List<string>();
            foreach (var p in graphRaw)
            {
                int up = p.Select(x => x.Split('-')[0]).Distinct().Count();
                int down = p.Select(x => x.Split('-')[1]).Distinct().Count();
                int count = p.Count;
                int diagonal = p.Where(x => x.Split('-')[0] == x.Split('-')[1]).Count();
                graph.Add(up + "-" + down + "-" + count + "-" + diagonal);
            }
            return graph;
        }


        private bool IsIsomorphic(List<string> graph1, List<string> graph2)
        {
            if (graph1.Count != graph2.Count) return false;
            foreach (var p in graph1)
            {
                if (graph1.Where(x => x == p).Count() != graph2.Where(x => x == p).Count()) return false;
            }
            return true;
        }

        private string[,] GetComparisonMatrix(string[,] values)
        {
            Dictionary<string, List<string>> graphCodes = new Dictionary<string, List<string>>();
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(0); j++)
                {
                    if (i == j) continue;
                    var graph = GetGraphForTwoRows(values, i, j);
                    string graphCode = GetCodeForGraph(graph);
                    string index = "(" + i + "," + j + ")";
                    if (graphCodes.Any(x => x.Key == graphCode))
                    {
                        graphCodes[graphCode].Add(index);
                    }
                    else
                    {
                        var list = new List<string>();
                        list.Add(index);
                        graphCodes.Add(graphCode, list);
                    }
                }
            }
            var matrix = new string[values.GetLength(0), values.GetLength(0)];
            int num = 1;
            foreach (var pair in graphCodes)
            {
                var code = pair.Key;
                var indices = pair.Value;
                foreach (var index in indices)
                {
                    var parts = index.Replace("(", "").Replace(")", "").Split(',');
                    int i = parts[0].AsInt();
                    int j = parts[1].AsInt();
                    matrix[i, j] = "" + num;
                }
                num++;
            }
            return matrix;
        }

        private string GetCodeForGraph(List<string> graph)
        {
            graph.Sort();
            string code = "";
            foreach (var subGraph in graph)
            {
                code += "#" + subGraph;
            }
            return code;
        }

        private void RefinePartitions()
        {

        }
    }
}