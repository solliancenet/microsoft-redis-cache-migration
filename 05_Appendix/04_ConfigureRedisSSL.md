# Configure Redis SSL

SSL/TLS is supported by Redis starting with version 6 as an optional feature that needs to be enabled at compile time.

## Download Redis 6.0

- Run the following to download Redis 6.0

```bash
wget https://download.redis.io/releases/redis-6.2.5.tar.gz
tar xzf redis-6.2.5.tar.gz
cd redis-6.2.5
```

## Build with TLS support

- Run the following to build Redis with TLS support:

```bash
sudo apt install make pkg-config libssl-dev

make distclean

make BUILD_TLS=yes MALLOC=libc
```

## Create Certificates

- Create the self-signed certificates:

```bash
./utils/gen-test-certs.sh

sudo apt-get install -y tcl-tls

./runtest --tls
```

## Start the server

```bash
./src/redis-server --tls-port 6380 --port 0 \
    --tls-cert-file ./tests/tls/redis.crt \
    --tls-key-file ./tests/tls/redis.key \
    --tls-ca-cert-file ./tests/tls/ca.crt
```

The Redis instance should be running and now ready to accept SSL connections.

## Test connectivity

```bash
./src/redis-cli --tls \
    --cert ./tests/tls/redis.crt \
    --key ./tests/tls/redis.key \
    --cacert ./tests/tls/ca.crt
```

## Resources

- https://redis.io/topics/encryption