using System;
namespace PushwooshSDK.DotNet.Core
{
    public class RequestResult 
    {
        public PushwooshError? Error { get; set; }
    }
    public class RequestResult<TResult> : RequestResult
    {
        public TResult? Result { get; set; }
    }
}
