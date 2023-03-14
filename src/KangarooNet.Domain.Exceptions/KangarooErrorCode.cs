// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum KangarooNetErrorCode
    {
        Others = 0,
        SecurityValidation = 1,
        InvalidPassword = 2,
    }
}
