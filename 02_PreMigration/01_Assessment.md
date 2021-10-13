# Assessment

Before jumping right into migrating a Redis workload, there is a fair amount of due diligence that must be performed. This includes analyzing the data, hosting environment, and application workloads to validate the Azure landing zone is properly configured and prepared to host the soon-to-be migrated workloads.

## Redis Versions

Remote Dictionary Server (Redis) has a rich history starting in late 2000s.  Since then, it has evolved into a widely used memory based key value data management system. Azure Cache for Redis started with the support of Redis version 3.0 and has continued to 6.0 (as of 10/2021).  For a listing of all Redis version releases, reference [this detailed page](https://github.com/redis/redis/releases).

For the latest on Azure Cache for Redis version support, reference [Supported Azure Cache for Redis server versions](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview#redis-versions). In the Post Migration Management section, we will review how upgrades (such as 4.0 to 6.0) are applied to the Redis instances in Azure.

> **Note** Redis OSS Support is based on the release of the latest stable release and only the latest stable version, and the last two versions will receive maintenance from Redis OSS. As of 10/2021, anything prior to 4.0 is end of life.  Microsoft Cache for Redis may not exactly match to these same version support windows and may provide support for a period after Redis OSS support ends, review the [supported versions](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview#redis-versions) for the latest updates.

Knowing the source Redis version is important as many features have been introduced through the major versions. The applications using the system may be expecting behavior and features that are specific to that version. Although Redis has been great at keeping breaking changes at a minimum and keeping compatibility between versions, there are a small handful of cases that may influence the migration path and version:

- Support for Redis Modules (RedisSearch, RedisBloom, RedisTimeSeries)
- RDB format change (5.0 not backwards compatible, 4.0 is not backwards compatible)
- Change in INFO fields (4.0)
- Usage of REDIS ACL (6.0+)
- RESP3 mode (6.0+)
- Redis streams (5.0+)
- LRU Cache changes (4.0+)
- Any Lua Language changes (EVAL, EVALSHA)
- Extensive use of TTL
- Number of databases

To check the Redis server version run the following command against the Redis instance:

```bash
redis-server --version
```

For a list of changes between versions, reference the latest release documentation:

- [Redis 6.x](https://raw.githubusercontent.com/redis/redis/6.0/00-RELEASENOTES)
- [Redis 5.x](https://raw.githubusercontent.com/redis/redis/5.0/00-RELEASENOTES)
- [Redis 4.x](https://raw.githubusercontent.com/redis/redis/4.0/00-RELEASENOTES)
- [Redis 3.x](https://raw.githubusercontent.com/redis/redis/3.0/00-RELEASENOTES)

## Architecture and Objects

Data (keys and the values) is only one component of instance migration. The instance supporting configuration may also need to be migrated and validated to ensure the applications will continue to run reliably.  As part of the assessment, it is important to understand what features of the system are being used other than data storage.

Here is a list of inventory items that should be inventoried before and after the migration:

- Users
- Configuration settings

After reviewing the above items, notice there is much more than just data that may be required to migrate a Redis workload.  The following sections below address more specific details about several of the above.

> **Note** Even though you may be able to get these configuration values out of your source system, it is unlikely that you will be able to import them using the same commands as many configurations must be done via the Azure Portal, PowerShell or Azure Cli using Azure specific syntax.  For example, the `config` command is not exposed in Azure Cache for Redis.

## Limitations

Azure Cache for Redis is a fully supported version of Redis running as a platform as a service. However, there are some common limitations to become familiar with when doing an initial assessment for selected your landing zone. Many of these limitations are driven by the tier selected as shown in the online [supported features document](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview#feature-comparison)

Many of the other items are simply operational aspects that administrators should become familiar with as part of the operational data workload lifecycle management. This guide will explore many of these operational aspects in the [Post Migration Management](#management) section.

- Each tier supports a maximum number of databases (when not in cluster mode).  If you have more than the default of `16`, be sure that you pick a tier to migrate too that has support for all source databases.

- Although you can cluster enable the premium (Enterprise tiers are cluster enabled by default), in doing so, you will only be able to support the `db0` database.  If you are using a tool that supports migrating databases, you will need to ensure that you move all source databases to the `db0` database in the target.

- You cannot cluster enable `Basic` or `Standard` tiers so migrating via cluster failover is not an option. You can cluster enable a premium instance, but it will become part of its own cluster and you cannot use it to cluster failover.

- Once you cluster enabled the premium instance or use an Enterprise Tier instance, it will communicate over the [Redis cluster protocol](https://redis.io/topics/cluster-spec).

### Redis Modules

You can extend the features of Redis by implemented custom Redis modules.  Look for any `loadmodule` directives in the `redis.conf` file that are not part of the default installation.  You can also get a list of all modules by running:

  ```bash
  redis-cli MODULE LIST
  ```

## Databases

When performing a migration, consider the Redis instance may have more than one database. Databases in Redis were not designed for scaling but rather for namespaces. For example, a SaaS Application may run one code base but hundreds of clients each with their own namespace / Redis database. Databases allow you to flush a client without affecting others and minimize administrative overhead.

The tool you select will need to be able to support migrating keys in all databases and ensuring they are moved to the target appropriately versus just moving the default `0` database. You can find the number of databases by running the following:

  ```bash
  redis-cli INFO keyspace
  ```

OR

  ```bash
  redis-cli config get databases
  ```

## Source Systems

The amount of migration preparation can vary depending on the source system and its location. In addition to the instance objects, consider how to get the data from the source system to the target system. Migrating data can become challenging when there are firewalls and other networking components in between the source and target.

Internet migration speed is an important factor. Moving data over the Internet can be slower than using dedicated circuits to Azure.  Consider setting up an [ExpressRoute](https://docs.microsoft.com/en-us/azure/expressroute/expressroute-introduction) connection between the source network and the Azure network when moving many gigabytes, terabytes, and petabytes of data.

Do not overwhelm existing network infrastructure. If ExpressRoute is already present, the connection is likely being used by other applications.  Performing a migration over an existing route can cause strain on the network throughput and potentially cause considerable performance degradation for both the migration and other applications using the network.

Lastly, disk space must be evaluated. When exporting a very large instance, consider the size of the data. Ensure the system where the tool is running and the export location have enough disk space to perform the export operation.

### Clusters

If the source system is running in a cluster, you will need to ensure that the target instance is also running in a similarly configured cluster with the same performance metrics as the source.

### Hashing layers

When not using clusters, you can place a `hashing` layer in front of a set of Redis servers.  In this case, you will need to ensure that you have the same technology sitting in front of the Azure Cache for Redis instances. The path or tool you use to migrate will need to be tested with whatever hashing layer you are using to ensure that all keys are discovered and migrated to the target.

For instance, the default source code for the tool `twemproxy` will require all the target servers to have the same password or no password.  You cannot change the Azure Cache for Redis password/keys to a custom value.  This means you would need to setup Redis cache servers in Virtual Machines in a private network and place the `twemproxy` in front of them.

During the migration from the hashing layer, you will need to export keys from each of the source servers and then add the values through the new hashing layer setup with the same configuration on the target side.

### Cloud Providers

Migrating instances from cloud services providers, such as Google Cloud (GCP) and Amazon Web Services (AWS), may require extra networking configuration steps to access the cloud-hosted Redis instances or they may prevent Redis migration commands. Any first party or third-party migration tools will require access from outside IP ranges and may be blocked by default.

### On-premises

Like cloud provider-hosted environments, if the Redis data environment is behind corporate firewalls or other network security layers, a path will need to be created between the on-premises instance and Azure Cache for Redis.

## Performance Analysis Tools

Many tools and methods can be used to assess the Redis data workloads and environments. Each tool will provide a different set of assessment and migration features and functionality. As part of this guide, we will review the most commonly used tools for assessing Redis data workloads.

## Azure Cache for Redis - Service Tiers

Equipped with the assessment information (CPU, memory, storage, etc.), the migration user's next choice is to decide which Azure Cache for Redis service and pricing tier to start with.

There are currently five potential options:

- **Azure Cache for Redis (Basic)**: An OSS Redis cache running on a single VM. This tier has no service-level agreement (SLA) and is ideal for development/test and non-critical workloads.

- **Azure Cache for Redis (Standard)**: An OSS Redis cache running on two VMs in a replicated configuration.

- **Azure Cache for Redis (Premium)**: High-performance OSS Redis caches. This tier offers higher throughput, lower latency, better availability, and more features. Premium caches are deployed on more powerful VMs compared to the VMs for Basic or Standard caches.

- **Azure Cache for Redis (Enterprise)**: High-performance caches powered by Redis Labs' Redis Enterprise software. This tier supports Redis modules including RediSearch, RedisBloom, and RedisTimeSeries. Also, it offers even higher availability than the Premium tier.

- **Azure Cache for Redis (Enterprise Flash)**: Cost-effective large caches powered by Redis Labs' Redis Enterprise software. This tier extends Redis data storage to non-volatile memory, which is cheaper than DRAM, on a VM. It reduces the overall per-GB memory cost.

Briefly, these options were discussed in the [Limitations](##Limitations) document.

### Comparison of Services

Which Azure Cache for Redis service should be selected and used?  This table outlines some of the advantages and disadvantages of each along with their Redis version support as of 4/2021.

| **Service** | **Pros** | **Cons** | **Versions Supported**
| --- | --- |--- |--- |
| Azure VM | Any version, most flexible, full Redis feature support | Customer responsible for updates, security, and administration | Any Version
| Basic | Sizes up to 53GB, low cost | Lower performance, no data persistence, no replication or failover | 4.x, 6.x
| Standard | All basic, plus replication and failover support | Lower performance, no data persistence, no geo-replication | 4.x, 6.x
| Premium | All Standard, plus zone redundancy, data persistence and clustering | No support for Redis Modules, no active geo-replication | 4.x, 6.x
| Enterprise | All Premium, plus Redis Module support and active geo-replication | Higher costs | 6.x
| Enterprise Flash | Flash based memory | No Redis Module support | 6.x

As displayed above, if the instance is running Redis 3.x or lower and do not plan to upgrade, the workload will need to run in an Azure VM.

### Costs

After evaluating the entire WWI Redis data workloads, WWI determined they would need at least 6GB of cache capacity with data persistence and clustering support so a Premium Sku was selected. WWI intentionally chose to begin its Azure migration journey with a relatively small workload. However, the best practices of instance migration still apply and will be used as a template for future migrations.

To determine the memory usage, they interrogated the Redis processes on their source system during a heavy load period:

  ```bash
  ps -o pid,user,%mem,command ax | sort -b -k3 -r
  ```

They then monitored the network bandwidth to see how much traffic was being used between the clients and the Redis server. They measured about 15% cache usage per hour which equated to 900MB of traffic per hour which equates to 328GB of traffic per year.  The current application will not be moved to the same Azure region but will utilize the Azure Redis Cache which means network bandwidth will have to be paid.  They had a couple of tools to choose from to monitor the network traffic (`iptraf` and `nethogs`):

```bash
sudo apt-get install iptraf -y

sudo netstat =tump | grep <port_number>
```

```bash
sudo apt-get install nethogs

sudo nethogs
```

Additionally, because they want [data persistence](https://redis.io/topics/persistence) and backups, they will persist this to Azure Storage.

Using the [Azure Cache for Redis pricing calculator](https://azure.microsoft.com/en-us/pricing/details/cache/) WWI was able to determine the costs for the Azure Cache for Redis instance. As of 8/2021, the total costs of ownership (TCO) is displayed in the following table for the WWI Conference instance:

 | **Resource** | **Description** | **Quantity** | **Cost** |
 | --- | --- | --- | --- |
 | Compute (Premium) | 6GB Memory (1 primary, 1 replica) | 24 x 365 @ $0.554/hr | $4853.04 / yr |
 | Storage (backup) | 6GB | 6 * 12 @ $0.15 | $10.80 / yr |
 | Network | ~27.37GB/month egress | 12 * 22.37 * $.08 | $21.4752 / yr
 | Total |  |   | $4885.31 / yr |

After reviewing the initial costs, WWI's CIO confirmed they will be on Azure for a period much longer than 3 years. They decided to use 3-year [reserve instances](https://docs.microsoft.com/en-us/azure/Redis/concept-reserved-pricing) to save an extra ~$2.6K/yr:

 | **Resource** | **Description** | **Quantity** | **Cost** |
 | --- | --- | --- | --- |
 | Compute (Premium) | 6GB Memory (1 primary, 1 replica) | 24 x 365 @ $0.249/hr | $2190 / yr |
 | Storage (backup) | 6GB | 6 * 12 @ $0.15 | $10.80 / yr |
 | Network | ~27.37GB/month egress | 12 * 22.37 * $.08 | $21.4752 / yr
 | Total   |  |  | $2222.27 / yr (~45% savings) |

As the table above shows, backups, network egress, and any extra nodes must be considered in the total cost of ownership (TCO). As more instances are added, the storage and network traffic generated would be the only extra cost-based factor to consider.

> **Note:** The estimates above do not include any [ExpressRoute](https://docs.microsoft.com/en-us/azure/expressroute/expressroute-introduction), [Azure App Gateway](https://docs.microsoft.com/en-us/azure/application-gateway/overview), [Azure Load Balancer](https://docs.microsoft.com/en-us/azure/load-balancer/load-balancer-overview), or [App Service](https://docs.microsoft.com/en-us/azure/app-service/overview) costs for the application layers.

> The above pricing can change at any time and will vary based on region.  The region used above was `West US 2`.

### Application Implications

When moving to Azure Cache for Redis, the conversion to [secure sockets layer (SSL)](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-remove-tls-10-11#configure-your-application-to-use-tls-12) based communication is likely to be one of the biggest changes for the applications. SSL is enabled by default in Azure Cache for Redis and it is likely the on-premises application and data workload is not set up to connect to Redis using SSL. When enabled, SSL usage will add some additional processing overhead and should be monitored.

> **Note** Although SSL is enabled by default, it is possible to disable. This is strongly not recommended.

Lastly, it is likely that the application configuration will need to be modified to point to the new Azure Cache for Redis server, however if you use private endpoints and have a route to the Azure Cache for Redis, you may only need to change your DNS entry to point to the new cloud-based instance.

## WWI Use Case

WWI started the assessment by gathering information about their Redis instances. They were able to compile the following:

 | **Name** | **Source** | **Size** | **Data Persistence** | **Version** | **Owner** | **Downtime** |
 | --- | --- | --- | ---- | ---- | ---- | ---- |
 | Redis (Www) | AWS (PaaS) | 6GB | yes | 3.0 | Information Technology | 4 hr |
 | Redis (Database) | On-premises | 12GB | yes | 5.0 | Information Technology | 1 hrs |
  
Each instance owner was contacted to determine the acceptable downtime period. The planning and migration method selected was based on the acceptable instance downtime.

For the first phase, WWI focused solely on the web site supporting instance. The team needed the migration experience to assist in the proceeding data workload migrations. The www instance was selected because of the simple instance structure and the lenient downtime requirements. Once the instance was migrated, the team focused on migrating the application into the secure Azure landing zone.

## Assessment Checklist

- Test the workload runs successfully on the target system.
- Ensure the right networking components are in place for the migration.
- Understand the data workload resource requirements.
- Estimate the total costs.
- Understand the downtime requirements.
- Be prepared to make application changes.
