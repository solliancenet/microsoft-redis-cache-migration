# Data Migration - Application Settings

Most applications use Redis client libraries to handle communication with their caches. In some cases, you may need to upgrade the client library to get the SSL supported version.  Once you have that version, you may also need to make code changes to support SSL.  Reference [Configure your application to use TLS 1.2](https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-remove-tls-10-11#configure-your-application-to-use-tls-12) for more information.

## Setup

Follow all the steps in the [Setup](./../05_Appendix/00_Setup.md) guide to create an environment to support the following steps.

## Update Applications to support SSL

- Open `Redis-Testing` project
- Update the following code:

```csharp
TODO
```

- Update the Connectionstring

```text
TODO
```

## Redeploy the application

- TODO

## Test the application

- TODO

You have successfully completed an on-premises to Azure Cache for Redis migration!
