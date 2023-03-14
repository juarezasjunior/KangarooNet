// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain.Exceptions
{
    using System;

    public class KangarooNetException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KangarooNetException"/> class.
        /// </summary>
        /// <param name="internalErrorCode">Error code used by KangarooNet to register some specific internal exceptions.</param>
        /// <param name="errorCode">Your error code that you want to register to classify your exceptions.</param>
        /// <param name="additionalInfo">Add any other information that you want to.</param>
        public KangarooNetException(KangarooNetErrorCode internalErrorCode = KangarooNetErrorCode.Others, int? errorCode = null, string additionalInfo = null)
        {
            this.InternalErrorCode = internalErrorCode;
            this.ErrorCode = errorCode;
            this.AdditionalInfo = additionalInfo;
        }

        public KangarooNetErrorCode InternalErrorCode { get; }

        public int? ErrorCode { get; }

        public string AdditionalInfo { get; }
    }
}
