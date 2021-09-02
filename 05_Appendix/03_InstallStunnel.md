# SSL Connectivity with STunnel

In order to make calls to an SSL enabled version of Redis, you will need the latest redis-cli (6.x or higher) or you will need to enable a proxy to the SSL endpoints.

## Install STunnel

```bash
sudo apt-get install stunnel4 -y
```

- Setup the SSL port for Azure Redis

```bash
sudo nano /etc/default/stunnel4
```

- Set the `Enabled` value to `1`
- Save the file
- Setup the pids directory and create the redis connection:

```bash
cd

mkdir pids

sudo chown -R nobody:nogroup pids/

sudo nano /etc/stunnel/redis.conf
```

- Add the following:

```text
[redis-cli]
client = yes
accept = 127.0.0.1:6380
connect = yourcachename.redis.cache.windows.net:6380
```

- Restart stunnel

```bash
/etc/init.d/stunnel4 restart
```

- Ensure that stunnel is running:

```bash
sudo apt install net-tools

sudo netstat -tlpn | grep 6380
```
