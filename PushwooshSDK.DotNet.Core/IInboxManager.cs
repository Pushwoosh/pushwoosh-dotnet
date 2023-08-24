using System;
namespace PushwooshSDK.DotNet.Core
{
	public interface IInboxManager
	{
        public void LoadMessages(Action<List<InboxMessage>, string> completion);
        public void MessagesCount(Action<int, string> completion);
        public void MessagesWithNoActionPerformedCount(Action<int, string> completion);
        public void UnreadMessagesCount(Action<int, string> completion);

        public void DeleteMessage(string code);
        public void DeleteMessages(List<string> codes);
        public void ReadMessage(string code);
        public void ReadMessages(List<string> codes);
        public void PerformAction(string code);
    }
}

