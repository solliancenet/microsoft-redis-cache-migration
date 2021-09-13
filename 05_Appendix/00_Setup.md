# Appendix A: Environment Setup

The following steps will configure an environment to perform the guide's migration steps.

## Deploy the ARM template

- Open the Azure Portal
- Create a new resource group
- Select **+Add**, type **template**, select the **Template Deployment...**

  ![](media/00_Template_Deployment.png)

- Select **Create**
- Select **Build your own template in the editor**

  ![](media/00_Build_Template_In_Editor.png)

- Choose between the [`secure`](../artifacts/template-secure.json) or the [`non-secure`](../artifacts/template.json) ARM template.  The difference between the two options is the secured option's resources are hidden behind an App Gateway with private endpoints, whereas the other, resources are directly exposed to the internet.

> **Note** The secure template runs at ~$2700 per month.  The non-secure template runs at ~$1700 per month.

- Copy the json into the window
- Select **Save**

  ![](media/00_Putting_Template_In_Editor.png)

- Fill in the parameters
  - Be sure to record your prefix and password, they are needed later
- Select **Review + create**
- Select the **I agree...** checkbox
- Select **Create**, after about 20 minutes the landing zone will be deployed

  > **NOTE** If anything deploys incorrectly in the redis images via the Azure script extensions, you can check the Azure agent log files using:

  ```bash
  sudo nano /var/lib/waagent/custom-script/download/0/stdout
  sudo nano /var/lib/waagent/custom-script/download/0/stderr
  ```

> **NOTE** If you choose the `secure` template, you will need to perform all the tasks via the Azure Portal or inside the **PREFIX-win10** jump machine in the **PREFIX-vnet-hub** virtual network.  You will also need to make sure that DNS records are correct such that you can connect to the resources in the **PREFIX-vnet-redis** virtual network.  For simplicity, you should use the `non-secure` template.

## Ensure Redis is Configured

- Browse to the Azure Portal
- Select your lab resource group
- Select the **PREFIX-redis01** virtual machine
- Record the public IP address
- Login to the redis image (**PREFIX-redis01** and **PREFIX-redis02**) by opening a PowerShell window and run the following:

  ```PowerShell
  ssh s2admin@<IP>
  ```

- Enter the password `S2@dmins2@dmin`
- Open the `redis.conf` file:

  ```bash
  sudo nano /etc/redis/redis.conf
  ```

- Check the following `bind` statement exists:

  ```bash
  bind 0.0.0.0 ::1
  ```

- If the setting does not exist or it is an empty file, run all the commands in the `.\artifacts\post-install-script01.sh` file

## Connect to the Azure VM

- Login to the deployed instance VM.
  - Browse to the Azure Portal.
  - Select the **PREFIX-win10** virtual machine resource
  - Select **Connect->RDP**.
  - Select **Open** in the RDP dialog.
  - Login using `s2admin` and `S2@dmins2@dmin`.
  - Select **Accept** in the dialog.
  
## Configure and Test the Web Application

Perform the following on the **PREFIX-win10** virtual machine resource.

- Open Visual Studio
- Open the **C:\redismigration\microsoft-redis-cache-migration\artifacts\applications\Redis.sln** file
- When prompted, login using an account that has a Visual Studio license enabled
- When prompted, select to **Install** the .NET desktop development
- In the **RedisWeb** project, open the `.\Properties\launchSettings.json` file
- Update the **IISExpress** profile **REDIS_CONNECTION** environment variable to the Redis Connections string `<REDIS01_VM_IP>:6379`
- Press **F5** to run the application, a browser window should open and display the cache value `Hello World`:

  ![RedisWeb app running with Hello World displayed](./media/RedisWeb_HelloWorld.png)

## Deploy the Application to Azure

- Right-click the **RedisWeb** project, select **Publish**
- Select **Azure**, then select **Next**
- Select **Azure App Server (Linux)**, then select **Next**
- Login if prompted, then select your lab subscription and resource group
- Select the **PREFIX-app01** app service then select **Next**
- Select **Finish**
- Select **Publish**
- Browse to the Azure Portal
- Select the **PREFIX-app01** app service
- Under **Deployment** select **Configuration**
- Select **New application setting**, for the name type **REDIS_CONNECTION**
- Set the value to `<REDIS01_VM_IP>:6379`
- Select **Save**
- Open the web app url `https://PREFIX-app01.azurewebsites.net/`

> **NOTE** If you used the secure template, you would need to ensure your DNS is setup to point to the private IP address of the app service.

- Congratulations. You have migrated the sample Redis app to Azure, now we need to migrate the Redis instance.

# Install twemproxy

- Run the following:

  ```bash
  sudo apt-get install automake libtool autoconf bzip2 -y

  git clone https://github.com/twitter/twemproxy
  cd twemproxy
  autoreconf -fvi
  ./configure --enable-debug=full
  make
  src/nutcracker -h
  ```

- Configure `twemproxy`:

  ```bash
  sudo rm nutcracker.yml
  sudo nano nutcracker.yml
  ```

- Update the configuration to the following:

  ```bash
  alpha:
    listen: 127.0.0.1:22121
    hash: fnv1a_64
    distribution: ketama
    auto_eject_hosts: true
    redis: true
    server_retry_timeout: 2000
    server_failure_limit: 1
    servers:
    - <REDIS_IP1>:6379:0
    - <REDIS_IP2>:6379:0
  ```

- Run `nutcracker`

  ```bash
  cd
  ./twemproxy/src/nutcracker -c ~/twemproxy/conf/nutcracker.yml
  ```

- Test `nutcracker`

  ```bash
  redis-cli -h localhost -p 22121 set hashkey1 "key1"
  redis-cli -h localhost -p 22121 set hashkey2 "key2"
  redis-cli -h localhost -p 22121 set hashkey3 "key3"
  redis-cli -h localhost -p 22121 set hashkey4 "key4"
  redis-cli -h localhost -p 22121 set hashkey5 "key5"
  ```
  