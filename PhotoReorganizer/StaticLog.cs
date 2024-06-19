// <copyright file="StaticLog.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner.Lib
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