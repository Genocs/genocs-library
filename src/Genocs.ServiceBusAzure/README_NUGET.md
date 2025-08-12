# .NET Core Azure Service Bus library

This package contains a set of base functionalities to use Azure Service Bus, designed by Genocs.
The library is built to be used with NET6, NET7 NET8 and NET9.


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

## Support

Please check the [GitHub repository](https://github.com/Genocs/genocs-library) to get more info.


## Release notes

The change log and breaking changes are listed here.

- [releases](https://github.com/Genocs/genocs-library/releases)