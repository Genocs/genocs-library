namespace Genocs.Core.Collections.Extensions;

/// <summary>
/// Extension methods for <see cref="IList{T}"/>.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Sort a list by a topological sorting, which consider their  dependencies.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="source">A list of objects to sort.</param>
    /// <param name="getDependencies">Function to resolve the dependencies.</param>
    /// <returns>A list of objects sorted by their dependencies.</returns>
    public static List<T> SortByDependencies<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getDependencies)
        where T : notnull
    {
        /* See: http://en.wikipedia.org/wiki/Topological_sorting */

        var sorted = new List<T>();
        var visited = new Dictionary<T, bool>();

        foreach (var item in source)
        {
            SortByDependenciesVisit(item, getDependencies, sorted, visited);
        }

        return sorted;
    }

    /// <summary>
    /// Sort a list by a topological sorting, which consider their  dependencies.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="item">Item to resolve.</param>
    /// <param name="getDependencies">Function to resolve the dependencies.</param>
    /// <param name="sorted">List with the sortet items.</param>
    /// <param name="visited">Dictionary with the visited items.</param>
    private static void SortByDependenciesVisit<T>(T item, Func<T, IEnumerable<T>> getDependencies, List<T> sorted, Dictionary<T, bool> visited)
        where T : notnull
    {
        bool alreadyVisited = visited.TryGetValue(item, out bool inProcess);

        if (alreadyVisited)
        {
            if (inProcess)
            {
                throw new ArgumentException("Cyclic dependency found! Item: " + item);
            }
        }
        else
        {
            visited[item] = true;

            var dependencies = getDependencies(item);
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    SortByDependenciesVisit(dependency, getDependencies, sorted, visited);
                }
            }

            visited[item] = false;
            sorted.Add(item);
        }
    }
}
