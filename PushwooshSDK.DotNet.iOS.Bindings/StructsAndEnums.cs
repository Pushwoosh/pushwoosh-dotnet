using System;
using ObjCRuntime;

namespace Com.Pushwoosh.iOS
{
    [Native]
    public enum PWInboxMessageType : long
    {
        Plain = 0,
        Richmedia = 1,
        Url = 2,
        Deeplink = 3
    }
}


