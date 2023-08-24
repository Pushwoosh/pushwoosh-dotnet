using System;
using System.Runtime.Serialization;
using Com.Pushwoosh.iOS;
using PushwooshSDK.DotNet.Core;

namespace Pushwoosh.iOS
{
    public class InboxManager : IInboxManager
    {
        public InboxManager()
        {
        }

        public void LoadMessages(Action<List<InboxMessage>, string> completion)
        {
            PWInbox.LoadMessagesWithCompletion((messages, error) =>
            {
                List<InboxMessage> inboxMessages = new List<InboxMessage>();
                foreach (var msg in messages)
                {

                    DateTime dateTime = ((DateTime)msg.SendDate).ToLocalTime();
                    InboxMessage message = new InboxMessage(msg.Message, msg.Title, (InboxMessageType)msg.Type, msg.ActionParams.ToString(), msg.AttachmentUrl, msg.ImageUrl, msg.Code, ((DateTime)msg.SendDate).ToLocalTime(), msg.IsActionPerformed, msg.IsRead);
                    inboxMessages.Add(message);
                }
                completion(inboxMessages, error != null ? error.LocalizedDescription : null);
            });
        }

        public void MessagesCount(Action<int, string> completion)
        {
            PWInbox.MessagesCountWithCompletion((count, error) =>
            {
                completion((int)count, error != null ? error.LocalizedDescription : null);
            });
        }

        public void MessagesWithNoActionPerformedCount(Action<int, string> completion)
        {
            PWInbox.MessagesWithNoActionPerformedCountWithCompletion((count, error) =>
            {
                completion((int)count, error != null ? error.LocalizedDescription : null);
            });
        }


        public void UnreadMessagesCount(Action<int, string> completion)
        {
            PWInbox.UnreadMessagesCountWithCompletion((count, error) =>
            {
                completion((int)count, error != null ? error.LocalizedDescription : null);
            });
        }

        public void PerformAction(string code)
        {
            PWInbox.PerformActionForMessageWithCode(code);
        }

        public void ReadMessage(string code)
        {
            string[] arr = { code };
            PWInbox.ReadMessagesWithCodes(arr);
        }

        public void ReadMessages(List<string> codes)
        {
            string[] arr = codes.ToArray();
            PWInbox.ReadMessagesWithCodes(arr);
        }

        public void DeleteMessage(string code)
        {
            string[] arr = { code };
            PWInbox.DeleteMessagesWithCodes(arr);
        }

        public void DeleteMessages(List<string> codes)
        {
            string[] arr = codes.ToArray();
            PWInbox.DeleteMessagesWithCodes(arr);
        }
    }
}