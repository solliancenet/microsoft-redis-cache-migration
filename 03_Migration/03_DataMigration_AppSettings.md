# Data Migration - Application Settings

## Setup

Follow all the steps in the [Setup](./../05_Appendix/00_Setup.md) guide to create an environment to support the following steps.

## Update Applications to support SSL

TODO

- Switch to the Java Server API (conferencedemo) in Visual Studio code.
- Open the **launch.json** file.
- Update the **DB_CONNECTION_URL** to `jdbc:Redis://servername:5432/reg_app?useUnicode=true&useJDBCCompliantTimezoneShift=true&useLegacyDatetimeCode=false&serverTimezone=UTC&verifyServerCertificate=true&useSSL=true&requireSSL=true&noAccessToProcedureBodies=true`. Note the additional SSL parameters.
- Update **DB_USER_NAME** to **conferenceuser@servername**.
- Start the debug configuration, and ensure that the application works locally with the new instance.

## Change Connection String for the Java API

TODO

- Use the following commands to change the connection string for the App Service Java API

```PowerShell
$rgName = "{RESOURCE_GROUP_NAME}";
$app_name = "{SERVER_NAME}";
az webapp config appsettings set -g $rgName -n $app_name --settings DB_CONNECTION_URL={DB_CONNECTION_URL}
```

  > **Note:** Remember that the Azure Portal can be used to set the connection string.

- Restart the App Service API

```PowerShell
az webapp restart -g $rgName -n $app_name
```  

You have successfully completed an on-premises to Azure Cache for Redis migration!
