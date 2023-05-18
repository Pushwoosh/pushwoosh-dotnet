using System;
using System.Collections.Generic;

namespace PushwooshSDK.DotNet.Core
{
    public class PushNotification
    {
        public string? Payload { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? CustomData { get; set; }
    }
}
