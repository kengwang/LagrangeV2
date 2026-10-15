using System;
using Lagrange.Core.Exceptions;
using Serilog;

namespace Lagrange.Milky.Api;

internal static class ForwardErrors
{
    internal static LagrangeException? Find(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is ForwardPreparationException or OperationException) return (LagrangeException)current;
        return null;
    }

    internal static string Describe(Exception exception)
    {
        var failure = Find(exception)!;
        if (failure is ForwardPreparationException forward)
        {
            Log.Warning(exception, "Forward preparation failed at {NodePath} ({SegmentType})", forward.NodePath, forward.SegmentType);
            return forward.Message;
        }
        Log.Warning(exception, "Message operation failed");
        return ((OperationException)failure).ErrMsg ?? "Message operation failed.";
    }

    internal static long Code(Exception exception) => Find(exception) is OperationException operation ? operation.Result : -1;
}
