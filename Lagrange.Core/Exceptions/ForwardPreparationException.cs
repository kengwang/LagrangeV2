namespace Lagrange.Core.Exceptions;

/// <summary>A failure while preparing or packing a forwarded node. The inner exception is for diagnostics.</summary>
public sealed class ForwardPreparationException(string path, string segmentType, Exception innerException)
    : LagrangeException($"Forward node {path} ({segmentType}) could not be prepared.", innerException)
{
    public string NodePath { get; } = path;
    public string SegmentType { get; } = segmentType;
}
