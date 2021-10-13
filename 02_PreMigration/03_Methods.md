# Migration Methods

Getting the data from the source to target will require using tools or features of Redis to accomplish the migration.

It is important to complete the entire assessment and planning stages before starting the next stages.  The decisions and data collected are migration path and tool selection dependencies.

We explore the following commonly used tools in this section:

- Database export/import via RDB file
- Append Only File (AOF)
- Layer of abstraction (Dual write)
- SLAVEOF / REPLICAOF commands
- MIGRATE command
- 3rd Party tools

## Redis Persistence

Redis is a memory server, designed for high-performance storage and retrieval.  If the server or service where to be shutdown, all the items in the cache would be lost.  To ensure durability, you can [select a persistence mode](https://redis.io/topics/persistence) to keep the values in the case of failure. These two persistence methods include RDB and AOF.

You can also select the [persistence in Azure Redis instances](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-premium-persistence).

## RDB File

When enabled, Redis can keep cache data persisted to disk on a regular basis.  When enabled this can lead to some performance degradation so testing should be done to ensure your cache responds to your clients appropriately after enabling the feature. When not enabled, in the event of a server failure or reboot any data in memory will be lost. Read more about the RDP file persistence [advantages and disadvantages](https://redis.io/topics/persistence).

## Append Only File (AOF)

The append-only file is an alternative to RDB and is a fully-durable strategy for Redis. It first became available in version 1.1. AOF can be enabled in the Redis configuration file:

```text
appendonly yes
```

Once enabled, every time Redis receives a command that changes the dataset (e.g. SET) it will append it to the AOF. When you restart Redis it will re-play the AOF to rebuild the state.  This same file can be used to rebuild / migrate a Redis instance in Azure, however, the replay will need to be done via a tool such as `redis-cli` as you cannot use automatic import of the AOF file like an RDB file via Azure Storage.

This option is more durable than the RDB file, but comes at some costs in larger files, repeated commands and slower performance when under huge write loads.

## Manual (SET)

The most basic way to migrate an instance is to enumerate all the keys from the source and then `SET` the values in the destination.  This works well with basic key values such as strings and integers, but care must be taken with more complex objects such that the tool encodes the values correctly in the migrate process.

## Manual (DUMP/RESTORE)

This path is the preferred path as it will export the key in the Redis encoded format. Although it is the preferred method, it presents various challenges when the source and target are not within a compatible version range for the encoding algorithm.

## SLAVEOF / REPLICAOF

Redis includes the ability to create replicas of master nodes.  This path is one of the easiest to setup, but unfortunately none of the Azure services supports the `SLAVEOF` or `REPLICAOF` commands.  This means this path is best used for when you are moving from one version to another to support a move to the cloud using the `DUMP` and `RESTORE` path.

## MIGRATE

The [`MIGRATE`](https://redis.io/commands/migrate) Redis command will atomically transfer a key from a source Redis instance to a destination Redis instance. On success the key is deleted from the original instance and is guaranteed to exist in the target instance.

## Layer of Abstraction (Dual write)

Layer of abstraction means that you can use your applications to migrate your Redis data in real-time and as the data is used.  Once you hit 100% key coverage, you can then remove the layer of abstraction and retire the old Redis instances.

## Other open-source tools

There are several 3rd party migration tools that help migrate Redis workloads easily and quickly. In most cases, the time savings and ease of use come with a price and may add extra costs to the migration.

Some of these include:

- [redis-copy](https://github.com/deepakverma/redis-copy)
- [redis-migrate](https://github.com/vipshop/redis-migrate-tool)

## Fastest/Minimum Downtime Migration

As outlined above, there are plenty of paths for migrating cache data. Deciding which path to take is a function of the migration team's skill set, and the amount of downtime the instance and application owners are willing to accept.  Some tools support multi-threaded parallel data migration approaches while other tools were designed for simple migrations of key/value data only.

## Decision Table

There are many paths WWI can take to migrate their Redis workloads. We have provided a table of the potential paths and the advantages and disadvantages of each:

| **Objective** | **Description** | **Tool** | **Prerequisites** | **Advantages** | **Disadvantages** |
| --- | --- | --- | --- | ---- | ---- |
| Fastest migration possible | Parallel approach | 3rd party tool | Scripted Setup | Highly parallelized | Target throttling |
| Online migration | Keep the source up for as long as possible | Replication | None | Seamless | Extra processing and storage |
| Highly Customized Offline Migration | Selectively export objects | IMPORT/EXPORT | None | Highly customizable | Manual |

In terms of the more specific tools and methods, here is a table of supported features with advantages and disadvantages of each:

| **Path\Tool** | **Supported in Azure** | **Multiple Databases** | **Clusters** | **Underlying Cmds** | **Advantages** | **Disadvantages**
| --- | --- | --- | --- | --- | --- | --- |
| RDP Backup/Restore | Premium+ | Yes | Yes | N/A | Simple file copy | Requires storage account, Complicated for clusters
| AOF Backup/Restore | Premium+ | Yes | Yes | N/A | Replay of AOF file via redis-cli | Requires storage account, Complicated for clusters, Not all commands may execute
| Cluster Failover (Migration) | No | - | - | - | - | -
| Replication (SLAVEOF/REPLICAOF) | No | - | - | - | - | -
| Mass Insertion (SET/GET) | Yes | Yes | Yes | SET/GET | Simple to implement | Binary encoding complexities (output and input formatting)
| Mass Insertion (DUMP/RESTORE) | Yes | Yes | Yes | DUMP/RESTORE | Simple to implement | Binary encoding complexities (versions must match)
| Mass Insertion (MIGRATE) | Yes | Yes | Yes | MIGRATE | Simple to implement | Binary encoding complexities (versions must match), Data is deleted from source during migration.
| Layer of Abstraction | Yes | Yes | Yes | SET/GET | Simple to implement | Requires application changes, does not support local SET/GET (outside of application)

## WWI Use Case

WWI has selected its conference instance as its first migration workload. The workload was selected because it had the least risk and the most available downtime due to the gap in the annual conference schedule. They also assessed the instance to not be using any unsupported features in the target Azure Cache for Redis service. Based on the migration team's other assessment details, they determined that they would attempt to perform an offline migration using the backup and restore Redis tools.

During their assessment period, they did find that the customer instance does use some languages, extensions, and a custom function that are not available in the target service for the conference instance. They have asked the development team to review replacing those features while they migrate the simpler workloads. If they can be replaced successfully, they will choose an Azure Cache for Redis service, otherwise, they will provision an Azure VM to host the workload.

## Migration Methods Checklist

- Ensure the right method is selected given the target and source environments.
- Ensure the method can meet the business requirements.
- Always verify if the data workload will support the method.
