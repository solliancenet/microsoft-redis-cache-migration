# Configure Redis SSL

SSL/TLS is supported by Redis starting with version 6 as an optional feature that needs to be enabled at compile time.

## Download Redis 6.0

- TODO

## Build with TLS support

- TODO

```bash
make BUILD_TLS=yes
```

## Create Certificates

- TODO

```bash
./utils/gen-test-certs.sh
./runtest --tls 
```

## Start the server

```bash
./src/redis-server --tls-port 6379 --port 0 \
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