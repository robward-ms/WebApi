// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE
namespace Microsoft.Test.E2E.AspNet.OData.Common.Extensions
{
    public static class DirectoryInfoExtension
    {
        /// <summary>
        /// Delete and create the folder if it already exists
        /// </summary>
        public static void Recreate(this DirectoryInfo self)
        {
            self.Refresh();
            if (self.Exists)
            {
                self.Delete(true);
            }
            self.Create();
        }
    }
}
#endif
