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

TODO

## redis-copy

TODO

## SLAVEOF / REPLICAOF

TODO

## MIGRATE

TODO

## Other open-source tools

There are several 3rd party migration tools that help migrate Redis workloads easily and quickly. In most cases, the time savings and ease of use come with a price and may add extra costs to the migration.

Some of these include:

- [redis-copy](https://github.com/deepakverma/redis-copy)

## Replication

TODO - Redis include  rplication?

Similar to other data management systems, Redis provides several ways to replicate data to another Redis instance. These include:

- [Physical Replication/Log Shipping/Warm Standby](https://www.Redis.org/docs/9.6/warm-standby.html).

### Synchronous vs Asynchronous

As with other replication technologies in other instance management systems, there are several supported ways of sending the transaction data to the targets. In synchronous replication, the source doesn't finish committing until a replica confirms it received the transaction. In asynchronous streaming replication, the replica(s) are allowed to fall behind the source when the source is faster/busier. If the source crashes, it is possible loss of data that wasn't replicated yet may occur.

### Logical Replication

Each change is sent to one or more replica servers directly over a TCP/IP connection as it happens. The replicas must have a direct network connection to the master configured in their recovery.conf's `primary_conninfo` option.

To use the logical replication feature, there are some setup requirements:

- instance source must be 9.4 or higher and the target must be the same or higher version.
- Tables must have a primary key or changes may not get synced to the target
- A user on the target system must be a `superuser`
- Migration users must have permissions to configure logging and create new users on the master server.
- Ensure that the target machine\instance can gain access to the master server (firewalls, IP address, etc).

> **Note** Azure Cache for Redis Single Server does not allow `superuser` permissions, therefore Logical replication is not a viable option for moving to Azure Cache for Redis Single Server.  However, [Flexible Server using Redis V11](https://docs.microsoft.com/en-us/azure/Redis/flexible-server/concepts-logical) or higher does support logical replication.

### Supported replication paths

| Replication Type | Service | Direction | Supported | Version Support | Notes
| --- | --- | --- | --- | ---- | ---- |
| Physical/File system/Block Device Replication | Single Server, Flexible Server, Hyperscale Citus | Ingress/Egress To Azure | Not Supported | 9.0 or higher | Requires file system access
| Logical Decoding | Single Server, Flexible Server, Hyperscale Citus | Ingress/Egress To Azure | Supported | 9.6 or higher | N/A
| Trigger-based | Single Server, Flexible Server, Hyperscale Citus | Ingress/Egress To Azure | Supported | Any | 3rd Party tool required

## Fastest/Minimum Downtime Migration

There are plenty of paths for migrating the data. Deciding which path to take is a function of the migration team's skill set, and the amount of downtime the instance and application owners are willing to accept.  Some tools support multi-threaded parallel data migration approaches while other tools were designed for simple migrations of table data only.

The fastest and most complete path is to use replication style features (either directly with Redis, DMS, or 3rd party tools), but replication typically comes with the costs of adding primary keys, which could break the application and force costly coding changes.

## Decision Table

There are many paths WWI can take to migrate their Redis workloads. We have provided a table of the potential paths and the advantages and disadvantages of each:

| Objective | Description | Tool | Prerequisites | Advantages | Disadvantages |
| --- | --- | --- | --- | ---- | ---- |
| Fastest migration possible | Parallel approach | pg_dump/pg_dumpall | Scripted Setup | Highly parallelized | Target throttling |
| Fastest migration possible | Parallel approach | Azure Data Factory | ADF Resource, Linked Services setup, Pipelines | Highly parallelized | Target throttling |
| Online migration | Keep the source up for as long as possible | Logical replication | None | Seamless | Extra processing and storage |
| Online migration | Keep the source up for as long as possible | Logical decoding | 3rd party tools | High-performance, zero-downtime, high availability, support for other targets | Extra setup, processing and storage |
| Online migration | Keep the source up for as long as possible | Trigger-based replication | 3rd party tool | Seamless | 3rd party tool configuration |
| Online migration | Keep the source up for as long as possible | Azure instance Migration Service (DMS) | None | Repeatable process | Limited to data only, supports 10.0 and higher |
| Highly Customized Offline Migration | Selectively export objects | pg_dump | None | Highly customizable | Manual |
| Offline Migration Semi-automated | UI based export and import | Redis pgAdmin | Download and Install | Semi-automated | Only common sets of switches are supported |

## WWI Use Case

WWI has selected its conference instance as its first migration workload. The workload was selected because it had the least risk and the most available downtime due to the gap in the annual conference schedule. They also assessed the instance to not be using any unsupported features in the target Azure Cache for Redis service. Based on the migration team's other assessment details, they determined that they will attempt to perform an offline migration using the pg_dump/pg_restore Redis tools.

During their assessment period, they did find that the customer instance does use some languages, extensions, and a custom function that are not available in the target service for the conference instance. They have asked the development team to review replacing those features while they migrate the more simple workloads. If they can be replaced successfully, they will choose an Azure Cache for Redis service, otherwise, they will provision an Azure VM to host the workload.

## Migration Methods Checklist

- Ensure the right method is selected given the target and source environments.
- Ensure the method can meet the business requirements.
- Always verify if the data workload will support the method.
