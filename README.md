# MongoDB.Driver.Linq.AsyncEnumerable

Unofficial IAsyncEnumerable support for MongoDB C# Driver.

Provides `ToAsyncEnumerable()` extensions for MongoDB `IAsyncCursor` and `IAsyncCursorSource`.

Asynchronous enumeration is implemeted so that the iterator fetches documents from database to memory one batch at time.

The size of the batch can be controller by `BatchSize` property of e.g. `AggregateOptions` or `FindOptions`.

## Usage

- Include types in `MongoDB.Driver.Linq` namespace by `using MongoDB.Driver.Linq`
- Call the `ToAsyncEnumerable()` extension for either `IAsyncCursor` or `IAsyncCursorSource`
- Asynchronously iterate through elements returned by `ToAsyncEnumerable()`

### Enumerating results of `Find()`

```
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

var client = new MongoClient();
var database = client.GetDatabase("ExampleDatabase");
var collection = database.GetCollection<BsonDocument>("ExampleCollection");

await foreach (var document in collection.Find(_ => true).ToAsyncEnumerable())
{
    Console.WriteLine(document.ToString());
}
```

### Enumerating results of `Aggregate()` by fetching documents in batches of 1000 documents

```
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

var client = new MongoClient();
var database = client.GetDatabase("ExampleDatabase");
var collection = database.GetCollection<BsonDocument>("ExampleCollection");
var options = new AggregateOptions { BatchSize = 1000 };

await foreach (var document in collection.Aggregate(options).ToAsyncEnumerable())
{
    Console.WriteLine(document.ToString());
}
```

