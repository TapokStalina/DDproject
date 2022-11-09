using Api.Models.Attach;
using DAL.Entities;

namespace Api.Models.Post
{
    public class PostModel
    {
        public List<MetadataModel>? PostAttaches { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset Created { get; set; }

    }
}
