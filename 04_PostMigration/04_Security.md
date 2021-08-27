# Security

Moving to a cloud-based service doesn't mean the entire internet will have access to it at all times. Azure provides best in class security that ensures data workloads are continually protected from bad actors and rouge programs.

## Authentication

Azure Cache for Redis supports the basic authentication mechanisms for Redis user connectivity, but also supports [integration with Azure Active Directory](https://docs.microsoft.com/en-us/azure/Redis/concepts-aad-authentication). This security integration works by issuing tokens that act like passwords during the Redis login process.  [Configuring Active Directory integration](https://docs.microsoft.com/en-us/azure/Redis/howto-configure-sign-in-azure-ad-authentication) is incredibly simple to do and supports not only users, but AAD groups as well.

This tight integration allows administrators and applications to take advantage of the enhanced security features of [Azure Identity Protection](https://docs.microsoft.com/en-us/azure/active-directory/identity-protection/overview-identity-protection) to further surface any identity issues.

> **Note:** Be sure to test the application with Azure AD Authentication. See [Use Azure Active Directory for authentication with Redis](https://docs.microsoft.com/en-us/azure/Redis/howto-configure-sign-in-aad-authentication) for more information.

## Threat Protection

In the event that user or application credentials are compromised, logs are not likely to reflect any failed login attempts.  Compromised credentials can allow bad actors to access and download the data. [Azure Threat Protection](https://docs.microsoft.com/en-us/azure/Redis/howto-instance-threat-protection-portal) can watch for anomalies in logins (such as unusual locations, rare users or brute force attacks) and other suspicious activities.  Administrators can be notified in the event something does not `look` right.

## Audit Logging

Redis has a robust built-in logging feature. By default, this [log feature is disabled](https://docs.microsoft.com/en-us/azure/Redis/concepts-server-logs) in Azure Cache for Redis.  Server level logging can be enabled or modified by changing various server parameters. Once enabled, logs can be accessed through [Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/overview) and [Log Analytics](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/design-logs-deployment) by turning on [diagnostic logging](https://docs.microsoft.com/en-us/azure/Redis/howto-configure-audit-logs-portal#set-up-diagnostic-logs).

To query for log related events, run the following KQL query:

```kql
AzureDiagnostics
| where LogicalServerName_s == "myservername"
| where Category == "RedisLogs"
| where TimeGenerated > ago(1d) 
```

In addition to the basic logging feature, gain access to more in-depth [audit logging information](https://docs.microsoft.com/en-us/azure/Redis/concepts-audit) that is provided with the `pgaudit` extension.

```kql
AzureDiagnostics
| where LogicalServerName_s == "myservername"
| where TimeGenerated > ago(1d) 
| where Message contains "AUDIT:"
```

## Encryption

Data in the Redis instance is encrypted at rest by default. Any automated backups are also encrypted to prevent potential leakage of data to unauthorized parties. This encryption is typically performed with a key that is created when the instance is created. In addition to this default encryption key, administrators have the option to [bring your own key (BYOK)](https://docs.microsoft.com/en-us/azure/Redis/concepts-data-encryption-Redis).

When using a customer-managed key strategy, it is vital to understand responsibilities around key lifecycle management. Customer keys are stored in an [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/general/basic-concepts) and then accessed via policies. It is vital to follow all recommendations for key management as the loss of the encryption key equates to the loss of data access.

In addition to a customer-managed keys, use service-level keys to [add double encryption](https://docs.microsoft.com/en-us/azure/Redis/concepts-infrastructure-double-encryption).  Implementing this feature will provide highly encrypted data at rest, but it does come with encryption performance penalties. Testing should be performed.

Data can be encrypted during transit using SSL/TLS. As previously discussed, it may be necessary to [modify your applications](https://docs.microsoft.com/en-us/azure/Redis/concepts-ssl-connection-security) to support this change and also configure the appropriate TLS validation settings.

## Firewall

Once users are set up and the data is encrypted at rest, the migration team should review the network data flows.  Azure Cache for Redis provides several mechanisms to secure the networking layers by limiting access to only authorized users, applications and devices.  

The first line of defense for protecting the Redis instance is to implement [firewall rules](https://docs.microsoft.com/en-us/azure/Redis/concepts-firewall-rules). IP addresses can be limited to only valid locations when accessing the instance via internal or external IPs. If the Redis instance is destined to only serve internal applications, then [restrict public access](https://docs.microsoft.com/en-us/azure/Redis/concepts-data-access-and-security-private-link#deny-public-access-for-azure-instance-for-Redis-single-server).

When moving an application to Azure along with the Redis workload, it is likely there will be multiple virtual networks setup in a hub and spoke pattern that will require [Virtual Network Peering](https://docs.microsoft.com/en-us/azure/virtual-network/virtual-network-peering-overview) to be configured.

## Private Link

To limit access to the Azure Cache for Redis to internal Azure resources, enable [Private Link](https://docs.microsoft.com/en-us/azure/Redis/concepts-data-access-and-security-private-link).  Private Link will ensure that the Redis instance will be assigned a private IP rather than a public IP address.

> **Note** Not all Azure Cache for Redis services support private link (4/2021).

> **Note** There are many other [basic Azure Networking considerations](https://docs.microsoft.com/en-us/azure/Redis/concepts-data-access-and-security-vnet) that must be taken into account that are not the focus of this guide.

## Security baseline

Review a set of potential [security baseline](https://docs.microsoft.com/en-us/azure/Redis/security-baseline) tasks that can be implemented across all Azure resources. Not all of the items described on the reference link will apply to the specific data workloads or Azure resources.

## Security Checklist

- Use Azure AD authentication where possible.
- Enable Advanced Thread Protection.
- Enable all auditing features.
- Consider a Bring-Your-Own-Key (BYOK) strategy.
- Implement firewall rules.
- Utilize private endpoints for workloads that do not travel over the Internet.
