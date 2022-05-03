using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Driver.Linq.AsyncEnumerable.Tests;

public class AsyncEnumerableTests
{
    [Fact]
    public async Task Should_enumerate_an_empty_result()
    {
        var cancellationToken = new CancellationToken();

        var cursorToEmptyResult = new Mock<IAsyncCursor<BsonDocument>>();

        cursorToEmptyResult.Setup(mock => mock.MoveNext(cancellationToken)).Returns(false);
        cursorToEmptyResult.Setup(mock => mock.MoveNextAsync(cancellationToken)).ReturnsAsync(false);
        cursorToEmptyResult.Setup(mock => mock.Current).Throws<InvalidOperationException>();

        var enumerated = await cursorToEmptyResult.Object.ToAsyncEnumerable(cancellationToken).ToListAsync();

        IMongoCollection<BsonDocument> mongo;

        enumerated.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_enumerate_a_result_containing_single_document()
    {
        var cancellationToken = new CancellationToken();

        var document = new BsonDocument("ExampleElementName", new BsonString("ExampleElementValue"));

        var result = new[]
        {
            new Batch<BsonDocument>(new[] { document })
        };

        var cursor = SetupAsyncCursor(result, cancellationToken);

        var enumerated = await cursor.ToAsyncEnumerable(cancellationToken).ToListAsync();

        enumerated.Should().ContainSingle().Which.Should().BeEquivalentTo(document);
    }

    [Fact]
    public async Task Should_enumerate_a_result_containing_one_batch_with_multiple_documents()
    {
        var cancellationToken = new CancellationToken();

        var firstDocument = new BsonDocument("FirstExampleElementName", new BsonString("FirstExampleElementValue"));
        var secondDocument = new BsonDocument("SecondExampleElementName", new BsonString("SecondExampleElementValue"));
        var thirdDocument = new BsonDocument("ThirdExampleElementName", new BsonString("ThirdExampleElementValue"));

        var result = new[]
        {
            new Batch<BsonDocument>(new[] { firstDocument, secondDocument, thirdDocument })
        };

        var cursor = SetupAsyncCursor(result, cancellationToken);

        var enumerated = await cursor.ToAsyncEnumerable(cancellationToken).ToListAsync();

        enumerated.Should().HaveCount(3);
        enumerated[0].Should().BeEquivalentTo(firstDocument);
        enumerated[1].Should().BeEquivalentTo(secondDocument);
        enumerated[2].Should().BeEquivalentTo(thirdDocument);
    }

    [Fact]
    public async Task Should_enumerate_a_result_containing_multiple_batches_each_containing_one_document()
    {
        var cancellationToken = new CancellationToken();

        var firstDocument = new BsonDocument("FirstExampleElementName", new BsonString("FirstExampleElementValue"));
        var secondDocument = new BsonDocument("SecondExampleElementName", new BsonString("SecondExampleElementValue"));
        var thirdDocument = new BsonDocument("ThirdExampleElementName", new BsonString("ThirdExampleElementValue"));

        var result = new[]
        {
            new Batch<BsonDocument>(new[] { firstDocument }),
            new Batch<BsonDocument>(new[] { secondDocument }),
            new Batch<BsonDocument>(new[] { thirdDocument }),
        };

        var cursor = SetupAsyncCursor(result, cancellationToken);

        var enumerated = await cursor.ToAsyncEnumerable(cancellationToken).ToListAsync();

        enumerated.Should().HaveCount(3);
        enumerated[0].Should().BeEquivalentTo(firstDocument);
        enumerated[1].Should().BeEquivalentTo(secondDocument);
        enumerated[2].Should().BeEquivalentTo(thirdDocument);
    }

    [Fact]
    public async Task Should_enumerate_a_result_containing_multiple_batches_with_multiple_documents()
    {
        var cancellationToken = new CancellationToken();
        var firstDocument = new BsonDocument("FirstExampleElementName", new BsonString("FirstExampleElementValue"));
        var secondDocument = new BsonDocument("SecondExampleElementName", new BsonString("SecondExampleElementValue"));
        var thirdDocument = new BsonDocument("ThirdExampleElementName", new BsonString("ThirdExampleElementValue"));
        var fourthDocument = new BsonDocument("FourthExampleElementName", new BsonString("FourthExampleElementValue"));
        var fifthDocument = new BsonDocument("FifthExampleElementName", new BsonString("FifthExampleElementValue"));
        var sixthDocument = new BsonDocument("SixthExampleElementName", new BsonString("SixthExampleElementValue"));

        var result = new[]
        {
            new Batch<BsonDocument>(new[] { firstDocument, secondDocument }),
            new Batch<BsonDocument>(new[] { thirdDocument, fourthDocument }),
            new Batch<BsonDocument>(new[] { fifthDocument, sixthDocument }),
        };

        var cursor = SetupAsyncCursor(result, cancellationToken);

        var enumerated = await cursor.ToAsyncEnumerable(cancellationToken).ToListAsync();

        enumerated.Should().HaveCount(6);
        enumerated[0].Should().BeEquivalentTo(firstDocument);
        enumerated[1].Should().BeEquivalentTo(secondDocument);
        enumerated[2].Should().BeEquivalentTo(thirdDocument);
        enumerated[3].Should().BeEquivalentTo(fourthDocument);
        enumerated[4].Should().BeEquivalentTo(fifthDocument);
        enumerated[5].Should().BeEquivalentTo(sixthDocument);
    }

    private struct Batch<TDocument>
    {
        public Batch(TDocument[] documents)
        {
            Documents = documents;
        }

        public TDocument[] Documents { get; }
    }

    private static IAsyncCursor<BsonDocument> SetupAsyncCursor(Batch<BsonDocument>[] resultInBatches, CancellationToken cancellationToken)
    {
        var cursor = new Mock<IAsyncCursor<BsonDocument>>();
        var currentBatch = 0;

        cursor.Setup(mock => mock.MoveNext(cancellationToken)).Returns(() => currentBatch++ < resultInBatches.Length);
        cursor.Setup(mock => mock.MoveNextAsync(cancellationToken)).ReturnsAsync(() => currentBatch++ < resultInBatches.Length);
        cursor.Setup(mock => mock.Current).Returns(() => resultInBatches[currentBatch - 1].Documents);

        return cursor.Object;
    }
}
