namespace Api.Models.Post
{
    public class GetCommentsModel
    {
        public string CommentText { get; set; }
        public DateTimeOffset Created { get; set; }
        public Guid UserId { get; set; }
    }
}
