using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PushwooshSDK.DotNet.Core;
using Pushwoosh.Function;
using Result = Pushwoosh.Function.Result;
using NativeInAppManager = Pushwoosh.InApp.InAppManager;

namespace Pushwoosh.Android
{
    public class InAppManager : IInAppManager
    {
        // Android SDK 6.10.0 split the old PushwooshInApp: events stayed in the
        // in-app manager, user identification moved to the main Pushwoosh class.
        NativeInAppManager nativeInApp => NativeInAppManager.Instance;
        PushNotificationsManager nativeManager => PushNotificationsManager.Instance;

        public Task<RequestResult> MergeUserId(string oldUserId, string newUserId, bool doMerge)
        {
            var taskSource = new TaskCompletionSource<RequestResult>();
            nativeManager.MergeUserId(oldUserId, newUserId, doMerge, new PushManager.Callback() { ResultCallback = (Result result) => {
                    taskSource.SetResult(new RequestResult() { Error = PushManager.ErrorFromJavaException(result.Exception) });
                } });
            return taskSource.Task;
        }

        public Task<RequestResult> PostEventAsync(string anEvent, Dictionary<string, string> attributes)
        {
            var taskSource = new TaskCompletionSource<RequestResult>();
            var tagsBundleBuilder = new Tags.TagsBundle.Builder();
            if (attributes != null)
            {
                foreach (var kv in attributes)
                {
                    tagsBundleBuilder.PutString(kv.Key, kv.Value);
                }
            }
            nativeInApp.PostEvent(anEvent, tagsBundleBuilder.Build(), new PushManager.Callback() { ResultCallback = (Result result) => {
                    taskSource.SetResult(new RequestResult() { Error = PushManager.ErrorFromJavaException(result.Exception) });
                } });

            return taskSource.Task;
        }

        public void SetUserId(string userId)
        {
            nativeManager.UserId = userId;
        }
    }
}
