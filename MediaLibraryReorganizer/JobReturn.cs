// <copyright file="JobReturn.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    public class JobReturn
    {
        public bool Success { get; set; }

        public AggregateException HandledError { get; set; }

        public static implicit operator bool(JobReturn resultBool) => resultBool.Success;
    }
}
