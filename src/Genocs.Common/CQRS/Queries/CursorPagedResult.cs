namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// Cursor-based paged result contract.
/// </summary>
/// <typeparam name="T">Result item type.</typeparam>
public class CursorPagedResult<T>
{
    /// <summary>
    /// Returned items for the current cursor window.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Opaque token to request the next window.
    /// Null when no further window is available.
    /// </summary>
    public string NextCursor { get; }

    /// <summary>
    /// Opaque token to request the previous window when supported.
    /// </summary>
    public string PreviousCursor { get; }

    /// <summary>
    /// Number of records returned in the current window.
    /// </summary>
    public int ReturnedCount { get; }

    /// <summary>
    /// Indicates whether a next cursor is available.
    /// </summary>
    public bool HasMore => !string.IsNullOrWhiteSpace(NextCursor);

    /// <summary>
    /// Default constructor for empty results.
    /// </summary>
    protected CursorPagedResult()
    {
        Items = [];
        ReturnedCount = 0;
    }

    /// <summary>
    /// Standard constructor.
    /// </summary>
    /// <param name="items">Current window items.</param>
    /// <param name="nextCursor">Next cursor token.</param>
    /// <param name="previousCursor">Previous cursor token.</param>
    protected CursorPagedResult(IEnumerable<T> items, string nextCursor, string previousCursor)
    {
        T[] materialized = items?.ToArray() ?? [];
        Items = materialized;
        ReturnedCount = materialized.Length;
        NextCursor = nextCursor;
        PreviousCursor = previousCursor;
    }

    /// <summary>
    /// Creates a cursor-based result.
    /// </summary>
    public static CursorPagedResult<T> Create(IEnumerable<T> items, string nextCursor = null, string previousCursor = null)
        => new(items, nextCursor, previousCursor);

    /// <summary>
    /// Empty cursor-based result.
    /// </summary>
    public static CursorPagedResult<T> Empty => new();
}
