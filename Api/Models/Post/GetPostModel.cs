using DAL.Entities;

namespace Api.Models.Post
{
    public class GetPostModel
    {
        public virtual Guid UserId { get; set; }
        public string[] AttachPaths { get; set; }
        public string? Description { get; set; }

    }
}
