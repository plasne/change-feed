# Change Feed

This sample shows an implementation of using an Azure Event Hub as a change feed.

A common use-case might look like this:

- An application is comprised of multiple processors and APIs.
- A user submits a change to a Customer entity via an API.
- The API calls `NotifyAsync()` with a payload of "evict:customers" (an indication of the scope to evict).
- The processors use `ListenAsync()` combined with the `OnNotifiedAsync` event handler to evict all customers from cache.

## Payload

The payload is arbitrary, it is up to the sender and receivers to agree on the payload to send and how to interpret it. Some common examples might be:

- __evict:???__: Evict some scope of entities from cache.
- __process:???__: Perform some activity.
- __log:???__: Change the log level.

### Consumer Groups

This implementation means that each replica for each service is considered a [non-epoch consumer](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-event-processor-host#no-epoch), and is therefore limited such that no consumer group may have more than 5 consumers. For instance, if you have 3 services with 3 replicas each, you will have 9 consumers and therefore need at least 2 consumer groups. If a service is not going to support more than 5 replicas, then you could simply have a consumer group dedicated to that service. If you have a service that may have more than 5 replicas, then you will need to have multiple consumer groups for that service and some way to load balance between them. Based on tier, there is also a maximum number of consumer groups per Event Hub, which can be found [here](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/azure-subscription-service-limits#basic-vs-standard-vs-premium-vs-dedicated-tiers).

If the limit is exceeded, the EventHubsException.FailureReason.QuotaExceeded exception will be thrown by the client when trying to read messages from a partition. This exception is not thrown when accessing the Event Hub in other ways, for instance, reading metadata.

## Partitions

Since each replica of each service is going to read all messages from all partitions and because the number of messages per second is likely to be small, it is recommended that you use as few partitions as possible for the load. Generally 1 partition is appropriate.

## Get This Sample Running

This sample consists of two projects:

- __library__: This is the implementation of the change feed.
- __client__: This is a sample client that uses the library.

To run this sample, you will need to create an Event Hub in Azure.

Then create an `appsettings.json` file in the client project with the following contents:

```json
{
    "CHANGEFEED_CONNSTRING": "DefaultEndpointsProtocol=https;AccountName=???;AccountKey=???;EndpointSuffix=core.windows.net",
    "CHANGEFEED_CONSUMER_GROUPS": "???, ???, ???"
}
```

You should set CHANGEFEED_CONNSTRING as appropriate, and it must specify an EntityPath (the name of the Event Hub in this namespace). You need to create enough consumer groups in the Event Hub to cover your use-case (as described above) and set CHANGEFEED_CONSUMER_GROUPS to a comma-delimited list of the consumer group names.

You can run the client project from the command line by typing:

```bash
dotnet run
```

## Alternatives

I don't believe there are great alternatives, but a few possibilities were mentioned by others:

- Event Grid. The problem with Event Grid is that it pushes event messages, rather than allowing the individual replicas to pull them. Therefore, Event Grid would need to be configured with the IP address or host name of every replica, both of which can change over the lifetime of the deployment.

- Service Bus Topic/Subscriptions. Services could publish changes to the topic but allow many subscribers to get those change notifications. The challenge is subscriptions have to be provisioned for new replica and de-provisioned as they are no longer needed. This is possible via the ARM REST API, but increases complexity.
