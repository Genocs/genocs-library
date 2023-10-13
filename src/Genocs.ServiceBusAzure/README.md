# .NET Core Azure Service Bus library

This package contains a set of base functionalities to use Azure Service Bus, designed by Genocs.
The libraries are built using .NET standard 2.1.


## Description

Core NuGet package contains Azure Service Bus functionalities to be used on DDD services.


## Support

Please check the GitHub repository getting more info.


### DataProvider Settings
Following are the project settings needed to enable monitoring

``` json

  "AzureServiceBusTopic": {
    "ConnectionString": "Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=RMQ-xxxx;SharedAccessKey=xxxx",
    "TopicName": "topic-name",
    "SubscriptionName": "subscription-name"
  },
  "AzureServiceBusQueue": {
    "ConnectionString": "Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=RMQ-xxxx;SharedAccessKey=xxxx",
    "QueueName": "queue-name"
  }

```

## Release notes

### [2023-10-13] 5.0.0-preview.5.0
- Added [editorconfig](https://editorconfig.org/)
- Added StyleCop
- Updated logo
- Updated readme

### [2023-03-12] 5.0.0-preview.4.0
- Implemented MongoDB repository interfaces

### [2023-03-12] 5.0.0
- New Architecture

### [2023-03-12] 3.1.0
- Added Builders

### [2023-03-12] 3.0.0
- Refactory to implement CQRS pattern

### [2023-03-04] 2.4.1
- Updated System.Text.Json

### [2023-03-12] 2.1.1
- Updated to Genocs.Core 3.0.0

### [2023-02-06] 2.1.0
- Updated Settings section name