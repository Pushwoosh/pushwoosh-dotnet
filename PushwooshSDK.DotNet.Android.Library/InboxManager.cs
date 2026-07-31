using System;
using Pushwoosh.Inbox;
using Pushwoosh.Inbox.Data;
using Pushwoosh.Function;
using Result = Pushwoosh.Function.Result;
using InboxMessageType = PushwooshSDK.DotNet.Core.InboxMessageType;
using PushwooshSDK.DotNet.Core;
using Android.Runtime;

namespace Pushwoosh.Android
{
	public class InboxManager : IInboxManager
	{
		public InboxManager()
		{
		}

        public void LoadMessages(Action<List<InboxMessage>, string> completion)
        {
            PushwooshInbox.LoadMessages(new PushManager.Callback()
            {
                ResultCallback = (Result obj) =>
                {
                    List<InboxMessage> messagesList = new List<InboxMessage>();
                    try
                    {
                        var messagesFunc = obj.Data.JavaCast<JavaList<IInboxMessage>>;
                        var messages = messagesFunc.Invoke();

                        if (messages != null)
                        {
                            foreach (var msg in messages)
                            {
                                // Per-message, because everything below comes from Java without
                                // nullability guarantees, and the whole point is that one bad
                                // message must not cost the caller the entire batch — which is what
                                // the unguarded DateTime.Parse used to do, the same way iOS lost a
                                // batch to a missing action_params.
                                try
                                {
                                    messagesList.Add(MessageFromNative(msg));
                                }
                                catch (System.Exception)
                                {
                                    // Skip this one and keep the rest.
                                }
                            }
                        }
                    }
                    catch (System.InvalidCastException)
                    {
                        //obj.Data.JavaCast<JavaList<IInboxMessage>> fails to cast empty lists to JavaList, and we cannot check native type of obj.Data so just catch exception and do nothing.
                    }
                    completion(messagesList, obj.Exception != null ? obj.Exception.ToString() : null);
                }
            });
        }

        // com.pushwoosh.inbox.data.InboxMessage declares every getter as a plain Java type
        // (checked with javap on pushwoosh-inbox 6.10.0), so getISO8601SendDate() and getType()
        // can both come back null. DateTime.Parse on a null string throws
        // ArgumentNullException, and on a malformed one FormatException — neither of which the
        // outer InvalidCastException handler catches.
        static InboxMessage MessageFromNative(IInboxMessage msg)
        {
            DateTime? sendDate = DateTime.TryParse(msg.ISO8601SendDate, out var parsed)
                ? parsed
                : null;

            var type = msg.Type != null
                ? (InboxMessageType)msg.Type.Code
                : InboxMessageType.Plain;

            return new InboxMessage(msg.Message, msg.Title, type, msg.ActionParams,
                msg.BannerUrl, msg.ImageUrl, msg.Code, sendDate,
                msg.IsActionPerformed, msg.IsRead);
        }

        public void UnreadMessagesCount(Action<int, string> completion)
        {
            PushwooshInbox.UnreadMessagesCount(new PushManager.Callback()
            {
                ResultCallback = (Result obj) =>
                {
                    completion(obj.Data != null ? (int)obj.Data : 0, obj.Exception != null ? obj.Exception.ToString() : null);
                }
            });
        }

        public void MessagesCount(Action<int, string> completion)
        {
            PushwooshInbox.MessagesCount(new PushManager.Callback()
            {
                ResultCallback = (Result obj) =>
                {
                    completion(obj.Data != null ? (int)obj.Data : 0, obj.Exception != null ? obj.Exception.ToString() : null);
                }
            });
        }

        public void MessagesWithNoActionPerformedCount(Action<int, string> completion)
        {
            PushwooshInbox.MessagesWithNoActionPerformedCount(new PushManager.Callback()
            {
                ResultCallback = (Result obj) =>
                {
                    completion(obj.Data != null ? (int)obj.Data : 0, obj.Exception != null ? obj.Exception.ToString() : null);
                }
            });
        }

        public void PerformAction(string code)
        {
            PushwooshInbox.PerformAction(code);
        }

        public void ReadMessage(string code)
        {
            PushwooshInbox.ReadMessage(code);
        }

        public void ReadMessages(List<string> codes)
        {
            PushwooshInbox.ReadMessages(codes);
        }

        public void DeleteMessage(string code)
        {
            PushwooshInbox.DeleteMessage(code);
        }

        public void DeleteMessages(List<string> codes)
        {
            PushwooshInbox.DeleteMessages(codes);
        }
    }
}

