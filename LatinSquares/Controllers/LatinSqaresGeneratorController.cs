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
            catch (Exception)
            {
                res = "square not valid";
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
            string res = "";
            try
            {
                string fileName = new Random().Next().ToString() + ".txt";
                res = Utils.CommandLine.Execute(@"c:\plr_non_trivial.exe", howMany + " " + fileName);
                FileInfo file = new FileInfo(fileName);
                while (Utils.IsFileLocked(file));
                res = File.ReadAllText(fileName);
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
    }
}
