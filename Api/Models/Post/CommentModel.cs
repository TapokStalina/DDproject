namespace Api.Models.Post
{
    public class CommentModel
    {
        public string CommentText { get; set; }
        public DateTimeOffset Created { get; set; }
        public Guid PostId { get; set; }
    }
}
