// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IHasGuidKey
    {
        /// <summary>
        /// Get the entity key.
        /// </summary>
        /// <returns>The currenty key.</returns>
        public Guid GetKey();

        /// <summary>
        /// Set the entity key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void SetKey(Guid key);
    }
}
