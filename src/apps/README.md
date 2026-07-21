# Demo Application

This folder contains a demo application composed



## Load Test Web Servers with Apache Bench (ab) on Ubuntu

To 

[Read this post](https://oneuptime.com/blog/post/2026-03-04-how-to-load-test-web-servers-with-apache-bench-ab-on-rhel/view)



```sh
ab -n 10000 -c 50 \
  -p ./dev/test.json \
  -T 'application/json' \
  -H 'Authorization: Bearer your_token' \
  http://localhost:5500/orders-service/orders
  ```
