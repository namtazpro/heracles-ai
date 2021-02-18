using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Heracles
{
    internal class HeraclesGeneratorWorker
    {
        private readonly HeraclesContext _heraclesContext;
        
        internal HeraclesGeneratorWorker(HeraclesContext heraclesContext)
        {
            
            _heraclesContext = heraclesContext;
        }
        internal async Task<int> Run(ILogger log, string triggerFunction, int calls = 0)
        {
            Random random = new Random(); 
            if (calls == 0)
            {
                calls = random.Next(1, _heraclesContext.NumberOfCallsPerInvocation);
            }
            
            string[] pageMaster = { "mares-of-diomedes", "nemean-lion", "cretan-bull", "belt-of-hippolyta", "apples-of-hesperides", "lernaean-hydra", "ceryneian-hind", "erymanthian-boar", "stymphalian-birds", "augean-stables", "cerberus" };
            log.LogInformation($"About to generate {calls} calls.");
            List<string> pages = new List<string>();
            for (int i = 0; i < calls; i++)
            {
                int min = 1;
                int max = pageMaster.Length + 1;
                int index = random.Next(min, max);
                var traceGuid = Guid.NewGuid().ToString();
                var pageString = $"{pageMaster[index - 1]}?traceGuid=INSIGHTSGENERATOR: {traceGuid}";
                if (pageMaster[index - 1] == "pisces")
                {
                    if (shouldMaxCPU()) pageString += "&cpumax=true"; else pageString += "&cpumax=false";
                }
                pages.Add(pageString);
            }
            string baseUrl = $"{_heraclesContext.BaseUrl}/home/";
            int j = 0;
            foreach (var page in pages)
            {
                j++;
                await RestApi.Call(baseUrl, page, log);
                if (j % 25 == 0) log.LogDebug($"{j} calls have been made");
            }
            log.LogInformation($"Done. {j} calls were made in total.");
            return calls;
        }
        private static bool shouldMaxCPU()
        {
            int min = 1;
            int max = 5;
            Random random = new Random();
            int index = random.Next(min, max);
            if (index == 3) return true;
            return false;
        }
    }
}
