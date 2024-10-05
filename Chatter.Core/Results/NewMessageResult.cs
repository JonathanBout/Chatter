using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatter.Core.Results
{
    public class NewMessageResult
    {
        public required UserResult Sender { get; set; }
        public required DateTimeOffset SentTime { get; set; }
        public required string Content { get; set; }
        public required bool IsEncrypted { get; set; }
    }
}
