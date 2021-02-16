# Microservice Descriptions 
## Chania

Chania is the main user-facing website of the Heracles Application.  Chania is a dotnet core 3.1 application hosted on Windows App Service.  Chania has its own applications insights instance, and it is this instance that is inspected to get consolidated information from all the microsoervices of the application.  The demo viseos on this site generally use the Chania application insights instance.

The website makes available 12 tiles, each of which represents one of the labours of Heracles. The following sections describe what happens when a user clicks on a tile:

### Nemean Lion

### Lernaean Hydra

### Ceryneian Hind

### Erymanthian Boar

### Augean Stables

### Stymphalian Birds

### Cretan Bull

### Mares of Diomedes

### Belt of Hippolyta

### Cattle of Geryon

### Apples of Hesperides

### Cerberus

## Thessaloniki

Thessaloniki is one of two main Apis that provide the functionality for the Chania web site.  Thessaloniki is a dotnet core 3.1 application hosted on Windows App Service, and has its own Application Insights instance.  It hosts 2 main operations (that control the functionality for the Ceryneian Hind and MAres of Diomedes labours) operations. [You can view the swagger for Thessaloniki](https://hercthessaloniki-api.azurewebsites.net/index.html). The SettingsView operation is just a test, and isnt used by the Heracles application.

## Iraklion

Iraklion is the main Apis that provide the functionality for the  Chania web site. Iraklion is a dotnet core 3.1 application hosted on Linux App Service for containers.  It again has its own Application Insights instance, and it hosts the remaining 10 operations for the labours. [You can view the swagger for Iraklion](https://herciraklion-api.azurewebsites.net/index.html). The Azure portal does not support App Insights configuration for App Service for Linux, so the option is greyed out in the portal menu for the iraklion app service.  Howver, you can still see specifics relating to Iraklion by selecting the Ap Insights instance directly.

## Ioaninna

Ioannina is a very simple service that is invoked only when the Cretan Bull labour is executed.  The Cretan operation in the Iraklion api places a message on a service bus queue and messages on this queue trigger an Azure Function within the Ioannina service that processes the message.   * Notice that correlation is maintained even after asynchronous processing via a queue*. Ioannina is interesting in that it shares it's App Insights instance with IRaklion (and uses the Iraklion instance).  Contrast this with Patra below.

## Patra

Patra is a clone of Ioannina, which in this case is invoked by the Stymphalian Birds labour.  The Cretan Stymphalian in the Iraklion api places a message on a service bus queue and messages on this queue trigger an Azure Function within the Patra service that processes the message.  Unlike Ioannina, Patra has it's own App Insights instance with demonstrates that it clearly possible to have multiple App Insights instances (one per micrososervice) and still maintain correlation and end to end traceability.

## Herecles

Heracles is a utility service.  It doesnt play any part in the Labours of Heracles, but it does do some interesting things.  Heracles is an custom Azure Function app.  It's main purpose is to generate traffic periodically, so that at all times there is meaningful data to inspect in all areas of Application Insights.

Probably most importantly there is a TimerTrigger that fires every 5 minutes and generates a random number of calls to random services chosen from the 12 labours.

Additionally, [and subject to additional configuration](configure-ui-generator.md), Heracles can (optionally) generate real user traffic to the Chania site.  It does this using a ChromeDriver headless browser from the [Selenium project](https://github.com/SeleniumHQ).  When configured, this service will run every minute, will logon to the site and randomly choose navigation paths.  This enables us to examine the user tracking and web analytics capabilities withing Application Insights.

> Hercules uses a custom base container image (so that the Selenium components can be installed) and as such needs to run on a Premium function plan - which is quite costly.  IF you are very cost conscious then you may want to delete the Hracles function app and generate traffic manually.
