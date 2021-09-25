# Business Continuity and Disaster Recovery (BCDR)

## Backup and Restore

As with any mission critical system, having a backup and restore as well as a disaster recovery (BCDR) strategy is an important part of the overall system design. If an unforseen event occurs, it is important to have the ability to restore the data to a point in time (Recovery Point Objective) and in a reasonable amount of time (Recovery Time Objective).

### Backup

Azure Cache for Redis supports automatic backups ([data persistence](https://redis.io/topics/persistence)) based on RDB or AOF features.  The backup frequency can be as low as 15 minutes or up to 24 hours. If enabled, the files are stored in Azure Storage, which should be factoring into in the total cost of ownership of your solution.

You can also choose to `export` your data using the Azure Portal, Azure CLI or Azure PowerShell.

### Restore

As you learned in the migration sections, you can restore a Redis instance from a RDB or AOF backup.

## High Availability (HA)

Azure Cache for Redis has several options for [implementing high availability](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-high-availability) across the various hosting options (IaaS or PaaS) whether you are looking for 99.9 with up to 99.999% uptime.  These include using Virtual Machines with Availability Zones and replication, or utilizing the PaaS based Premium and Enterprise SKUs for built-in clustering and geo-replication.

### Clustering

To support high availability you can enabled clustering on the `Premium` and `Enterprise` skus.  `Basic` and `Standard` do not support clustering.  You can scale up to 10 shards in Azure Cache for Redis Premium.

### Geo-replication

[Geo-replication](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-retries-exponential-backoff) allows you to created cache replication links to Azure Cache for Redis premium tier instances running in any region in Azure.  This provides for the ability to recover from any regional outages that may occur. Secondary instances are read-only and can be accessed from applications.

Geo-replication is not automatic failover, so if any issues do arise, you will need to be ready to `unlike` the replication to make the secondary instance a primary.  You would also need to manage changing the connection settings in any applications, or adding a load balancer to route traffic.

> **Note** Geo-replication is not enabled for the `Basic` or `Standard` tiers.

### Active geo-replication

The Enterprise tiers support a more advanced form of geo-replication called active geo-replication. Using conflict-free replicated data types, the Redis Enterprise software supports writes to multiple cache instances and takes care of merging of changes and resolving conflicts. You can join two or more Enterprise tier cache instances in different Azure regions to form an active geo-replicated cache.

In this configuration, both instances are active and will accept write requests.  Unlike geo-replication, active geo-replication can essentially be used for automatic failover.

#### Cache Replication Links

Once a link has been setup, there are numerous features that are not supported and some restrictions that are placed on your instances.  Reference [Configure geo-replication for Premium Azure Cache for Redis instances](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-geo-replication) for more information.  Also part of the document, reference the [Geo-replication FAQ](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-geo-replication#geo-replication-faq).

## Deleted Servers, Resource Locks

If an administrator or bad actor deletes the instance in the Azure Portal or via automated methods, it is possible that the operations could delete your instance. It is important that [resource locks](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/lock-resources) are created on the Azure Cache for Redis resource group to add an extra layer of deletion prevention to the instances.

## Regional Failure

Although rare, if a regional failure occurs geo-redundant backups or cluster nodes can be used to get the data workloads running again. It is best to have both geo-replication and a clustering available for the best protection against unexpected regional failures.

Changing the instance server region also means the endpoint will change and application configurations will need to be updated accordingly or load balancers should be utilized.

### Load Balancers

If the application is made up of many different instances around the world, it may not be feasible to update all of the clients. Utilize an [Azure Load Balancer](https://docs.microsoft.com/en-us/azure/load-balancer/load-balancer-overview) or [Application Gateway](https://docs.microsoft.com/en-us/azure/application-gateway/overview) to implement a seamless failover functionality. Although helpful and time-saving, these tools are not required for regional failover capability.

## WWI Use Case

WWI wanted to test the failover capabilities of clusters and geo-replication so they performed the steps outlined below.

### Creating a Cluster

- Open the Azure Portal.
- Browse to the Azure Cache for Redis **PREFIX-redis-basic6** instance.
- Under **Settings**, select **Scale**
- Select `C0 Standard`, then select **Select**, the instance will start to scale
- Under **Settings**, select **Scale**
- Select `P1 Premium`, then select **Select**, the instance will start to scale
- Under **Settings**, select **Cluster size**
- Select **Enable** to enable clustering on the instance
- Select **Save**

> **Note:** Each cluster node will incur additional charges equal to the main instance.

### Setup Geo-replication

- Browse to the **PREFIX-redis-prem** Azure Cache for Redis instance
- Under **Settings**, select **Geo-replication**
- Select **Add cache replication link**
- Select the **PREFIX-redis-basic6** instance
- Select **Link**, wait for the status to change to **Synced**
- Select **Unlink caches**

### Failover to replica

Once a replica has been created and has completed the replication process, it can be used for failed over. Replication will stop during a failover and make the read replica its own primary instance.

Failover Steps:

- Open the Azure Portal.
- Browse to the **PREFIX-redis-basic6** Azure Cache for Redis instance.
- Under **Settings** select **Geo-replication**
- Select **Unlink caches**, the replication will unlink and the two caches will become read/write.

## BCDR Checklist

- Modify backup frequency to meet requirements.
- Setup clustering for high-availability
- Create resource locks on resource groups.
- Setup geo-replication for regional failure mitigation
- Implement a load balancing strategy for applications for quick failover.
