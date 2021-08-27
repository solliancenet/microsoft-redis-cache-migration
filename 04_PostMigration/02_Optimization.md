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

The [Performance Recommendations](https://docs.microsoft.com/en-us/azure/Redis/concepts-performance-recommendations) feature analyzes workloads across the server to identify indexes with the potential to improve performance. The tool requires the Query Store feature to be enabled.  Once enabled, manually review and implement any suggestions.

## Azure Advisor

[Azure Advisor](https://docs.microsoft.com/en-us/azure/Redis/concepts-azure-advisor-recommendations) will provide recommendations around Azure Cache for Redis Performance, Reliability and Cost.  For example, Azure Advisor could present a recommendation to modify a server parameter based on the workloads and instance pricing tier and available scaling settings.

## Upgrading the Tier

The Azure Portal can be used to scale between from `General Purpose` and `Memory Optimized`. If a `Basic` tier is chosen, there will be no option to upgrade the tier to `General Purpose` or `Memory Optimized` later. However, it is possible to utilize other techniques to perform a migration/upgrade to a new Azure Cache for Redis instance.

For an example of a script that will migrate from Basic to another server tier, reference [Upgrade from Basic to General Purpose or Memory Optimized tiers in Azure Cache for Redis](https://techcommunity.microsoft.com/t5/azure-instance-for-Redis/upgrade-from-basic-to-general-purpose-or-memory-optimized-tiers/ba-p/690976).

## Scale the Server

Within the tier, it is possible to scale cores and memory to the minimum and maximum limits allowed in that tier. If monitoring shows a continual maxing out of CPU or memory, follow the steps to [Manage an Azure Cache for Redis server using Azure Portal](https://docs.microsoft.com/en-us/azure/Redis/howto-create-manage-server-portal).

## Moving Regions

Moving a instance to a different Azure region depends on the approach and architecture.  Depending on the approach, it could cause system downtime.

The recommended process is the same as utilizing read replicas for maintenance failover. However, compared to the planned maintenance method mentioned above, the speed to failover is much faster when a failover layer has been implemented in the application. The application should only be down for a few moments during the read replica failover process. More details are covered in the [Business Continuity and Disaster Recovery](03_BCDR.md) section.

## Quick Tips

Use the following to make quick performance changes:

- **CPU Usage** : If CPU usage for an Azure Cache for Redis server is saturated at 100%, then select the next higher level of Compute Units to get more CPU.
- **IOPS** : The default storage size of 125GB is limited to 375 IOPs. If the application requires higher IOPs, then it is recommended that a higher storage size be selected to get more IOPs.
- **Regions** :  It is recommended having the application server/client machine in the same region in Azure to reduce latency between the client/application server and the instance.
- **Accelerated Networking** : Accelerated networking enables single root I/O virtualization (SR-IOV) to a VM, greatly improving its networking performance. This high-performance path bypasses the host from the datapath reducing latency, jitter, and CPU utilization for use with the most demanding network workloads on supported VM types.

## WWI Use Case

WWI business and application users expressed a high level of excitement regarding the ability to scale the instance on-demand. They were also interested in using the Query Performance Insight to determine if long running queries performance needed to be addressed.

They opted to utilize a read replica server for any potential failover or read-only needed scenarios.

The migration team, working with the Azure engineers, set up KQL queries to monitor for any potential issues with the Redis server performance. The KQY queries were set up with alerts to email event issues to the instance and conference team.

They elected to monitor any potential issues for now and implement Azure Automation run books at a later time, if needed, to improve operational efficiency.

## Optimization Checklist

- Enable Query Store
- Monitor for slow queries.
- Periodically review the Performance and Azure Advisor recommendations.
- Utilize monitoring to drive tier upgrades and scale decisions.
- Consider moving regions if the users or application needs change.
