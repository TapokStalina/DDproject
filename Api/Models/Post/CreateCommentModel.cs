using DAL.Entities;

namespace Api.Models.Post
{
    public class CreateCommentModel
    {
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string CommentText { get; set; }
        public DateTimeOffset Created { get; set; }
        public List<CommentContent> Contents { get; set; }

    }
    public class CreateCommentRequest
    {
        public string CommentText { get; set; }
        public Guid PostId { get; set; }

    }
}
