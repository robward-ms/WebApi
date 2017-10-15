// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace WebStack.QA.Test.OData
{
    /// <summary>
    /// This is an XUnit Fixture class designed to manage the services for a given test class.
    /// Fixtures must reside in the same assembly as the tests that use them.
    /// </summary>
    class NuwaFixture : IDisposable
    {
        private bool disposedValue = false; // To detect redundant calls

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
    }
}
