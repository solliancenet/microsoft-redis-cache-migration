# Optimization

## Monitoring Hardware and Query Performance

In addition to the audit and activity logs, server performance can also be monitored with [Azure Metrics](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/data-platform-metrics). Azure metrics are provided in a one-minute frequency and alerts can be configured from them. For more information, reference [Monitoring in Azure Cache for Redis](https://docs.microsoft.com/en-us/azure/Redis/concepts-monitoring) for specifics on what kind of metrics that can be monitored.

As previously mentioned, monitoring metrics such as the `cpu_percent` or `memory_percent` can be important when deciding to upgrade the instance tier. Consistently high values could indicate a tier upgrade is necessary.

Additionally, if cpu and memory do not seem to be the issue, administrators can explore instance-based options such as indexing and query modifications for poor performing queries.

To find poor performing queries, run the following:

```kql
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.DBFORRedis"
| where Category == 'RedisSlowLogs'
| project TimeGenerated, LogicalServerName_s, event_class_s, start_time_t , query_time_d, sql_text_s
| top 5 by query_time_d desc
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

## Performance Recommendations

TODO

The [Performance Recommendations](https://docs.microsoft.com/en-us/azure/Redis/concepts-performance-recommendations) feature analyzes workloads across the server to identify indexes with the potential to improve performance. The tool requires the Query Store feature to be enabled.  Once enabled, manually review and implement any suggestions.

## Azure Advisor

TODO

[Azure Advisor](https://docs.microsoft.com/en-us/azure/Redis/concepts-azure-advisor-recommendations) will provide recommendations around Azure Cache for Redis Performance, Reliability and Cost.  For example, Azure Advisor could present a recommendation to modify a server parameter based on the workloads and instance pricing tier and available scaling settings.

## Scale the Server

It is possible to scale up the tier at any time, however, once scaled up, you cannot scale the instance down.  You would need to re-create a lower tiered instance and then migrate to it.

## Moving Regions

Moving a instance to a different Azure region depends on the approach and architecture. Depending on the selected approach, it could cause system downtime.

The recommended process is the same as utilizing cluster replicas for maintenance failover. However, compared to the planned maintenance method mentioned above, the speed to failover is much faster when a failover layer has been implemented in the application. The application should only be down for a few moments during the read replica failover process. More details are covered in the [Business Continuity and Disaster Recovery](03_BCDR.md) section.

## Quick Tips

Use the following to make quick performance changes:

TODO

- **Memory Usage** : If CPU usage for an Azure Cache for Redis server is saturated at 100%, then select the next higher level of Compute Units to get more CPU.
- **Regions** :  It is recommended having the application server/client machine in the same region in Azure to reduce latency between the client/application server and the instance.
- **Accelerated Networking** : Accelerated networking enables single root I/O virtualization (SR-IOV) to a VM, greatly improving its networking performance. This high-performance path bypasses the host from the datapath reducing latency, jitter, and CPU utilization for use with the most demanding network workloads on supported VM types.

## WWI Use Case

WWI business and application users expressed a high level of excitement regarding the ability to scale the instance on-demand.

They opted to utilize a read replica server for any potential failover or read-only needed scenarios.

The migration team, working with the Azure engineers, set up KQL queries to monitor for any potential issues with the Redis server performance. The KQY queries were set up with alerts to email event issues to the instance and conference team.

They elected to monitor any potential issues for now and implement Azure Automation run books at a later time, if needed, to improve operational efficiency.

## Optimization Checklist

- Periodically review the Performance and Azure Advisor recommendations.
- Utilize monitoring to drive tier upgrades and scale decisions.
- Consider moving regions if the users or application needs change.
