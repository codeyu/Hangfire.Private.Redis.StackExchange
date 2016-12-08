# Hangfire.Redis.StackExchange

[![Build Status](https://ci.appveyor.com/api/projects/status/g3kymrbabl5hifk0/branch/master?svg=true)](https://ci.appveyor.com/project/codeyu/hangfire-private-redis-stackexchange)

[![Hangfire.Private.Redis.StackExchange Nightly Build Status](https://www.myget.org/BuildSource/Badge/Hangfire.Private.Redis.StackExchange?identifier=353f7625-8961-4162-b1b3-bd28373cc4b6)](https://www.myget.org/gallery/Hangfire.Private.Redis.StackExchange)

HangFire Redis storage based on [HangFire.Redis](https://github.com/HangfireIO/Hangfire.Redis/) but using lovely [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis) client.

#### Highlights
- Support for Hangfire Batches ([feature of Hangfire Pro](http://hangfire.io/blog/2015/04/17/hangfire-pro-1.2.0-released.html))
- Efficient use of Redis resources thanks to ConnectionMultiplexer
- Support for Redis Prefix, allow multiple Hangfire Instances against a single Redis DB
- Allow customization of Succeeded and Failed lists size

