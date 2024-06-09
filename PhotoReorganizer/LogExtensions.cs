// <copyright file="LogExtensions.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

using log4net;
public static class LogExtensions
{
    public static void Enter(this ILog log, string methodName)
    {
        log.DebugFormat("Entering method {0}", methodName);
    }

    public static void Exit(this ILog log, string methodName)
    {
        log.DebugFormat("Exiting method {0}", methodName);
    }
}