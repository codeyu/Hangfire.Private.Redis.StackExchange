﻿using System;
using Microsoft.Win32;
using Moq;
using Xunit;
using StackExchange.Redis;

namespace Hangfire.Redis.Tests
{
    public class RedisFetchedJobFacts
    {
        private const string JobId = "id";
        private const string Queue = "queue";

        private readonly Mock<IDatabase> _redis;

        public RedisFetchedJobFacts()
        {
            _redis = new Mock<IDatabase>();
        }

        [Fact]
        public void Ctor_ThrowsAnException_WhenRedisIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new RedisFetchedJob(null, JobId, Queue));

            Assert.Equal("redis", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsAnException_WhenJobIdIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new RedisFetchedJob(_redis.Object, null, Queue));

            Assert.Equal("jobId", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsAnException_WhenQueueIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new RedisFetchedJob(_redis.Object, JobId, null));

            Assert.Equal("queue", exception.ParamName);
        }

        [Fact, CleanRedis]
        public void RemoveFromQueue_RemovesJobFromTheFetchedList()
        {
            UseRedis(redis =>
            {
                // Arrange
                redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "job-id");

                var fetchedJob = new RedisFetchedJob(redis, "job-id", "my-queue");

                // Act
                fetchedJob.RemoveFromQueue();

                // Assert
                Assert.Equal(0, redis.ListLength("{hangfire}:queue:my-queue:dequeued"));
            });
        }

        [Fact, CleanRedis]
        public void RemoveFromQueue_RemovesOnlyJobWithTheSpecifiedId()
        {
            UseRedis(redis =>
            {
                // Arrange
				redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "job-id");
                redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "another-job-id");

                var fetchedJob = new RedisFetchedJob(redis, "job-id", "my-queue");

                // Act
                fetchedJob.RemoveFromQueue();

                // Assert
                Assert.Equal(1, redis.ListLength("{hangfire}:queue:my-queue:dequeued"));
                Assert.Equal("another-job-id", (string)redis.ListRightPop("{hangfire}:queue:my-queue:dequeued"));
            });
        }

        [Fact, CleanRedis]
        public void RemoveFromQueue_RemovesOnlyOneJob()
        {
            UseRedis(redis =>
            {
                // Arrange
				redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "job-id");
                redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "job-id");

                var fetchedJob = new RedisFetchedJob(redis, "job-id", "my-queue");

                // Act
                fetchedJob.RemoveFromQueue();

                // Assert
                Assert.Equal(1, redis.ListLength("{hangfire}:queue:my-queue:dequeued"));
            });
        }

        [Fact, CleanRedis]
        public void RemoveFromQueue_RemovesTheFetchedFlag()
        {
            UseRedis(redis =>
            {
                // Arrange
                redis.HashSet("{hangfire}:job:my-job", "Fetched", "value");
                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.RemoveFromQueue();

                // Assert
                Assert.False(redis.HashExists("{hangfire}:job:my-job", "Fetched"));
            });
        }

        [Fact, CleanRedis]
        public void RemoveFromQueue_RemovesTheCheckedFlag()
        {
            UseRedis(redis =>
            {
                // Arrange
                redis.HashSet("{hangfire}:job:my-job", "Checked", "value");
                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.RemoveFromQueue();

                // Assert
                Assert.False(redis.HashExists("{hangfire}:job:my-job", "Checked"));
            });
        }

        [Fact, CleanRedis]
        public void Requeue_PushesAJobBackToQueue()
        {
            UseRedis(redis => 
            {
                // Arrange
                redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "my-job");
                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.Requeue();

                // Assert
                Assert.Equal("my-job", (string)redis.ListRightPop("{hangfire}:queue:my-queue"));
            });
        }

        [Fact, CleanRedis]
        public void Requeue_PushesAJobToTheRightSide()
        {
            UseRedis(redis =>
            {
                // Arrange
				redis.ListRightPush("{hangfire}:queue:my-queue", "another-job");
                redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "my-job");

                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.Requeue();

                // Assert - RPOP
                Assert.Equal("my-job", (string)redis.ListRightPop("{hangfire}:queue:my-queue")); 
            });
        }

        [Fact, CleanRedis]
        public void Requeue_RemovesAJobFromFetchedList()
        {
            UseRedis(redis =>
            {
                // Arrange
                redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "my-job");
                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.Requeue();

                // Assert
                Assert.Equal(0, redis.ListLength("{hangfire}:queue:my-queue:dequeued"));
            });
        }

        [Fact, CleanRedis]
        public void Requeue_RemovesTheFetchedFlag()
        {
            UseRedis(redis =>
            {
                // Arrange
                redis.HashSet("{hangfire}:job:my-job", "Fetched", "value");
                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.Requeue();

                // Assert
                Assert.False(redis.HashExists("{hangfire}:job:my-job", "Fetched"));
            });
        }

        [Fact, CleanRedis]
        public void Requeue_RemovesTheCheckedFlag()
        {
            UseRedis(redis =>
            {
                // Arrange
                redis.HashSet("{hangfire}:job:my-job", "Checked", "value");
                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.Requeue();

                // Assert
                Assert.False(redis.HashExists("{hangfire}:job:my-job", "Checked"));
            });
        }

        [Fact, CleanRedis]
        public void Dispose_WithNoComplete_RequeuesAJob()
        {
            UseRedis(redis =>
            {
                // Arrange
                redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "my-job");
                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.Dispose();

                // Assert
                Assert.Equal(1, redis.ListLength("{hangfire}:queue:my-queue"));
            });
        }

        [Fact, CleanRedis]
        public void Dispose_AfterRemoveFromQueue_DoesNotRequeueAJob()
        {
            UseRedis(redis =>
            {
                // Arrange
				redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "my-job");
                redis.ListRightPush("{hangfire}:queue:my-queue:dequeued", "my-job");
                var fetchedJob = new RedisFetchedJob(redis, "my-job", "my-queue");

                // Act
                fetchedJob.RemoveFromQueue();
                fetchedJob.Dispose();

                // Assert
                Assert.Equal(0, redis.ListLength("{hangfire}:queue:my-queue"));
            });
        }

        private static void UseRedis(Action<IDatabase> action)
        {
			var redis = RedisUtils.CreateClient();
            action(redis);
        }
    }
}
