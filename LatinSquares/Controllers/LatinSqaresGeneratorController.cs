using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LatinSquares.Models;
using System.Net.Http.Headers;
using System.IO;

namespace LatinSquares.Controllers
{
    public class LatinSqaresGeneratorController : ApiController
    {
        [HttpGet]
        [Route("api/GetRectangle")]
        public HttpResponseMessage GetRectangle([FromUri] int rows = 5, [FromUri] int cols = 5, [FromUri] int symbols = 5, [FromUri] int count = 25)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent("the rectangle didnt make it this time:(");

            Rectangle sq = null;
            sq = Utils.GetRectangle(rows, cols, symbols, count);
            Utils.SaveRectanlgeToDb(sq, rows, cols, symbols, count, DbModels.DbRectangle.TYPE_EMPTY);
            response.Content = new StringContent(sq.ToString());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [HttpGet]
        [Route("api/GetRectangleFile")]
        public HttpResponseMessage GetRectangleFile([FromUri] int rows = 5, [FromUri] int cols = 5, [FromUri] int symbols = 5, [FromUri] int count = 25, [FromUri] int squares = 10)
        {
            var response = new HttpResponseMessage();
            List<Rectangle> squaresList = new List<Rectangle>();
            while (squares-- > 0)
            {
                Rectangle sq = Utils.GetRectangle(rows, cols, symbols, count);
                Utils.SaveRectanlgeToDb(sq, rows, cols, symbols, count, DbModels.DbRectangle.TYPE_EMPTY);
                if (squaresList.Contains(sq)) squares++;
                else squaresList.Add(sq);
            }
            string squaresString = "";
            foreach (var sq in squaresList)
            {
                squaresString += sq.GetPlainTextString();
            }
            response.Content = new StringContent(squaresString);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [HttpGet]
        [Route("api/GetFullRectangle")]
        public HttpResponseMessage GetFullRectangle([FromUri] int rows = 5, [FromUri] int cols = 5, [FromUri] int count = 25, [FromUri] int symbols = 5)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent("the rectangle didnt make it this time:(");

            Rectangle sq = null;
            sq = Utils.GetFullRectangle(rows, cols, symbols, count);
            Utils.SaveRectanlgeToDb(sq, rows, cols, symbols, count, DbModels.DbRectangle.TYPE_FULL);
            response.Content = new StringContent(sq.ToString());

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


        [HttpGet]
        [Route("api/GetFullRectanglesFile")]
        public HttpResponseMessage GetFullRectanglesFile([FromUri] int rows = 5, [FromUri] int cols = 5, [FromUri] int symbols = 5, [FromUri] int count = 25, [FromUri] int squares = 10)
        {
            var response = new HttpResponseMessage();
            List<Rectangle> squaresList = new List<Rectangle>();
            while (squares-- > 0)
            {
                Rectangle sq = Utils.GetFullRectangle(rows, cols, symbols, count);
                Utils.SaveRectanlgeToDb(sq, rows, cols, symbols, count, DbModels.DbRectangle.TYPE_FULL);
                if (squaresList.Contains(sq)) squares++;
                else squaresList.Add(sq);
            }
            string squaresString = "";
            foreach (var sq in squaresList)
            {
                squaresString += sq.GetPlainTextString();
            }
            response.Content = new StringContent(squaresString);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


        [HttpGet]
        [Route("api/GetInvestigationObjectForRectangle")]
        public HttpResponseMessage GetInvestigationObjectForRectangle([FromUri] string squareString)
        {
            var response = new HttpResponseMessage();
            string res = "";
            try
            {
                InvestigationObject obj = new InvestigationObject(squareString);
                res = obj.AsString("all");
            }
            catch (Exception e)
            {
                res = "error: " + e.Message;
            }

            response.Content = new StringContent(res);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


        [HttpGet]
        [Route("api/GetNonTrivialRectangles")]
        public HttpResponseMessage GetNonTrivialRectangles([FromUri] string howMany)
        {
            var response = new HttpResponseMessage();
            string result = "";
            string res = "";
            try
            {
                string fileName = new Random().Next().ToString() + ".txt";
                res = Utils.CommandLine.Execute(@"c:\plr_non_trivial.exe", howMany + " " + fileName);
                FileInfo file = new FileInfo(fileName);
                while (Utils.IsFileLocked(file));
                res = File.ReadAllText(fileName);
                var rectStrings = res.Split(new string[] { "\n\n"}, StringSplitOptions.RemoveEmptyEntries);
                List<char> chars = new List<char>();
                foreach (var rect in rectStrings)
                {
                    string r = rect;
                    foreach (var c in r.ToCharArray())
                    {
                        if (Char.IsLetterOrDigit(c) && !chars.Contains(c)) chars.Add(c);
                    }
                    for (int i = 0; i < r.Length; i++)
                    {
                        if (chars.Contains(r[i]))
                            r = r.Substring(0, i) + Utils.SYMBOLS[chars.IndexOf(r[i])] + r.Substring(i+1);
                    }
                    Utils.SaveRectanlgeToDb(r, DbModels.DbRectangle.TYPE_NON_TRIVIAL);
                    result += (r + "\n\n");
                }
            }
            catch (Exception e)
            {
                res = "error: " + e.ToString();
                File.WriteAllText(@"c:\log.txt", res);
            }

            response.Content = new StringContent(result);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


        [HttpGet]
        [Route("api/GetAtp")]
        public HttpResponseMessage GetAtp([FromUri] string squareString)
        {
            var response = new HttpResponseMessage();
            string res = "";
            try
            {
                InvestigationObject obj = new InvestigationObject(squareString);
                Random r = new Random();
                string inputFileName = @"c:\" + r.Next().ToString() + "_in.txt";
                File.WriteAllText(inputFileName, obj.AsString("cleanSquare"));

                string outputFileName = @"c:\" + r.Next().ToString() + "_out.txt";
                res = Utils.CommandLine.Execute(@"c:\atp.exe", inputFileName + " " + outputFileName);
                FileInfo file = new FileInfo(outputFileName);
                while (Utils.IsFileLocked(file));
                res = File.ReadAllText(outputFileName);
                File.Delete(inputFileName);
                File.Delete(outputFileName);
            }
            catch (Exception e)
            {
                res = "error: " + e.ToString();
                File.WriteAllText(@"c:\log.txt", res);
            }

            response.Content = new StringContent(res);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


        [HttpGet]
        [Route("api/CMDTest")]
        public HttpResponseMessage CMDTest()
        {
            var response = new HttpResponseMessage();
            string res = "";
            try
            {
                res = Utils.CommandLine.Execute(@"cmd", "ls");
            }
            catch (Exception e)
            {
                res = "error: " + e.ToString();
                File.WriteAllText(@"c:\log.txt", res);
            }

            response.Content = new StringContent(res);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


        [HttpGet]
        [Route("api/GetRectangleFromDb")]
        public HttpResponseMessage GetRectangleFromDb([FromUri] int rows = 5, [FromUri] int cols = 5, [FromUri] int count = 25, [FromUri] int symbols = 5, [FromUri] string type = "empty", [FromUri] int number = 1)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent("the rectangle didnt make it this time:(");

            response.Content = new StringContent(Utils.GetRectangleFromDB(rows, cols, symbols, count, type, number));

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


        [HttpPost]
        [Route("api/checkRectangles/{flag}")]
        public HttpResponseMessage CheckRectangles(bool flag, [FromBody] string input)
        {
            var response = new HttpResponseMessage();
            if (flag)
            {
                input = Utils.Format(input);
            }
            int count = 0, countG = 0;
            string[] squares = input.Trim().Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in squares)
            {
                InvestigationObject obj = new InvestigationObject(s.Replace("\n", ""));
                if (obj.IsPartitionTrivial()) count++;
                if (obj.IsPartitionTrivial(true)) countG++;
            }
            string res = "number of squares inserted: " + squares.Count();
            res += "\nnumber of squares found trivial by first partition (D): " + count;
            res += "\npercent: " + (((double)count)/squares.Count());
            res += "\nnumber of squares found trivial by first partition (G): " + countG;
            res += "\npercent: " + (((double)countG) / squares.Count());
            res += "\nHope that helped!!";
            response.Content = new StringContent(res);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
       
    }
}
