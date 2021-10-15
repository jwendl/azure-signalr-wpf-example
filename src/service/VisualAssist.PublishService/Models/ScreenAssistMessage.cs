using System;

namespace VisualAssist.PublishService.Models
{
    public class ScreenAssistMessage
    {
        public string SentBy { get; set; }

        public DateTime MessageDateTime { get; set; }

        public string Message { get; set; }
    }
}
