using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using System.Fabric;
using SharedLibrary;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace FrontEndWebServer.Controllers
{
    [Route("api/[controller]")]
    public class CamerasController : Controller
    {
        private static readonly Uri backEndServiceUri;

        static CamerasController()
        {
            backEndServiceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/BackEndStatefulService");
        }

        // GET api/cameras
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            long camerasCount = 0;

            try
            {
                Microsoft.ServiceFabric.Services.Client.ServicePartitionKey partKey = new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(0);
                ICameras cameras = ServiceProxy.Create<ICameras>(backEndServiceUri, partKey, Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.Default, null);

                camerasCount = await cameras.GetCamerasCountAsync();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.WriteLine(String.Format("Error: {0}", exc.Message));
            }

            System.Diagnostics.Trace.WriteLine(String.Format(" Camera Count: {0}", camerasCount));
            return new string[] { camerasCount.ToString() };
        }

        // GET api/cameras/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/cameras
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            long camerasCount = 0;

            try
            {
                Microsoft.ServiceFabric.Services.Client.ServicePartitionKey partKey = new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(0);
                ICameras cameras = ServiceProxy.Create<ICameras>(backEndServiceUri, partKey, Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.Default, null);

                camerasCount = await cameras.AddCameraAsync();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.WriteLine(String.Format("Error: {0}", exc.Message));
            }

            System.Diagnostics.Trace.WriteLine(String.Format(" Camera Count: {0}", camerasCount));

            return this.Ok(camerasCount.ToString());
        }

        // PUT api/cameras/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/cameras/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
