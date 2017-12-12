using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace LatinSquares.Models
{
    public class DbModels
    {
        public class DbRectangle
        {
            public static readonly string TYPE_EMPTY = "empty";
            public static readonly string TYPE_FULL = "full";
            public static readonly string TYPE_NON_TRIVIAL = "non_trivial";

            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public Guid Id { get; set; }
            public int Rows { get; set; }
            public int Cols { get; set; }
            public int Symbols { get; set; }
            public int Count { get; set; }
            public string Type { get; set; }
            public string Content { get; set; }
        }

        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext() : base("DefaultConnection")
            {

            }

            public DbSet<DbRectangle> Rectangles { get; set; }

            public void SaveRectangleIfNotInDb(DbRectangle r)
            {
                if (Rectangles.Where(x => x.Rows == r.Rows)
                    .Where(x => x.Cols == r.Cols)
                    .Where(x => x.Symbols == r.Symbols)
                    .Where(x => x.Count == r.Count)
                    .Where(x => x.Type == r.Type)
                    .Any(x => x.Content == r.Content))
                    return;
                Rectangles.Add(r);
                SaveChanges();
            }
        }
    }
}