using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Subscribe
    {
        public Guid Id { get; set; }
        public Guid SubscribeOwnerId { get; set; }
        public Guid SubscriberId { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
