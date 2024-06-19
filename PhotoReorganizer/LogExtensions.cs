// <copyright file="LogExtensions.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace Serilog
{
    public static class LogExtensions
    {
        public static void Enter(this ILogger log, string methodName)
        {
            log.Debug("Entering method {0}", methodName);
        }

        public static void Exit(this ILogger log, string methodName)
        {
            log.Debug("Exiting method {0}", methodName);
        }
    }
}