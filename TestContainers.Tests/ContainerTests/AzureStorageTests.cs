using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq;
using FluentAssertions;

namespace TestContainers.Tests.ContainerTests
{
    public class AzureStorageFixture : IAsyncLifetime
    {
        public string ConnectionString => _container.ConnectionString;
        readonly AzureStorageContainer _container;

        public AzureStorageFixture() => _container = new GenericContainerBuilder<AzureStorageContainer>()
            .Begin()
            .WithImage("arafato/azurite:latest")
            .WithExposedPorts(10000, 10001, 10002)
            .Build();

        public Task InitializeAsync() => _container.Start();

        public Task DisposeAsync() => _container.Stop();
    }

    public class AzureStorageTests: IClassFixture<AzureStorageFixture>
    {
        readonly CloudStorageAccount _storageAccount;
        public AzureStorageTests(AzureStorageFixture fixture) => _storageAccount = CloudStorageAccount.Parse(fixture.ConnectionString);

        [Fact]
        public async Task SimpleBlobTest()
        {
            var blobClient = _storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("testcontainers");

            await blobContainer.CreateIfNotExistsAsync();
            var blob = blobContainer.GetBlockBlobReference("testcontainersblob.text");
            await blob.UploadTextAsync("testcontainers!");

            var text = await blob.DownloadTextAsync();

            Assert.Equal("testcontainers!", text);
        }

        [Fact]
        public async Task SimpleTableTest()
        {
            var tableClient = _storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("testcontainers");
            await table.CreateIfNotExistsAsync();

            var foo = new FooEntity("foo", "bar");
            var insertOperation = TableOperation.Insert(foo);
            await table.ExecuteAsync(insertOperation);

            tableClient = _storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("testcontainers");

            //var getFooQuery = new TableQuery<FooEntity>().Where(
            //    TableQuery.CombineFilters
            //    (
            //        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "foo"),
            //        TableOperators.And,
            //        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "bar")
            //    )
            //);

            var getFooQuery = new TableQuery<FooEntity>();

            getFooQuery.TakeCount = 20;

            TableContinuationToken token = null;

            var segment = await table.ExecuteQuerySegmentedAsync(getFooQuery, token);

            var expectedFoo = segment.Results.Single();

            expectedFoo.Should().BeEquivalentTo(foo, o => o.Excluding(p => p.Timestamp));
        }

        class FooEntity : TableEntity
        {
            public FooEntity() { }
            public FooEntity(string foo, string bar) => (PartitionKey, RowKey) = (foo, bar);
        }

        [Fact]
        public async Task SimpleQueueTest()
        {
            var queueClient = _storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("testcontainers");
            await queue.CreateIfNotExistsAsync();

            await queue.AddMessageAsync(new CloudQueueMessage("testcontainers!"));

            var expectedQueuedMessage = await queue.PeekMessageAsync();
            expectedQueuedMessage.AsString.Should().Be("testcontainers!");
            //queue.ApproximateMessageCount.Should().Be(1);

            var expectedMessage = await queue.GetMessageAsync();
            expectedMessage.AsString.Should().Be("testcontainers!");
            //queue.ApproximateMessageCount.Should().Be(0);

            var expectedNoMessage = await queue.GetMessageAsync();
            expectedNoMessage.Should().BeNull();
        }
    }

}