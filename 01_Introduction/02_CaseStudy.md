# Use Case

## Overview

World Wide Importers (WWI) is a San Francisco, California-based manufacturer and wholesale distributor of novelty goods. They began operations in 2002 and developed an effective business-to-business (B2B) model, selling the items they produce directly to retail customers throughout the United States. Its customers include specialty stores, supermarkets, computing stores, tourist attraction shops, and some individuals. This B2B model enables a streamlined distribution system of their products, allowing them to reduce costs and offer more competitive pricing on the items they manufacture. They also sell to other wholesalers via a network of agents who promote their products on WWI's behalf.

Before launching into new areas, WWI wants to ensure its IT infrastructure can handle the expected growth. WWI currently hosts all its IT infrastructure on-premises at its corporate headquarters and believes moving these resources to the cloud enables future growth. They have tasked their CIO with overseeing the migration of their customer portal and the associated data workloads to the cloud.

WWI would like to continue to take advantage of the many advanced capabilities available in the cloud, and they are interested in migrating their instances and associated workloads into Azure.  They want to do this quickly and without having to make any changes to their applications or instances. Initially, they plan on migrating their java-based customer portal web application and the associated Redis instances and workloads to the cloud.

### Migration Goals

The primary goals for migrating their Redis instances and associated workloads to the cloud include:

- Improve their overall security posture by encrypting data at rest and in-transit.
- Enhance the high availability and disaster recovery (HA/DR) capabilities.
- Position the organization to leverage cloud-native capabilities and technologies such as point in time restore.
- Take advantage of administrative and performance optimizations features of Azure Cache for Redis.
- Create a scalable platform that they can use to expand their business into more geographic regions.
- Allow for enhanced compliance with various legal requirements where PII information is stored.

WWI used the [Cloud Adoption Framework (CAF)](https://docs.microsoft.com/azure/cloud-adoption-framework/) to educate their team on following best practices guidelines for cloud migration. Using CAF as a higher-level migration guide, WWI customized their migration into three main stages. Within each stage, they defined activities that needed to be addressed to ensure a successful lift and shift cloud migration.

These stages include:

 | Stage | Name | Activities |
 | --- | --- | --- |
 | 1 | Pre-migration | Assessment, Planning, Migration Method Evaluation, Application Implications, Test Plans, Performance Baselines |
 | 2 | Migration | Execute Migration, Execute Test Plans |
 | 3 | Post-migration | Business Continuity, Disaster Recovery, Management, Security, Performance Optimization, Platform modernization |

WWI has several instances of Redis running with varying versions ranging from 9.5 to 11.  They would like to move their older instances to the latest Redis version as soon as possible, but there are some concerns regarding applications functioning without issues. A decision has been made to migrate to the cloud first and upgrade the Redis version later knowing that Redis 9.5 and 9.6 are coming to end of support.

They would also like to ensure that their data workloads are safe and available across multiple geographic regions in case of failure and are looking at the available configuration options.

WWI wants to start off with a simple application for the first migration, and then move to more business-critical applications in a later phase. This will provide the team with the knowledge and experience they need to prepare and plan for those future migrations.
