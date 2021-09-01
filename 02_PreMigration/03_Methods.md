# Migration Methods

Getting the data from the source to target will require using tools or features of Redis to accomplish the migration.

It is important to complete the entire assessment and planning stages before starting the next stages.  The decisions and data collected are migration path and tool selection dependencies.

We explore the following commonly used tools in this section:

- Database export/import via RDB file
- [redis-copy](https://github.com/deepakverma/redis-copy)
- Layer of abstraction
- SLAVEOF / REPLICAOF commands
- MIGRATE command

## Import / Export

### RDB File

By default, Redis will keep cache data persisted to disk on a fairly regular basis, this can however be disabled by the administrator to improve performance. However, doing so would cause any data in memory to be lost in the case of a server fault or reboot.  In most cases this is enabled.

## Manual (SET, DUMP/RESTORE)

TODO

## SLAVEOF / REPLICAOF

Redis includes the ability to create replicas of master nodes.  You can add an Azure Redis instance as a Replica of a source instance and then retire the old master server.

## MIGRATE

TODO

## Other open-source tools

There are several 3rd party migration tools that help migrate Redis workloads easily and quickly. In most cases, the time savings and ease of use come with a price and may add extra costs to the migration.

Some of these include:

- [redis-copy](https://github.com/deepakverma/redis-copy)
- [redis-migrat](https://github.com/vipshop/redis-migrate-tool)

## Replication

Similar to other data management systems, Redis provides several ways to replicate data to another Redis instance. These include:

- [TODO](TODO).

### Synchronous vs Asynchronous

As with other replication technologies in other instance management systems, there are several supported ways of sending the transaction data to the targets. In synchronous replication, the source doesn't finish committing until a replica confirms it received the transaction. In asynchronous streaming replication, the replica(s) are allowed to fall behind the source when the source is faster/busier. If the source crashes, it is possible loss of data that wasn't replicated yet may occur.

### Replication Process

Each change is sent to one or more replica servers directly over a TCP/IP connection as it happens. The replicas must have a direct network connection to the master.

To use replication feature, there are some setup requirements:

- instance source must be 9.4 or higher and the target must be the same or higher version.
- TODO

### Supported replication paths

| Replication Type | Service | Direction | Supported | Version Support | Notes
| --- | --- | --- | --- | ---- | ---- |
| Replication | All versions | Ingress/Egress To Azure | Not Supported | 9.0 or higher | Requires file system access

## Fastest/Minimum Downtime Migration

There are plenty of paths for migrating the data. Deciding which path to take is a function of the migration team's skill set, and the amount of downtime the instance and application owners are willing to accept.  Some tools support multi-threaded parallel data migration approaches while other tools were designed for simple migrations of key/value data only.

The fastest and most complete path is to use replication style features (either directly with Redis or 3rd party tools), but replication typically comes with the costs of extra setup steps and various configuration changes.

## Decision Table

There are many paths WWI can take to migrate their Redis workloads. We have provided a table of the potential paths and the advantages and disadvantages of each:

| Objective | Description | Tool | Prerequisites | Advantages | Disadvantages |
| --- | --- | --- | --- | ---- | ---- |
| Fastest migration possible | Parallel approach | 3rd party tool | Scripted Setup | Highly parallelized | Target throttling |
| Online migration | Keep the source up for as long as possible | Replication | None | Seamless | Extra processing and storage |
| Highly Customized Offline Migration | Selectively export objects | IMPORT/EXPORT | None | Highly customizable | Manual |

## WWI Use Case

WWI has selected its conference instance as its first migration workload. The workload was selected because it had the least risk and the most available downtime due to the gap in the annual conference schedule. They also assessed the instance to not be using any unsupported features in the target Azure Cache for Redis service. Based on the migration team's other assessment details, they determined that they will attempt to perform an offline migration using the backup and restore Redis tools.

During their assessment period, they did find that the customer instance does use some languages, extensions, and a custom function that are not available in the target service for the conference instance. They have asked the development team to review replacing those features while they migrate the more simple workloads. If they can be replaced successfully, they will choose an Azure Cache for Redis service, otherwise, they will provision an Azure VM to host the workload.

## Migration Methods Checklist

- Ensure the right method is selected given the target and source environments.
- Ensure the method can meet the business requirements.
- Always verify if the data workload will support the method.
