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

### [2023-03-12] 2.1.1
- Updated to Genocs.Core 3.0.0

### [2023-02-06] 2.1.0
- Updated Settings section name