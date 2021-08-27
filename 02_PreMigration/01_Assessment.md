# Assessment

Before jumping right into migrating a Redis workload, there is a fair amount of due diligence that must be performed. This includes analyzing the data, hosting environment, and application workloads to validate the Azure landing zone is properly configured and prepared to host the soon-to-be migrated workloads.

## Redis Versions

Remote Dictionary Server (Redis) has a rich history starting in late 2000s.  Since then, it has evolved into a widely used memory based key value data management system. Azure Cache for Redis started with the support of Redis version 4.0 and has continued to 6.0 (as of 8/2021).  For a listing of all Redis versions, reference [this detailed page](https://bucardo.org/postgres_all_versions.html).

For the latest on Azure Cache for Redis version support, reference [Supported Azure Cache for Redis server versions](https://docs.microsoft.com/en-us/azure/Redis/concepts-supported-versions). In the Post Migration Management section, we will review how upgrades (such as 4.0 to 6.0) are applied to the Redis instances in Azure.

> **Note** Redis Support is based on the release of the latest stable release.  Only the latest stable version, and the last two versions will receive maintenance.  As of 4/2021, anything prior to 4.0 is end of life.

Knowing the source Redis version is important as many features have been introduced through the major versions. The applications using the system may be expecting behavior and features that are specific to that version. Although Redis has been great at keeping breaking changes at a minimum and keeping compatibility between versions, there are a small handful of cases that may influence the migration path and version:

- Support for Redis Modules (RedisSearch, RedisBloom, RedisTimeSeries)
- RDB format change (5.0 not backwards compatible, 4.0 is not backwards compatible)
- Change in INFO fields (4.0)
- RESP3 mode (6.0+)
- Any Lua Language changes (EVAL, EVALSHA)

To check the Redis server version run the following SQL command against the Redis instance:

```sql
redis-server --version
```

For a list of changes between versions, reference the latest release documentation:

- [Redis 6.x](https://raw.githubusercontent.com/redis/redis/6.0/00-RELEASENOTES)
- [Redis 5.x](https://raw.githubusercontent.com/redis/redis/5.0/00-RELEASENOTES)
- [Redis 4.x](https://raw.githubusercontent.com/redis/redis/4.0/00-RELEASENOTES)
- [Redis 3.x](https://raw.githubusercontent.com/redis/redis/3.0/00-RELEASENOTES)

## Architecture and Objects

Data (keys and the values) is only one component of instance migration. The instance supporting configuration may also need to be migrated and validated to ensure the applications will continue to run reliably.  As part of the assessment, it is important to understand what features of the system are being used other than data storage.

Here is a list of inventory items that should be queried before and after the migration:

- Users
- Configuration settings

After reviewing the above items, notice there is much more than just data that may be required to migrate a Redis workload.  The following sections below address more specific details about several of the above.

## Limitations

TODO - find azure limitations

Azure Cache for Redis is a fully supported version of Redis running as a platform as a service. However, there are [some common limitations](https://docs.microsoft.com/en-us/azure/Redis/concepts-known-issues-limitations) to become familiar with when doing an initial assessment.

- Connection limits based on cache size
- 

In addition to the common limitations, each service has its limitations:

- [Single Server limitations](https://docs.microsoft.com/en-us/azure/Redis/concepts-limits)
  - No automated upgrades between major instance engine versions.
  - TODO

Many of the other items are simply operational aspects that administrators should become familiar with as part of the operational data workload lifecycle management. This guide will explore many of these operational aspects in the [Post Migration Management](../04_PostMigration/01_Management.md) section.

### User-Defined Functions and Types (C/C++)

TODO - the language Lua?

To identify these functions in the instance, run the following query:

```sql
TODO
```

Production environments that require these types of functions will need to migrate to Redis on Azure Virtual Machines.

### Extensions

Azure Cache for Redis does not support the full range of extensions that come out of the box with on-premises installs of Redis. For the latest list, run the `SELECT * FROM pg_available_extensions;` script or see the latest documentation for each service:

- [Single Server - (https://docs.microsoft.com/en-us/azure/Redis/concepts-extensions)](https://docs.microsoft.com/en-us/azure/Redis/concepts-extensions).

If using any extensions that are outside this list, evaluate if they can be removed or replaced. If they cannot be removed, select a migration path with Virtual Machine hosted Redis.

### File System Writes

Any functions, stored procedures, or application code that execute queries (such as [`COPY`](https://www.Redis.org/docs/current/sql-copy.html)) that need file system access are not allowed in Azure Cache for Redis.

Review application code to see if it makes any calls to the `COPY` command. Functions and stored procedures that contain the `COPY` command embedded can be exported.

## Source Systems

The amount of migration preparation can vary depending on the source system and its location. In addition to the instance objects, consider how to get the data from the source system to the target system. Migrating data can become challenging when there are firewalls and other networking components in between the source and target.

Internet migration speed is an important factor. Moving data over the Internet can be slower than using dedicated circuits to Azure.  Consider setting up an [ExpressRoute](https://docs.microsoft.com/en-us/azure/expressroute/expressroute-introduction) connection between the source network and the Azure network when moving many gigabytes, terabytes, and petabytes of data.

Do not overwhelm existing network infrastructure. If ExpressRoute is already present, the connection is likely being used by other applications.  Performing a migration over an existing route can cause strain on the network throughput and potentially cause considerable performance degradation for both the migration and other applications using the network.

Lastly, disk space must be evaluated. When exporting a very large instance, consider the size of the data. Ensure the system where the tool is running and the export location have enough disk space to perform the export operation.

### Cloud Providers

Migrating instances from cloud services providers, such as Amazon Web Services (AWS), may require extra networking configuration steps to access the cloud-hosted Redis instances.  Migration tools, like Azure instance Migration Service (DMS), require access from outside IP ranges and may be blocked.

### On-premises

Like cloud provider-hosted environments, if the Redis data environment is behind corporate firewalls or other network security layers, a path will need to be created between the on-premises instance and Azure Cache for Redis.

## Performance Analysis Tools

Many tools and methods can be used to assess the Redis data workloads and environments. Each tool will provide a different set of assessment and migration features and functionality. As part of this guide, we will review the most commonly used tools for assessing Redis data workloads.

## Azure Cache for Redis - Service Tiers

Equipped with the assessment information (CPU, memory, storage, etc.), the migration user's next choice is to decide which Azure Cache for Redis service and pricing tier to start with.

There are currently four potential options:

- Azure Cache for Redis (VM)
- Azure Cache for Redis (Single Server)
- Azure Cache for Redis (Flexible Server)
- Azure Cache for Redis (Hyperscale/Citus)

Briefly, these options were discussed in the [Limitations](##Limitations) document.

### Single Server Deployment Options

There are currently three pricing deployment options for the **Single Server** option:

- **Basic**: Workloads requiring light compute and I/O performance.
- **General Purpose**: Most business workloads requiring balanced compute and memory with scalable I/O throughput.
- **Memory-Optimized**: High-performance instance workloads requiring in-memory performance for faster transaction processing and higher concurrency.

The deployment option decision can be influenced by the RTO and RPO requirements of the data workload. When the data workload requires over 4TB of storage, an extra step is required to review and select [a region that supports](https://docs.microsoft.com/en-us/azure/Redis/concepts-pricing-tiers#storage) up to 16TB of storage.

> **Note**  Contact the Redis team ([AskAzureDBforRedis@service.microsoft.com](mailto:AskAzureDBforRedis@service.microsoft.com)) for regions that do not support the workload storage requirements.

Typically, the decision-making will focus on the storage and Input/output Operations Per Second (IOPS) needs.  The target system will always need at least as much storage as in the source system.  Additionally, since IOPS are allocated 3/GB, it is important to match up the IOPS needs to the final storage size.

 | Option | Factors |
 | --- | --- |
 | Basic | Development machine, no need for high performance with less than 1TB storage |
 | General Purpose | Needs for IOPS more than what basic option can provide, but for storage less than 16TB, and less than 4GB of memory |
 | Memory-Optimized | Data workloads that utilize high memory or high cache and buffer related server configuration such as high concurrency|

### Comparison of Services

Which Azure Cache for Redis service should be selected and used?  This table outlines some of the advantages and disadvantages of each along with their Redis version support as of 4/2021.

| Service | Pros | Cons | Versions Supported
| --- | --- |--- |--- |
| Azure VM | Any version, most flexible, full Redis feature support | Customer responsible for updates, security, and administration | Any Version
| Single Server | Autoupgrades, no management | Limited version support, no inbound logical replication support | 9.5 (deprecated), 9.6 (deprecated), 10, and 11

As displayed above, if running Redis 10 or lower and do not plan to upgrade, the workload will need to run an Azure VM or Single Server. If the requirement is to target v13, utilize Flexible Server or Hyperscale Citus.

### Costs

After evaluating the entire WWI Redis data workloads, WWI determined they would need at least 4 vCores and 20GB of memory and at least 100GB of storage space with an IOP capacity of 450 IOPS. Because of the 450 IOPS requirement, they will need to allocate at least 150GB of storage due to [Azure Cache for Redis's IOPS allocation method](https://docs.microsoft.com/en-us/azure/Redis/concepts-pricing-tiers#storage). Additionally, they will require at least 7 days' worth of backups and one read replica.  They do not anticipate an outbound egress of more than 5GB/month. 

WWI intentionally chose to begin its Azure migration journey with a relatively small workload. However, the best practices of instance migration still apply.

To determine these numbers, WWI installed Telegraf with the Redis Input Plugin to interface with the Redis statistics collector. Since the Plugin accesses the `pg_stat_instance` and `pg_stat_bgwriter` tables, which show per-instance and writer process statistics respectively, WWI has also made use of the following query to demonstrate the size of each user table in the `reg_app` schema. Note that the `pg_table_size` function solely includes table size, excluding the size of other associated objects, like indexes.

  ```sql
  SELECT  s.table_name
        ,pg_size_pretty(s.Table_Size)
  FROM 
  (
    SELECT  table_name
          ,pg_table_size('reg_app.' || table_name) AS Table_Size
    FROM information_schema.tables
    WHERE table_schema = 'reg_app'
    ORDER BY Table_Size DESC 
  ) s;
  ```

Using the [Azure Cache for Redis pricing calculator](https://azure.microsoft.com/en-us/pricing/details/Redis/) WWI was able to determine the costs for the Azure Cache for Redis instance. As of 4/2021, the total costs of ownership (TCO) is displayed in the following table for the WWI Conference instance:

 | Resource | Description | Quantity | Cost |
 | --- | --- | --- | --- |
 | Compute (General Purpose) | 4 vcores, 20GB | 1 @ $0.351/hr | $3074.76 / yr
 | Storage | 5GB | 12 x 5 @ $0.115 | $6.90 / yr |
 | Backup | 7 full backups (1x free) | 6 * 5(GB) * .10 | $3.00 / yr
 | Read Replica | 1 second region replica | compute + storage | $3081.66 / yr
 | Network | < 5GB/month egress | Free |
 | Total |  |   | $6166.32 / yr |

After reviewing the initial costs, WWI's CIO confirmed they will be on Azure for a period much longer than 3 years. They decided to use 3-year [reserve instances](https://docs.microsoft.com/en-us/azure/Redis/concept-reserved-pricing) to save an extra ~$4K/yr:

 | Resource | Description | Quantity | Cost |
 | --- | --- | --- | --- |
 | Compute (General Purpose) | 4 vCores | 1 @ $0.1431/hr | 1253.56 / yr |
 | Storage | 5GB | 12 x 5 @ $0.115 | $6.90 / yr |
 | Backup | 7 full backups (1x free) | 6 * 5(GB) * .10 | $3.00 / yr |
 | Network | < 5GB/month egress | Free |
 | Read Replica | 1 second region replica | compute + storage | 1260.46 / yr |
 | Total   |  |  | $2425.8 / yr (~39% savings) |

As the table above shows, backups, network egress, and any read replicas must be considered in the total cost of ownership (TCO). As more instances are added, the storage and network traffic generated would be the only extra cost-based factor to consider.

> **Note:** The estimates above do not include any [ExpressRoute](https://docs.microsoft.com/en-us/azure/expressroute/expressroute-introduction), [Azure App Gateway](https://docs.microsoft.com/en-us/azure/application-gateway/overview), [Azure Load Balancer](https://docs.microsoft.com/en-us/azure/load-balancer/load-balancer-overview), or [App Service](https://docs.microsoft.com/en-us/azure/app-service/overview) costs for the application layers.

> The above pricing can change at any time and will vary based on region.  The region used above was `West US 2`.

### Application Implications

When moving to Azure Cache for Redis, the conversion to secure sockets layer (SSL) based communication is likely to be one of the biggest changes for the applications. SSL is enabled by default in Azure Cache for Redis and it is likely the on-premises application and data workload is not set up to connect to Redis using SSL. When enabled, SSL usage will add some additional processing overhead and should be monitored.

> **Note** Although SSL is enabled by default, it is possible to disable. This is strongly not recommended.

Follow the activities in [Configure TLS connectivity in Azure Cache for Redis - Single Server](https://docs.microsoft.com/en-us/azure/Redis/concepts-ssl-connection-security) to reconfigure the application to support this strong authentication path.

Lastly, modify the server name in the application connection strings or switch the DNS to point to the new Azure Cache for Redis server.

## WWI Use Case

WWI started the assessment by gathering information about their Redis data estate. They were able to compile the following:

 | Name | Source | Size | IOPS | Version | Owner | Downtime |
 | --- | --- | --- | ---- | ---- | ---- | ---- |
 | WwwDB | AWS (PaaS) | 1GB | 150 | 9.5 | Marketing Dept | 1 hr |
 | BlogDB | AWS (Paas) | 1GB | 100| 9.6 | Marketing Dept | 4 hrs |
 | ConferenceDB | On-premises | 5GB | 50 | 9.5 | Sales Dept | 4 hrs |
 | CustomerDB | On-premises |  10GB | 75 | 10.0 | Sales Dept | 2 hrs |
 | SalesDB | On-premises | 20GB | 75 | 10.0 | Sales Dept | 1 hr |
 | DataWarehouse | On-premises | 50GB | 200 | 10.0 | Marketing Dept | 4 hrs
  
Each instance owner was contacted to determine the acceptable downtime period. The planning and migration method selected was based on the acceptable instance downtime.

For the first phase, WWI focused solely on the ConferenceDB instance. The team needed the migration experience to assist in the proceeding data workload migrations. The ConferenceDB instance was selected because of the simple instance structure and the lenient downtime requirements. Once the instance was migrated, the team focused on migrating the application into the secure Azure landing zone.

## Assessment Checklist

- Test the workload runs successfully on the target system.
- Ensure the right networking components are in place for the migration.
- Understand the data workload resource requirements.
- Estimate the total costs.
- Understand the downtime requirements.
- Be prepared to make application changes.
