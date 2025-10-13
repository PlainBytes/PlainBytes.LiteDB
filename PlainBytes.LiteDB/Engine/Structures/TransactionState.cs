using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using static PlainBytes.LiteDB.Constants;

namespace PlainBytes.LiteDB.Engine
{
    internal enum TransactionState
    {
        Active,
        Committed,
        Aborted,
        Disposed
    }
}