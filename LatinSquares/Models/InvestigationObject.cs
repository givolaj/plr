using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LatinSquares.Models
{
    public class InvestigationObject
    {
        private Rectangle square;
        private string[,] tripletValues;

        public InvestigationObject(string squareString)
        {
            square = ValidateAndReturnSquareString(squareString);
            if (square == null)
            {
                throw new Exception("this square string doesn't represent a valid latin square");
            }
            tripletValues = new string[square.GetRowsNumber(), square.GetColumnsNumber()];
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
                    indexedTriplets = GenerateIndexedTripletsString()
            };
                return JsonConvert.SerializeObject(obj);
            }
            else
            {
                return "didn't recognize the type";
            }
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
    }
}