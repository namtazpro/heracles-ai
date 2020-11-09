﻿using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Heracles.UI
{
    class UIControllerWorker
    {
        private readonly Random random = new Random();
        private readonly HeraclesContext _heraclesContext;
        private  UIParameters parameters;
        private readonly UIControllerUtil util;
       
        IWebDriver driver;

        internal UIControllerWorker(HeraclesContext heraclesContext)
        {
            _heraclesContext = heraclesContext;
            util = new UIControllerUtil(_heraclesContext);
            if (util.UserSimulationEnabled())
            {
                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--start-maximized");
                chromeOptions.AddArgument("--disable-dev-shm-usage"); // overcome limited resource problems
                chromeOptions.AddArgument("--no-sandbox"); // Bypass OS security model
                chromeOptions.AddArgument("--headless");
                chromeOptions.AddArgument("--verbose");
                chromeOptions.AddArgument("--whitelisted-ips=");
                chromeOptions.AddArgument("--user-agent=" + "Zodiac.Generator");
                try
                {
                    driver = new ChromeDriver(chromeOptions);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        internal UIUser GetRandomUser()
        {
            return parameters.Users[random.Next(0, parameters.Users.Count())];
        }

        internal UISession GetRandomSession()
        {
            return parameters.Sessions[random.Next(0, parameters.Sessions.Count())];
        }

        internal async Task<int> Run(ILogger log, string triggerFunction, int calls = 0)
        {
            if (util.UserSimulationEnabled(log))
            {
                parameters = await new UIControllerUtil(_heraclesContext).GetParameters(log);

                if (parameters.Users[0].Id != @"user1@tenant.onmicrosoft.com")
                {

                    Random random = new Random();
                    log.LogDebug($"User Simulation {calls} journeys requested");
                    if (calls == 0)
                    {
                        calls = random.Next(1, _heraclesContext.NumberOfUserJourneys);
                        log.LogDebug($"Number of simulations unspecified, randomly selecting {calls} simulations");
                    }
                    driver.Navigate().GoToUrl(_heraclesContext.BaseUrl);
                    AADLogin(GetRandomUser(), log);
                    try
                    {
                        for (int i = 0; i < calls; i++)
                        {
                            log.LogDebug($"Simulation session {i + 1} of {calls}");
                            EnactSession(GetRandomSession(), i + 1, log);
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError($"Exception while generating synthetic user traffic! Message: {e}");
                    }
                    return calls;
                }
                log.LogWarning($"Simulation not possible.  Default user still in simulation configuration {parameters.Users[0].Id}");
            }
            return 0;
        }

       

        private void EnactSession(UISession session, int sessionNumber, ILogger log)
        {
            // Click the Go button
            driver.Navigate().GoToUrl($"{_heraclesContext.BaseUrl}/capricorn");
            Thread.Sleep(3000);
            log.LogDebug($"Session {sessionNumber}: Go to capricorn home page");
            FindAndClick("capricorn-go", log);
            Think();

            for (int i = 0; i < session.Steps.Length - 1; i++)
            {
                log.LogDebug($"Session {sessionNumber}: Enacting Step {i+1} of {session.Steps.Length}");
                FindAndClick(session.Steps[i], log);
                Think();
            }
        }

        private void AADLogin(UIUser user, ILogger log)
        {
            log.LogDebug($"Logging in user {user.Id}");
            // Click Capricorn Option (causes ADD Login)
            FindAndClick("action-Capricorn", log);
            Think();

            // Fill in email address and click next.
            SendToElement("i0116", user.Id, log);
            Think();
            FindAndClick("idSIButton9", log);
            Think();

            // Fill in the password and click next
            SendToElement("i0118", user.Password, log, false);
            Think();
            FindAndClick("idSIButton9", log);
            Think();

            // Do not select the stay signed in option
            try
            {
                FindAndClick("idBtn_Back", log);
                Think();
            }
            catch (Exception){}
            log.LogDebug($"Logged in user {user.Id}");
        }

        private void Think()
        {
            int thinkTime = random.Next(_heraclesContext.MinimumThinkTimeInMilliseconds, 3000);
            Thread.Sleep(thinkTime);
        }

        public IWebElement FindAndClick(string Id, ILogger log)
        {
            try
            {
                var element = driver.FindElement(By.Id(Id));
                element.Click();
                log.LogDebug($"Found and Clicked element with Id {Id}");
                return element;
            }
            catch (Exception e)
            {
                log.LogError($"FindAndClick(): Exception {e}");
                throw e;
            }
        }

        public void SendToElement(string Id, string data, ILogger log, bool shouldLog = true)
        {
            try
            {
                var element = driver.FindElement(By.Id(Id));
                log.LogTrace($"Found element with Id {Id}");
                element.SendKeys(data);
                if (shouldLog)
                {
                    log.LogDebug($"Sent {data} to element with Id {Id}");
                }
                else 
                {
                    log.LogDebug($"Sent ***************** to element with Id {Id}");
                }
            }
            catch (Exception e)
            {
                log.LogError($"SendToElement(): Exception {e.Message}");
                throw e;
            }
        }

    }
}
