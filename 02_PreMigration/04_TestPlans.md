# Test Plans

## Overview

WWI created a test plan that included a set of IT and Business tasks. Successful migrations require all the tests to be executed.

Tests:

- Ensure the migrated instance has consistency (same record counts) with on-premises instance.
- Ensure the performance is acceptable (it should match the same performance as if it were running on-premises).
- Ensure acceptable network connectivity between on-premises and the Azure network.
- Ensure all identified applications and users can connect to the migrated data instance.

WWI has identified a migration weekend and time window that started at 10 pm and ended at 2 am Pacific Time. If the migration did not complete before the 2 am target (the 4hr downtime target) with all tests passing, the rollback procedures were started. Issues were documented for future migration attempts. All migrations windows were pushed forward and rescheduled based on acceptable business timelines.

## Sample Queries

A series of queries were executed on the source and target to verify migration success. The following queries and scripts will help determine if the migration moved all required instance objects from the source to the target.

### Exporting Objects

Use this query to get all the keys:

```bash
KEYS *
```

> **NOTE** Running this command on a production environment can cause performance issues with any applications using the target instance.  It is advisable to execute during low traffic/usage periods.

Use this query to get all the users:

```bash
ACL LIST
```

TODO : https://redis.io/topics/acl

### Commands

TODO

Command renames (for app level)

### Scripts

If you have any scripts or your application makes calls to the [`EVAL` command](https://redis.io/commands/eval), be sure to test them thoroughly in case your scripts use commands or features that are not available in Azure Cache for Redis.

## Rollback Strategies

The queries above will provide a list of object names and counts that can be used in a rollback decision.  Migration users can take the first object verification step by checking the source and target object counts. A failure in counts may not necessarily mean that a rollback is needed. Performing an in-depth evaluation could point out that the discrepancy is small and easily recoverable.  Manual migration of a few failed objects may be possible.  

For example, if all keys and values were migrated, but only a few users were missed, remediate those failed items and finalize the migration. If the instance is relatively small, it could be possible to clear the Azure Cache for Redis and restart the migration again. However, if the instance is relatively large, there may not be enough time to determine what occurred. The migration will need to stop and rollback.  

Identifying missing instance objects needs to occur quickly during a migration window.  Every minute counts.  One option could be exporting the environment object names to a file and using a data comparison tool to reduce the missing object verification time.  Another option could be exporting the source instance object names and importing the data into a target instance environment temp table.  Compare the data using a **scripted** and **tested** query statement.  Data verification speed and accuracy are critical to the migration process.  Do not rely on reading and verifying a long list of instance objects during a migration window.  Manage by exception.

### Decision Table

| Discrepancy | Time To Sync | Rollback? | Resolution Path |
| --- | --- | --- | --- |
| Key Count Mismatch | Less than the remaining window | No | Sync the missing keys
| Key Value Mismatch | More than the remaining window | Yes | Start the Rollback

In the [migration](./../03_Migration/01_DataMigration.md) section, we will provide a instance migration inventory script that will provide object counts that can be used to compare source and destination after a migration path has been completed.

## WWI Use Case

The WWI CIO received a confirmation report that all instance objects were migrated from the on-premises instance to the Azure Cache for Redis instance.  The infrastructure and dev teams ran the above queries against the instance before the beginning of the migration and saved all the results to a spreadsheet for tracking.

The source instance information was used to verify the target migration object fidelity.

## Checklist

- Have test queries scripted, tested, and ready to execute.
- Know how long test queries take to run and make them a part of the migration timeline.
- Have a mitigation and rollback strategy ready for different potential outcomes.
- Have a well-defined timeline of events for the migration.
