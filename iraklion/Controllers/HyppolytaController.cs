﻿using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Iraklion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HippolytaController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;

        public HippolytaController(ILogger<HippolytaController> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

         
         /// <summary>
         /// This api waits for a time before returning to the caller
         /// </summary>
         /// <returns>a string containing hello world</returns>
        [HttpGet]
        public ActionResult<string> Get(string traceGuid)
        {
            const string controllerName = "belt-of-hippolyta";

            var metricName = $"{controllerName}Transactions";
            var message = $"{controllerName} has been invoked. TraceGuid={traceGuid}";
            _telemetryClient.TrackEvent(message);
            _telemetryClient.GetMetric(metricName).TrackValue(1);
            _logger.LogInformation(message);
            // if its between 8.00 and 8.30 am take a long time
            var hour = System.DateTime.Now.Hour;
            var minute = System.DateTime.Now.Minute;
            if ((hour == 8) && (minute <31))
            {
                _logger.LogInformation($"hippolyta will sleep for 39 seconds!");
                Thread.Sleep(39000);
            }
            return "Hello World from hippolyta!";
        }

       

    }
}
