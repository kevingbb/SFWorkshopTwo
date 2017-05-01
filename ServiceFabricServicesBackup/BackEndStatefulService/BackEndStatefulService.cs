using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using SharedLibrary;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Data;

namespace BackEndStatefulService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class BackEndStatefulService : StatefulService, ICameras
    {
        public BackEndStatefulService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener>()
            {
                new ServiceReplicaListener((context) => this.CreateServiceRemotingListener(context))
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            IReliableDictionary<string, long> camerasDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("Cameras");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        ConditionalValue<long> result = await camerasDictionary.TryGetValueAsync(tx, "CameraCount");

                        ServiceEventSource.Current.ServiceMessage(this, "Current Camera Count Value: {0}", result.HasValue ? result.Value.ToString() : "Value does not exist.");

                        await camerasDictionary.AddOrUpdateAsync(tx, "CameraCount", 0, (key, value) => ++value);

                        // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                        // discarded, and nothing is saved to the secondary replicas.
                        await tx.CommitAsync();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                }
                catch (TimeoutException)
                {
                    //Service Fabric uses timeouts on collection operations to prevent deadlocks.
                    //If this exception is thrown, it means that this transaction was waiting the default
                    //amount of time (4 seconds) but was unable to acquire the lock. In this case we simply
                    //retry after a random backoff interval. You can also control the timeout via a parameter
                    //on the collection operation.
                    Thread.Sleep(TimeSpan.FromSeconds(new Random().Next(100, 300)));

                    continue;
                }
                catch (Exception exception)
                {
                    //For sample code only: simply trace the exception.
                    ServiceEventSource.Current.ServiceMessage(this, exception.ToString());
                }
            }
        }

        public async Task<long> GetCamerasCountAsync()
        {
            IReliableDictionary<string, long> camerasDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("Cameras");

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<long> result = await camerasDictionary.TryGetValueAsync(tx, "CameraCount");
                return result.HasValue ? result.Value : 0;
            }
        }

        public async Task<long> AddCameraAsync()
        {
            IReliableDictionary<string, long> camerasDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("Cameras");
            long newCameraCount = 0;

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                newCameraCount = await camerasDictionary.AddOrUpdateAsync(tx, "CameraCount", 0, (key, value) => ++value);

                await tx.CommitAsync();
            }

            return newCameraCount;
        }
    }
}
