using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Comment
    {
        public Guid AuthorId { get; set; }
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }
        public string CommentText { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual Post Post { get; set; } = null!;

    }

}
