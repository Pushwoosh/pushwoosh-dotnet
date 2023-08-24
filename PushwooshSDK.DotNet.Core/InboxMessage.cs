using System;
namespace PushwooshSDK.DotNet.Core
{
    public class InboxMessage
    {
        public InboxMessage(string? message, string? title, InboxMessageType type, string? actionParams, string? attachmentUrl, string? imageUrl, string? code, DateTime? sendDate, bool isActionPerformed, bool isRead)
        {
            Message = message;
            Title = title;
            Type = type;
            ActionParams = actionParams;
            AttachmentUrl = attachmentUrl;
            ImageUrl = imageUrl;
            Code = code;
            SendDate = sendDate;
            IsActionPerformed = isActionPerformed;
            IsRead = isRead;
        }

        public string? Message { get; set; }
        public string? Title { get; set; }
        public InboxMessageType Type {get; set;} 
        public string? ActionParams { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Code { get; set; }
        public DateTime? SendDate { get; set; }
        public bool IsActionPerformed { get; set; }
        public bool IsRead { get; set; }
	}

    public enum InboxMessageType : long
    {
        Plain = 0,
        Richmedia = 1,
        Url = 2,
        Deeplink = 3
    }
}
