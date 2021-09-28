# Optimization

## Monitoring Hardware and Cache Performance

In addition to the audit and activity logs, cache performance can also be monitored with [Azure Metrics](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/data-platform-metrics). Azure metrics are provided in a one-minute frequency and alerts can be configured from them. For more information, reference [Monitoring in Azure Cache for Redis](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-monitor) for specifics on what kind of metrics that can be monitored.

As previously mentioned, monitoring metrics such as the `allpercentprocessortime` or `usedmemory` can be important when deciding to upgrade the instance tier. Consistently high values could indicate a tier upgrade is necessary.

Additionally, if CPU and memory do not seem to be the issue, administrators can explore instance-based options such as cache misses.

To find cache misses, run the following:

```kql
AzureMetrics
| where ResourceProvider == "MICROSOFT.CACHE"
| where MetricName == 'cachemisses'
| limit 10
| project TimeGenerated, Total, Maximum, Minimum, TimeGrain, UnitName
| top 1 by TimeGenerated
```

## Typical Workloads and Performance Issues

There tends to be two common usage patterns with any instance system, Redis included.  These are (but are not limited too):

- An application server exposing a web endpoint on an application server, which connects to the instance.
- A client-server architecture where the client directly connects to the instance.

In consideration of the above patterns, performance issues can crop up in any of the following areas:

- **Resource Contention (Client)** - The machine/server serving as the client could be having a resource constraint which can be identified in the task manager, the Azure portal, or CLI if the client machine is running on Azure.
- **Resource Contention (Application Server)** - The machine/server acting as the application server could cause a resource constraint, which can be identified in the task manager, the Azure portal, or CLI if the application server/service VM is running on Azure. If the application server is an Azure service or virtual machine, then Azure metrics can help with determining the resource contention.
- **Resource Contention (instance Server)** - The instance service could be experiencing performance bottlenecks related to CPU, memory, and storage which can be determined from the Azure Metrics for the instance service instance.
- **Network latency** - A quick check before starting any performance benchmarking run is to determine the network latency between the client and instance using a simple SELECT 1 query. In most Azure regions, watch for less than two milliseconds of latency on `SELECT 1`  timing when using a remote client hosted on Azure in the same region as the Azure Cache for Redis server.

## Azure Monitor

[Azure Monitor for Azure Cache for Redis](https://docs.microsoft.com/en-us/azure/azure-monitor/insights/redis-cache-insights-overview) will provide you with common metrics reporting that are specific to Azure Cache for Redis.

## Scale the Server

It is possible to [scale up the tier](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-scale) at any time, however, once scaled up, you cannot scale the instance down.  You would need to re-create a lower tiered instance and then migrate to it.

> **Note** In order to scale a `Basic` to `Premium`, it must be scaled to `Standard` first.

## Moving Regions

Moving a instance to a different Azure region depends on the approach and architecture. Depending on the selected approach, it could cause system downtime.

The recommended process is the same as utilizing cluster replicas for maintenance failover. However, compared to the planned maintenance method mentioned above, the speed to failover is much faster when a failover layer has been implemented in the application. The application should only be down for a few moments during the read replica failover process. More details are covered in the [Business Continuity and Disaster Recovery](03_BCDR.md) section.

## Partitioning

Partitioning is the process of splitting your data across multiple Redis instances. Partitioning allows for much larger databases and the scaling of compute power and network bandwidth.

### twemproxy

In the absence of a Redis cluster, you can use the [`twemproxy` tool](https://github.com/twitter/twemproxy). Pronounced "two-em-proxy", aka nutcracker, it is a fast and lightweight proxy for memcached and redis protocol. It was built primarily to reduce the number of connections to the caching servers on the backend. This, together with protocol pipelining and sharding enables you to horizontally scale your distributed caching architecture.

### Other clients

In addition to running a proxy, you can select a client implementation that will hash the keys and handle the routing of your cache queries. There are multiple Redis clients with support for consistent hashing:

- [Redis-rb](https://github.com/redis/redis-rb) : A Ruby client that tries to match Redis' API one-to-one, while still providing an idiomatic interface.
- [Predis](https://github.com/nrk/predis) : A flexible and feature-complete Redis client for PHP 7.2 and newer.
- [Jedis](https://github.com/redis/jedis) : Jedis is a blazingly small and sane Redis java client.

## Quick Tips

Use the following to make quick performance changes:

- **Server load** : If your instance can't handle the request, you may need to scale up or add more shards to the cluster.
- **CPU Usage** : If CPU usage for an Azure Cache for Redis server is saturated at 100%, then select the next higher level of Compute Units to get more CPU.
- **Memory Usage** : If memory usage for an Azure Cache for Redis server is saturated at 100%, then select the next higher level of tier to get more memory.
- **Network** : If the tier runs out of bandwidth to serve the clients, the clients will start received time outs, scale up to a higher tier to get more bandwidth.
- **Regions** :  It is recommended having the application server/client machine in the same region in Azure to reduce latency between the client/application server and the instance.

You can also review the [best practices guidance on caching](https://docs.microsoft.com/en-us/azure/architecture/best-practices/caching) in the Azure Architecture Center.

## WWI Use Case

WWI business and application users expressed a high level of excitement regarding the ability to scale the instance on-demand.

They opted to utilize a read replica server for any potential failover or read-only needed scenarios.

The migration team, working with the Azure engineers, set up KQL queries to monitor for any potential issues with the Redis server performance. The KQY queries were set up with alerts to email event issues to the instance and conference team.

They elected to monitor any potential issues for now and implement Azure Automation run books at a later time, if needed, to improve operational efficiency.

## Optimization Checklist

- Periodically review the Performance and Azure Advisor recommendations.
- Utilize monitoring to drive tier upgrades and scale decisions.
- Consider moving regions if the users or application needs change.
