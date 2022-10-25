using System;
using SaveChangesMaybe.Core;

namespace SaveChangesMaybe.Tests
{
    public class TestBase : IDisposable
    {
        public void Dispose()
        {
            SaveChangesMaybeHelper.ClearBufferFromMemory();
        }
    }
}
