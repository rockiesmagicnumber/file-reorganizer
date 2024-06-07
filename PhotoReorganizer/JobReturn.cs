// <copyright file="JobReturn.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner.Lib
{
    public class JobReturn
    {
        public bool Success { get; set; }

        public Exception? Error { get; set; }

        public static implicit operator bool(JobReturn resultBool) => resultBool.Success;
    }
}
