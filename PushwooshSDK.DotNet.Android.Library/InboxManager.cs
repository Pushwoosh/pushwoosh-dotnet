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
                                InboxMessage inboxMessage = new InboxMessage(msg.Message, msg.Title, (InboxMessageType)msg.Type.Code, msg.ActionParams, msg.BannerUrl, msg.ImageUrl, msg.Code, msg.ISO8601SendDate, msg.IsActionPerformed, msg.IsRead);
                                messagesList.Add(inboxMessage);
                            }
                        }
                    }
                    catch (System.InvalidCastException ex)
                    {
                        //obj.Data.JavaCast<JavaList<IInboxMessage>> fails to cast empty lists to JavaList, and we cannot check native type of obj.Data so just catch exception and do nothing.
                    }
                    completion(messagesList, obj.Exception != null ? obj.Exception.ToString() : null);
                }
            });
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

