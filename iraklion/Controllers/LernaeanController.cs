﻿using System;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Iraklion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LernaeanController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;

        public LernaeanController(ILogger<LernaeanController> logger, TelemetryClient telemetryClient)
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
            const string controllerName = "lernaean-hydra";

            var metricName = $"{controllerName}Transactions";
            var message = $"{controllerName} has been invoked. TraceGuid={traceGuid}";
            _telemetryClient.TrackEvent(message);
            _telemetryClient.GetMetric(metricName).TrackValue(1);
            _logger.LogInformation(message);

            // If it's between 8.00am and 8.30aam service is unavailable
            var currentTime = DateTime.Now;
            var startTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 16, 0, 0);
            var endTime = startTime.AddMinutes(30);
            if ((currentTime > startTime) && (currentTime < endTime)) 
            {
                throw new Exception($"Service is not available between {startTime.ToLongTimeString()} and {endTime.ToLongTimeString()}");
            }

            int min = 1;
            int max = 10;
            Random random = new Random();
            int delay = random.Next(min, max);
            // if the delay is 10, then make it much longer
            if (delay == 10) delay += 20;

            _logger.LogInformation($"lernaean-hydra will sleep for {delay} seconds");
            Thread.Sleep(delay * 1000);
            return "Hello from lernaean-hydra!";
        }

       

    }
}
