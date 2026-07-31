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
                    // PWInbox.h declares the message protocol without NS_ASSUME_NONNULL,
                    // so every object property is nullable — and the native side really
                    // does return nil: PWInboxMessageInternal parses actionParams from
                    // JSON and leaves it nil for an empty or malformed value, and guards
                    // its own reads with `if (_actionParams != nil)`. Dereferencing
                    // either of these threw NullReferenceException out of LoadMessages
                    // for the whole batch as soon as one message lacked the field.
                    DateTime? sendDate = msg.SendDate is null
                        ? null
                        : ((DateTime)msg.SendDate).ToLocalTime();

                    InboxMessage message = new InboxMessage(msg.Message, msg.Title, (InboxMessageType)msg.Type, msg.ActionParams?.ToString(), msg.AttachmentUrl, msg.ImageUrl, msg.Code, sendDate, msg.IsActionPerformed, msg.IsRead);
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