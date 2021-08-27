# Data Migration

## Back up the instance

Lower the risk and back up the instance before upgrading or migrating data. Use the Redis pgAdmin UI or the `pg_dump` command to export the instance for restore capability.

## Offline vs. Online

Before selecting a migration tool, decide if the migration should be online or offline.

- **Offline migrations** require the system to be down while the migration takes place. Users will not be able to modify data. This option ensures that the state of the data will be exactly what is expected when restored in Azure.
- **Online migrations** will migrate the data in near real-time. This option is appropriate when there is little downtime for the users or application consuming the data workload. The costs are too high for the corporation to wait for complete migration. The process involves replicating the data using a replication method such as logical replication or similar functionality.

> **Case Study:**
In the case of WWI, their environment has some complex networking and security requirements that will not allow for the appropriate changes to be applied for inbound and outbound connectivity in the target migration time frame. These complexities and requirements essentially eliminate the online approach from consideration.

> **Note:** Review the Planning and Assessment sections for more details on Offline vs Online migration.

## Data Drift

Offline migration strategies have the potential for data drift. Data drift occurs when newly modified source data becomes out of sync with migrated data. When this happens, a full export or a delta export is necessary. To mitigate this problem, stop all traffic to the instance and then perform the export. If stopping all data modification traffic is not possible, it will be necessary to account for the data drift.

Determining the changes can become complicated if the instance tables don't have columns such as numeric primary keys, or some type of modification and creation date in every table that needs to be migrated.

For example, if a numeric primary key is present and the migration is importing in sort order, it will be relatively simple to determine where the import stopped and restart it from that position. If no numeric key is present, then  utilize modification and creation date, and again, import in a sorted manner to restart the migration from the last timestamp seen in the target.

## Performance recommendations

### Connection Pooling

In Redis, establishing a instance connection is an expensive operation because each new connection to the Redis instance requires forking the OS process and allocating memory for the connection. As a result, transactional applications that frequently open and close connections at the end of transactions can experience higher connection latency, resulting in lower instance throughput (transactions per second) and overall higher application latency.

