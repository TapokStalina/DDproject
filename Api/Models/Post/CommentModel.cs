using Api.Models.UserModel;
using DAL.Entities;

namespace Api.Models.Post
{
    public class CommentModel
    {
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }
        public string CommentText { get; set; }
        public DateTimeOffset Created { get; set; }

        public Guid AuthorId { get; set; }


    }
}
