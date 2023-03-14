// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain.Exceptions
{
    using System;

    public class KangarooNetSecurityException : KangarooNetException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KangarooNetSecurityException"/> class.
        /// </summary>
        /// <param name="internalErrorCode">Error code used by KangarooNet to register some specific internal exceptions.</param>
        /// <param name="additionalInfo">Add any other information that you want to.</param>
        public KangarooNetSecurityException(KangarooNetErrorCode internalErrorCode = KangarooNetErrorCode.SecurityValidation, string additionalInfo = null)
            : base(internalErrorCode, null, additionalInfo)
        {
        }
    }
}