Migration strategies requiring many parallel threads should utilize connection pooling tools such as [PgBouncer](https://www.pgbouncer.org/) or [Pgpool](https://www.pgpool.net/mediawiki/index.php/Main_Page).

These tools enable faster data migration because there no need to create and tear down instance connections.

PgBouncer can improve the data migration throughput by almost 4x times and instance connectivity latency by a factor of 40.

### Source Tool Network

When running the migration tool on a virtual machine, it is possible to change the TCP_NODELAY setting. By default, TCP uses Nagle's algorithm, which optimizes by batching up outgoing packets. This means fewer sends and this works well if the application sends packets frequently and latency is not the highest priority. Realize latency improvements by sending on sockets created with the TCP_NODELAY option enabled. This results in lower latency but more sends. Consider this client-side setting for the Virtual Machine (VM). Applications that benefit from the TCP_NODELAY option typically tend to do smaller, infrequent writes and are particularly sensitive to latency. As an example, alter this setting to reduce latency from 15-40 ms to 2-3 ms.

To change this setting on Windows machines, do the following:

- Open the `REGEDIT` tool
- Under the subtree HKEY_LOCAL_MACHINE, find the `SYSTEM\CurrentControlSet\services\Tcpip\Parameters\Interfaces` key
- Find the correct network interface
- In the space, right-click and select **New** for creating a DWORD Value
- For the value name, type **TcpNoDelay**
- For the Dword value, type **1**
  - In the empty space right-click, and select **New** for creating a DWORD Value
- For the value name, type **TcpAckFrequency**
- For the Dword value, type **1**
- Close the REGEDIT tool

### Exporting

- Upgrade the data and log disks if experiencing poor export performance.
- Use an export tool that leverages multiple threads
- When using Redis 9.6 or higher, use [partitioned tables](https://www.Redis.org/docs/10/ddl-partitioning.html) when appropriate to increase the speed of exports.
- When [logging is enabled](https://www.Redis.org/docs/9.1/runtime-config-logging.html), move the log files directory to a separate drive than the data drive.

### Importing

- Create [clustered indexes](https://www.Redis.org/docs/9.1/sql-cluster.html) and primary keys after loading data.
- Load data in primary key order or according to some date column (such as modification date or creation date)
- Delay the creation of secondary indexes until after data is loaded. Create all secondary indexes after loading.
- Disable foreign key constraints before loading. Disabling foreign key checks provides significant performance gains. Enable the constraints and verify the data after the load to ensure referential integrity.
- Load data in parallel.
  > Caution: Avoid too much parallelism that could cause resource contention and monitor resources by using the metrics available in the Azure portal.
- Use multi-valued INSERT commands to decrease the overhead of many single inserts.
- [Modify the WAL log file location](https://www.Redis.org/docs/current/wal-internals.html) to put the WAL log files on a separate drive to gain extra performance
- Set `shared_buffers` to at least 25% of available memory.
- Increase the `wal_buffers` server parameter to a higher value when performing parallel loading that will initiate many connections.

> **Note** The Redis wiki has several [articles focused on performance](https://wiki.Redis.org/wiki/Performance_Optimization).

### Post Import

- Run the `ANALYZE` command to update all statistics.

## Performing the Migration

- Back up the instance
- Create and verify the Azure Landing zone
- Export and configure Source Server configuration
- Export and configure Target Server configuration
- Export the instance objects (schema, users, etc.)
- Export the data
- Import the instance objects
- Import the data (no triggers, keys)
- Validation
- Migrate the Application(s)

## Common Steps

Despite what path is taken, there are common steps in the process:

- Upgrade to a supported Azure Redis version
- Inventory instance objects
- Export users and permissions

## Migrate to the latest Redis version

The WWI Conference instance, which currently runs 9.5, will be upgraded to Redis 11.0, the latest version supported by Azure Cache for Redis.

There are two options for upgrading to 11.0:

- In-place with `pg_upgrade`
- Export/Import with `pg_dumpall`

## WWI Use Case

After successfully migrating the Redis instance to 9.6, the WWI migration team realized the original [instance Migration Service (DMS)](https://datamigration.microsoft.com/scenario/Redis-to-azureRedis?step=1) migration is unavailable to them, as the DMS tool currently only supports 10.0 and higher.  DMS also required network access, and the WWI migration team was not ready to handle their complex network issues.  These environmental issues narrowed their migration tool choice to Redis pgAdmin.

## instance Objects

As outlined in the [Test Plans](../02_PreMigration/04_TestPlans.md) section, take an inventory of instance objects before and after the migration.  

Migration teams should develop and test helpful inventory SQL scripts before beginning the migration phase.

instance object inventory SQL examples:

```sql

TODO
```

## Execute migration

With the basic migration components in place, it is now possible to proceed with the data migration. WWI will utilize the Redis pgAdmin option to export the data and then import it into Azure Cache for Redis.  

Options:

- [Backup and Restore](./01.01_DataMigration_BackupRestore.md)
- [Copy command](./01.02_DataMigration_Copy.md)
- [Azure Data Factory (ADF)](./01.03_DataMigration_ADF.md)
- [Logical Replication](./01.05_DataMigration_LogicalReplication.md)
- [Logical Decoding](./01.06_DataMigration_LogicalDecoding.md)

Once the data is migrated, point the application to the new instance

- [Migrate Application Settings](./02_DataMigration_AppSettings.md)

Lastly, validate the target instance's inventory. Below is an example of the SQL results in a target environment. It is easy to identify object count discrepancies.

> **Note:** Unsupported objects will be dropped during the migrations.

  ![](media/00_Completed_DB_Count.PNG)

## Data Migration Checklist

- Understand the complexity of the environment and determine if an online approach is feasible.
- Account for data drift. Stopping the instance service can eliminate potential data drift. Acceptable downtime costs?
- Configure source parameters for fast export.
- Configure target parameters for fast import.
- Test any migrations that have a different source version vs the target.
- Migrate any miscellaneous objects, such as user names and privileges.
- Update application settings to point to the new instance.
- Document all tasks. Update task completion to indicate progress.
