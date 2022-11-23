namespace Api.Models.Subscribe
{
    public class SubscribeModel
    {
        public Guid SubscribeOwnerId { get; set; }
        public Guid SubscriberId { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
