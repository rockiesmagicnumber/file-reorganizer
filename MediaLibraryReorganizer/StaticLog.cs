// <copyright file="StaticLog.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using Serilog;

    public static class StaticLog
    {
        public static void Enter(string methodName)
        {
            Log.Information(string.Format("Enter - {0}", methodName));
        }

        public static void Exit(string methodName)
        {
            Log.Information(string.Format("Exit - {0}", methodName));
        }
    }
}