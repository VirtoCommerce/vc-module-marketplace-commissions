#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.MarketplaceCommissionsModule.Data.BackgroundJobs;
using VirtoCommerce.MarketplaceCommissionsModule.Data.IntegrationEventHandlers;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using Xunit;

namespace VirtoCommerce.MarketplaceCommissionsModule.Tests.Unit
{
    // The event handler enqueues through the static BackgroundJob facade, which this class points at its own stub:
    // any other test class that enqueues through the facade must join this collection.
    [Collection(nameof(CommissionFeesJobTests))]
    public class CommissionFeesJobTests : IDisposable
    {
        private readonly BackgroundJobStub _backgroundJob = new();
        private readonly ServiceProvider _provider;

        public CommissionFeesJobTests()
        {
            var services = new ServiceCollection();
            services.AddScoped<IBackgroundJob>(_ => _backgroundJob);
            _provider = services.BuildServiceProvider(validateScopes: true);
            BackgroundJob.Initialize(_provider);
        }

        public void Dispose()
        {
            _provider.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task OrderCreated_EnqueuesTheAddedOrders()
        {
            // Arrange
            var sut = new CustomerOrderCreatedEventHandler(null!, null!, null!);
            var added = new CustomerOrder { Id = "added" };
            var modified = new CustomerOrder { Id = "modified" };
            var message = new OrderChangedEvent(
            [
                new GenericChangedEntry<CustomerOrder>(added, EntryState.Added),
                new GenericChangedEntry<CustomerOrder>(modified, modified, EntryState.Modified),
            ]);

            // Act
            await sut.Handle(message);

            // Assert
            var job = _backgroundJob.Enqueued.Should().ContainSingle().Subject;
            job.Handler.Should().Be<CommissionFeesJobHandler>();
            job.Payload.Should().BeOfType<CommissionFeesJobPayload>().Which.OrderIds.Should().Equal("added");
        }

        [Fact]
        public async Task CommissionFeesJob_HandlesEachOrderUnderItsOwnLock()
        {
            // Arrange
            var eventHandler = new Mock<CustomerOrderCreatedEventHandler>(null!, null!, null!);
            var distributedLock = new DistributedLockStub();
            var sut = new CommissionFeesJobHandler(eventHandler.Object, distributedLock);

            // Act
            await sut.Execute(new CommissionFeesJobPayload { OrderIds = ["order1", "order2"] }, Mock.Of<IJobExecutionContext>(), TestContext.Current.CancellationToken);

            // Assert
            distributedLock.AcquiredResources.Should().Equal("vcmp:commission-fees:order1", "vcmp:commission-fees:order2");
            eventHandler.Verify(x => x.HandleOrderChangesInBackground(new[] { "order1" }), Times.Once);
            eventHandler.Verify(x => x.HandleOrderChangesInBackground(new[] { "order2" }), Times.Once);
        }

        /// <summary>Records what is enqueued instead of running it.</summary>
        private sealed class BackgroundJobStub : IBackgroundJob
        {
            public List<(Type Handler, object Payload)> Enqueued { get; } = [];

            public Task<string> Enqueue<THandler>(object payload, EnqueueOptions? options = null, CancellationToken cancellationToken = default)
                where THandler : class
            {
                Enqueued.Add((typeof(THandler), payload));
                return Task.FromResult("job-1");
            }
        }

        /// <summary>Grants every lock at once and records the resources.</summary>
        private sealed class DistributedLockStub : IDistributedLock
        {
            public List<string> AcquiredResources { get; } = [];

            public Task<IDistributedLockHandle> AcquireAsync(string resource, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
            {
                AcquiredResources.Add(resource);
                return Task.FromResult<IDistributedLockHandle>(new Handle(resource));
            }

            public Task<IDistributedLockHandle?> TryAcquireAsync(string resource, TimeSpan timeout = default, CancellationToken cancellationToken = default)
            {
                AcquiredResources.Add(resource);
                return Task.FromResult<IDistributedLockHandle?>(new Handle(resource));
            }

            private sealed class Handle(string resource) : IDistributedLockHandle
            {
                public string Resource => resource;

                // never lost
                public CancellationToken HandleLostToken => CancellationToken.None;

                public void Dispose()
                {
                }

                public ValueTask DisposeAsync() => ValueTask.CompletedTask;
            }
        }
    }
}
