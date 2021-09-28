# Security

Moving to a cloud-based service doesn't mean the entire internet will have access to it at all times. Azure provides best in class security that ensures data workloads are continually protected from bad actors and rouge programs.

## Encryption

Redis does not directly support any form of data encryption, so all encoding must be performed by client applications.

Additionally, Redis OSS does not provide any form of transport security unless a supported version is specifically compiled to support it. When running a default non-SSL enabled instance, if you need to protect data as it flows across the network, it is recommended to implement an SSL proxy.

Azure Cache for Redis supported SSL/TLS encryption and is enabled by default.  As covered in the migration assessment, your application may need to be modified to support SSL connectivity as it is not recommended to enable the non-SSL port in Azure Cache for Redis.

## Authentication

Redis is focused purely on providing fast access to data and is designed to run inside a trusted environment that can be accessed only by trusted clients. Redis supports [a limited security model](https://redis.io/topics/security) based on password authentication. (It is possible to remove authentication completely, although we don't recommend this.)

All authenticated clients share the same global password and have access to the same resources. If you need more comprehensive sign-in security, you must implement your own security layer in front of the Redis server, and all client requests should pass through this additional layer. Redis should not be directly exposed to untrusted or unauthenticated clients.

Azure Cache for Redis supports the basic authentication mechanisms for Redis user connectivity with two access keys per instance.  Azure Cache for Redis does not support Azure Active Directory integration.

## TLS Settings

By default, Azure Cache for Redis disables the non-SSL port 6379 and uses the SSL port of 6380. You can re-enable the non-SSL port if needed, but it is recommended that you upgrade your applications to support SSL rather than open non-SSL access to Azure Cache for Redis.  Additionally, you will want to ensure your applications will support TLS 1.2 rather than the older 1.0 or 1.1.

## Firewall

Azure provides a Firewall layer to limit access to only know IP address spaces.  This is a best practice to ensure that only allowed application can connect to your Azure Cache for Redis instances. The migration team should review the network data flows and configure the Firewall accordingly.  Azure Cache for Redis provides several mechanisms to secure the networking layers by limiting access to only authorized users, applications and devices.  

The first line of defense for protecting the Redis instance is to implement firewall rules. IP addresses can be limited to only valid locations when accessing the instance via internal or external IPs. If the Redis instance is destined to only serve internal applications, then restrict public access.

When moving an application to Azure along with the Redis workload, it is likely there will be multiple virtual networks setup in a hub and spoke pattern that will require [Virtual Network Peering](https://docs.microsoft.com/en-us/azure/virtual-network/virtual-network-peering-overview) to be configured.

## Networking

There are a couple of [network isolation options](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-network-isolation) you can choose from in Azure, each one has some advantages and limitations.

### Private Endpoint (Recommended)

To limit access to the Azure Cache for Redis to internal Azure resources, enable [Private Endpoint](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-network-isolation#azure-private-link).  Private Endpoints will ensure that the Redis instance will be assigned a private IP rather than a public IP address.

> **Note** Firewall rules can be used with VNet injected caches, but not private endpoints currently.

### Virtual Network Integration

If you do not want any public access to your instance, you can enable [Virtual Network integration](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-network-isolation#azure-virtual-network-injection) such that only VNet resources can access your data.

## Security baseline

Review a set of potential [security baseline](https://docs.microsoft.com/en-us/security/benchmark/azure/baselines/azure-cache-for-redis-security-baseline?toc=/azure/azure-cache-for-redis/TOC.json) tasks that can be implemented across all Azure resources. Not all of the items described on the reference link will apply to the specific data workloads or Azure resources.

## Security Checklist

- Upgrade applications to use SSL and TLS 1.2
- Enable all auditing features.
- Implement firewall rules.
- Utilize private endpoints for workloads that do not travel over the Internet.
- Ensure you read the security baseline article
