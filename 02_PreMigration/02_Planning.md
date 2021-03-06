# Planning

## Landing Zone

An [Azure Landing zone](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/landing-zone/) is the target environment defined as the final resting place of a cloud migration project. In most projects, the landing zone should be scripted via ARM templates for its initial setup. Finally, it should be customized with PowerShell or the Azure Portal to fit the needs of the workload.

Since WWI is based in San Francisco, all resources for the Azure landing zone were created in the `US West 2` region. The following resources were created to support the migration:

- [Azure Cache for Redis](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/quickstart-create-redis)

- [Express Route](https://docs.microsoft.com/en-us/azure/expressroute/expressroute-introduction)

- [Azure Virtual Network](https://docs.microsoft.com/en-us/azure/virtual-network/quick-create-portal) with [hub and spoke design](https://docs.microsoft.com/en-us/azure/architecture/reference-architectures/hybrid-networking/hub-spoke) with corresponding [virtual network peerings](https://docs.microsoft.com/en-us/azure/virtual-network/virtual-network-peering-overview) establish.

- [App Service](https://docs.microsoft.com/en-us/azure/app-service/overview)

- [Application Gateway](https://docs.microsoft.com/en-us/azure/load-balancer/quickstart-load-balancer-standard-internal-portal?tabs=option-1-create-internal-load-balancer-standard)

- [Private endpoints](https://docs.microsoft.com/en-us/azure/private-link/private-endpoint-overview) for the App Services and Redis instance

    > **Note:** As part of this guide, two ARM templates (one with private endpoints, one without) were provided to deploy a potential Azure landing zone for a Redis migration project. The private endpoints ARM template provides a more secure, production-ready scenario. Additional manual Azure landing zone configuration may be necessary, depending on the requirements.

    > **Note** Creating a Redis instance in an Azure Virtual Machine with a default port and no password or on an non-SSL port with a password with no network security group protecting them is highly discouraged.  Bots continually monitor the Azure IP address space and will find your Redis instance within a few days.  Be very careful creating resources that are exposed to the internet.

## Networking

Getting data from the source system to Azure Cache for Redis in a fast and optimal way is a vital component to consider in a migration project. Small unreliable connections may require administrators to restart the migration several times until a successful result is achieved. Restarting migrations due to network issues can lead to wasted effort, time and money.

Take the time to understand and evaluate the network connectivity between the source, tool, and destination environments. In some cases, it may be appropriate to upgrade the internet connectivity or configure an ExpressRoute connection from the on-premises environment to Azure. Once on-premises to Azure connectivity has been created, the next step is to validate that the selected migration tool can connect from the source to the destination.

The migration tool location will determine the network connectivity requirements. As shown in the table below, the selected migration tool must be able to connect to both the on-premises machine and Azure. Azure should be configured to only accept network traffic from the migration tool location.

| **Migration Tool** | **Type** | **Tool Location** | **Inbound Network Requirements** | **Outbound Network Requirements** |
| --- | --- | --- | --- | --- |
| Import/Export (RDB) | Offline | On-premises  | None | A path to copy the file to the new instance |
| DUMP/RESTORE | Online | On-premises  | None | Open port to the target instance |
| SLAVEOF / REPLICAOF | Online | On-premises  | None | Open port to the target instance |
| MIGRATE | Online | On-premises  | None | Open port to the target instance |
| 3rd party tools | Offline or Online | On-premises  | Based on tool | Based on tool |

> **Note** We will discuss these migration methods in more detail in the next section.

Other networking considerations include:

- When using an Azure Virtual Machine to run the migration tools, assign it a public IP address and then only allow it to connect to the on-premises Redis instance.

- Outbound firewalls must ensure outbound connectivity to Azure Cache for Redis.

## Private Link and/or VNet integration

All Azure Cache for Redis services supports private links and VNet integration.  There are however be sure to review the [FAQs for private endpoints](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-private-link#faq) to understand the behavior of the cache when behind a private endpoint.

You should also be familiar with the communication ports of Redis which are outlined in [Outbound port requirements](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-premium-vnet#outbound-port-requirements).

When integrating with other Azure services, you must also ensure that [other network connectivity](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-premium-vnet#additional-virtual-network-connectivity-requirements) is also allowed in a virtual network.

## Networking with Geo-replication

If you plan to use the Geo-replication feature of Azure Cache for Redis, there are several other ports that must be allowed for the replication to be successful.  See [Geo-replication peer port requirements](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-how-to-premium-vnet#geo-replication-peer-port-requirements) for more information.

## SSL/TLS Connectivity

In addition to the application implications of migrating to SSL-based communication, the SSL/TLS connection types are also something that needs to be considered. After creating the Azure Cache for Redis instance, review the SSL settings, and read the [Configure your application to use TLS 1.2](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-remove-tls-10-11#configure-your-application-to-use-tls-12) article to understand how the TLS settings can affect the security posture of an application.

## WWI Use Case

WWI's cloud team has created the necessary Azure landing zone resources in a specific resource group for the Azure Cache for Redis. Additional resources will be included to support the applications. To create the landing zone, WWI decided to script the setup and deployment using ARM templates. By using ARM templates, they would be able to quickly tear down and re-setup the environment, if needed.

As part of the ARM template, all connections between virtual networks will be configured with peering in a hub and spoke architecture. The instance and application will be placed into separate virtual networks. An Azure App Gateway will be placed in front of the app service to allow the app service to be isolated from the Internet.  The Azure App Service will connect to the Azure Cache for Redis using a private endpoint.

WWI originally wanted to test an online migration, but the lack of replication support made this infeasible as they did not want to deal with complexities of replaying the AOF files. WWI chose to do an offline migration instead. The Redis RDB backup option was used to export the on-premises data and then was used to import the data into the Azure Cache for Redis instance via Azure Storage.

## Planning Checklist

- Prepare the Azure landing zone. Consider using ARM template deployment in case the environment must be torn down and rebuilt quickly.
- Verify the networking setup. Verification should include testing connectivity, bandwidth, latency, and firewall configurations.
- Determine if you are going to use the online or offline data migration strategy.
- Decide on the SSL certificate strategy.
