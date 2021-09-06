#https://www.digitalocean.com/community/tutorials/how-to-install-and-secure-redis-on-ubuntu-20-04

sudo add-apt-repository ppa:chris-lea/redis-server -y

sudo apt-apt update -y

sudo apt-get install redis-tools -y

sudo apt-get install redis-server -y

sudo apt-get install make pkg-config libssl-dev -y

wget https://download.redis.io/releases/redis-6.2.5.tar.gz
tar xzf redis-6.2.5.tar.gz
cd redis-6.2.5
make distclean

make BUILD_TLS=yes MALLOC=libc

./utils/gen-test-certs.sh

sudo apt-get install -y tcl-tls -y

#./runtest --tls

cd utils

cd create-cluster

./create-cluster start

./create-cluster create -y

cd

#enable the firewall
sudo ufw allow 6379
sudo ufw allow 30001

#create some users
redis-cli ACL SETUSER chris on allkeys +set >S2@dmins2@dmin
redis-cli ACL SETUSER john on allkeys +set >S2@dmins2@dmin
redis-cli ACL SETUSER mary on allkeys +set >S2@dmins2@dmin

#manual cluster setup...
#setup bind to all IPs
sudo sed -i 's/bind 127.0.0.1/bind 0.0.0.0/' /etc/redis/redis.conf

#switch to systemd
sudo sed -i 's/supervised no/supervised systemd/' /etc/redis/redis.conf

#setup cluster
sudo sed -i 's/# cluster-enabled yes/cluster-enabled yes/' /etc/redis/redis.conf

sudo sed -i 's/# cluster-config-file nodes-6379.conf/cluster-config-file nodes.conf/' /etc/redis/redis.conf

sudo sed -i 's/# cluster-node-timeout 5000/cluster-node-timeout 5000/' /etc/redis/redis.conf

#restart the service
sudo service redis-server restart

#push one of all types...

#simple string
redis-cli set key1 "hello world"

#simple int
redis-cli set key2 12345

#redis list
redis-cli rpush mylist A
redis-cli rpush mylist B
redis-cli rpush mylist first

#hash
redis-cli hmset user:1000 username antirez birthyear 1977 verified 1

#redis set
redis-cli sadd myset 1 2 3

#sorted sets
redis-cli zadd hackers 1940 "Alan Kay"
redis-cli zadd hackers 1957 "Sophie Wilson"
redis-cli zadd hackers 1953 "Richard Stallman"
redis-cli zadd hackers 1949 "Anita Borg"
redis-cli zadd hackers 1965 "Yukihiro Matsumoto"
redis-cli zadd hackers 1914 "Hedy Lamarr"
redis-cli zadd hackers 1916 "Claude Shannon"
redis-cli zadd hackers 1969 "Linus Torvalds"
redis-cli zadd hackers 1912 "Alan Turing"

#REPEAT FOR THE CLUSTER

#simple string
redis-cli -c -h localhost -p 30001 set key1 "hello world"

#simple int
redis-cli -c -h localhost -p 30001 set key2 12345

#redis list
redis-cli -c -h localhost -p 30001 rpush mylist A
redis-cli -c -h localhost -p 30001 rpush mylist B
redis-cli -c -h localhost -p 30001 rpush mylist first

#hash
redis-cli -c -h localhost -p 30001 hmset user:1000 username antirez birthyear 1977 verified 1

#redis set
redis-cli -c -h localhost -p 30001 sadd myset 1 2 3

#sorted sets
redis-cli -c -h localhost -p 30001 zadd hackers 1940 "Alan Kay"
redis-cli -c -h localhost -p 30001 zadd hackers 1957 "Sophie Wilson"
redis-cli -c -h localhost -p 30001 zadd hackers 1953 "Richard Stallman"
redis-cli -c -h localhost -p 30001 zadd hackers 1949 "Anita Borg"
redis-cli -c -h localhost -p 30001 zadd hackers 1965 "Yukihiro Matsumoto"
redis-cli -c -h localhost -p 30001 zadd hackers 1914 "Hedy Lamarr"
redis-cli -c -h localhost -p 30001 zadd hackers 1916 "Claude Shannon"
redis-cli -c -h localhost -p 30001 zadd hackers 1969 "Linus Torvalds"
redis-cli -c -h localhost -p 30001 zadd hackers 1912 "Alan Turing"