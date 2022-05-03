using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Asynchronous enumeration of documents.
    /// </summary>
    public static class AsyncEnumerable
    {
        /// <summary>
        /// Provides asynchronous iteration over all document returned by <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="source">The source cursor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Async-enumerable that iterates over all document returned by cursor.</returns>
        public static async IAsyncEnumerable<TDocument> ToAsyncEnumerable<TDocument>(
            this IAsyncCursor<TDocument> source,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await source.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                foreach (var document in source.Current)
                {
                    yield return document;
                }
            }
        }
    }
}
