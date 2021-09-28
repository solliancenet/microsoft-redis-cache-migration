# Post Migration Management

## Monitoring and Alerts

Once the migration has been successfully completed, the next phase it to manage the new cloud-based cache workload resources. Management operations include both control plane and data plane activities. Control plane activities are those related to the Azure resources whereas data plane is **inside** the Azure resource (in this case Redis).

Azure Cache for Redis provides for the ability to monitor both of these types of operational activities using Azure-based tools such as [Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/overview), [Log Analytics](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/design-logs-deployment) and [Azure Sentinel](https://docs.microsoft.com/en-us/azure/sentinel/overview). In addition to the Azure-based tools, security information and event management (SIEM) systems can be configured to consume these logs as well.

Whichever tool is used to monitor the new cloud-based workloads, alerts will need to be created to warn Azure and instance administrators of any suspicious activity. If a particular alert event has a well-defined remediation path, alerts can fire automated [Azure run books](https://docs.microsoft.com/en-us/azure/automation/automation-quickstart-create-runbook) to address the event.

The first step to creating a fully monitored environment is to enable Redis log data to flow into Azure Monitor.  Reference [monitor Azure Cache for Redis](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-monitor) for more information.

Once log data is flowing, use the [Kusto Query Language (KQL)](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/) query language to query the various log information. Administrators unfamiliar with KQL can find a SQL to KQL cheat sheet [here](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/sqlcheatsheet) or the [Get started with log queries in Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/log-query/get-started-queries) page.

For example, to get the memory usage of the Azure Cache for Redis:

```kql
AzureMetrics
| where TimeGenerated > ago(15m)
| where ResourceProvider == "MICROSOFT.CACHE"
| where MetricName == "usedmemory"
| limit 10
| project TimeGenerated, Total, Maximum, Minimum, TimeGrain, UnitName
| top 1 by TimeGenerated
```

To get the CPU usage:

```kql
AzureMetrics
| where TimeGenerated > ago(15m)
| where ResourceProvider == "MICROSOFT.CACHE"
| where MetricName == "allpercentprocessortime"
| limit 10
| project TimeGenerated, Total, Maximum, Minimum, TimeGrain, UnitName
| top 1 by TimeGenerated
```

> **Note** for a list of other metrics, reference [Monitor Azure Cache for Redis](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-monitor).

Once a KQL query has been created, create [log alerts](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/alerts-unified-log) based off these queries.

## Server Configuration

As part of the migration, it is likely the on-premises server configuration was modified to support a fast egress (such as disabled AOF). Also, modifications were made to the Azure Cache for Redis configuration to support a fast ingress. The Azure Redis instance configuration should be set back to their original on-premises workload optimized values after the migration.

However, be sure to review and make server configuration changes that are appropriate for the workload and the environment. Some values that were great for an on-premises environment, may not be optimal for a cloud-based environment. Additionally, when planning to migrate the current on-premises configuration to Azure, verify that they can in fact be set.  

## PowerShell Module

The Azure Portal and Windows PowerShell can be used for managing the Azure Cache for Redis. To get started with PowerShell, install the Azure PowerShell cmdlets for Redis with the following PowerShell command:

```PowerShell
Install-Module -Name Az.RedisCache
```

After the modules are installed, reference tutorials and documentation like the following to learn ways to take advantage of scripting various management activities:

- [Manage Azure Cache for Redis with Azure PowerShell](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-manage-redis-cache-powershell)

## Azure Cache for Redis Upgrade Process

Since Azure Cache for Redis is a PaaS offering, administrators are not responsible for the management of the updates on the operating system or the Redis software. However, it is important to be aware the upgrade process can be random and when being deployed, will stop the Redis server workloads. Plan for these downtimes by rerouting the workloads to a replica in the event the particular instance goes into maintenance mode, or utilize a replication based SKU to ensure uptime.

> **Note:** This style of failover architecture may require changes to the applications layer to support this type of failover scenario. If the replica is maintained as a read only replica and is not promoted, the application will only be able to read data and it may fail when any operation attempts to write information to the instance.

### Scheduled updates

Schedule updates allows you to choose a maintenance window for your cache instance. A maintenance window allows you to control the day(s) and time(s) of a week during which the VM(s) hosting your cache can be updated. Azure Cache for Redis will make a best effort to start and finish updating Redis server software within the specified time window you define.

The default, and minimum, maintenance window for updates is five hours. This value isn't configurable from the Azure portal, but you can configure it in PowerShell using the MaintenanceWindow parameter of the [New-AzRedisCacheScheduleEntry](https://docs.microsoft.com/en-us/powershell/module/az.rediscache/new-azrediscachescheduleentry) cmdlet.

### Application support

The [AzureRedisEvents](https://docs.microsoft.com/en-us/azure/Redis/concepts-planned-maintenance-notification) feature will inform applications up to 30 seconds in advance of installation of an update or critical security patch.  

> **Note:** Azure Cache for Redis maintenance notifications are incredibly important.  The instance maintenance can take the instance and connected applications down for a random period of time. Nodes are patched one at a time to prevent data loss. Basic caches will have data loss. Clustered caches are patched one shard at a time.

In addition to supporting the Azure Redis Events, you can follow some best practices for application design when using caching technology:

- [Reliability patterns - Cloud Design Patterns](https://docs.microsoft.com/en-us/azure/architecture/framework/resiliency/reliability-patterns#resiliency)
- [Retry guidance for Azure services - Best practices for cloud applications](https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific)
- [Implement retries with exponential backoff](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-retries-exponential-backoff)

## WWI Use Case

WWI decided to utilize the Azure Activity logs and enable Redis logging to flow to a [Log Analytics workspace](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/design-logs-deployment). This workspace is configured to be a part of [Azure Sentinel](https://docs.microsoft.com/en-us/azure/sentinel/) such that any [Threat Analytics](https://docs.microsoft.com/en-us/azure/Redis/concepts-data-access-and-security-threat-protection) events would be surfaced, and incidents created.

## Management Checklist

- Create resource alerts for common things like CPU and Memory.
- Ensure the server is configured for the target data workload after migration.
- Script common administrative tasks.
- Set up notifications for maintenance events such as upgrades and patches. Notify users as necessary.
- Ensure the application can handle maintenance activities
