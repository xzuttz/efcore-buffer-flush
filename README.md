# What is Entity Framework Buffer Flush 
This is an extension to the [Entity Framework Plus (and Extensions) library](https://entityframework-plus.net/), which can buffer changes locally, before persisting them in the database.
Each bulk operation has a `Maybe` operation, which has a batch size parameter. Upon reaching the batch size, the library will flush the internal cache and persist the changes using Entity Framework Plus (and Extensions). 

# When to use this library ?
The Entity Framework Plus (and Extensions) library also supports batch sizes. It is, however, necessary to have the full batch when saving changes. 

Occasionally, you may not be able to accumulate all changes at once. It might be a good idea to buffer up small changes and persist them in batches rather than persisting them all at once if you expect to save changes frequently. 

If you are consuming messages from Kafka or another message broker, you may not be able to get a batch of messages right away. This library allows you to buffer up changes to a defined batch size or time interval before persisting them, thereby reducing SQL Server's workload. This library might also be useful if you have multiple applications processing messages in parallel and persisting changes to the same tables, and you want to have them back off until the batch size or time interval is reached. 

# Additional information
The `Maybe` operations in this library utilize an internal buffer to store entities, until the buffer reaches its limit or is flushed by the timer. All original functions of the ZZZ library do not use or flush the buffer, because they don't know about it. In the case of BulkMergeMaybe followed by BulkMerge, the entities from BulkMergeMaybe will not be considered. All of the functionality of the ZZZ library is reused in the `Maybe` operations. They act as wrappers, just with an internal buffer and logic for flushing it.

We have only tested the library in a single-threaded environment. If you have an ASP.NET application that receives multiple concurrent requests, the shared buffer may not work as expected due to its static nature. Feel free to test it out and see if it meets your needs.

# CircleCI

[![CircleCI](https://dl.circleci.com/status-badge/img/gh/xzuttz/zzz-efplus-buffer-flush/tree/main.svg?style=svg)](https://dl.circleci.com/status-badge/redirect/gh/xzuttz/zzz-efplus-buffer-flush/tree/main)

# Supported operations

| Operations          |
| --------------------|
|BulkMergeMaybe       |
|BulkUpdateMaybe      |
|BulkInsertMaybe      |
|BulkDeleteMaybe      |

# BulkMergeMaybe example

```c#
_schoolCtx.Students.BulkMergeMaybe(students, 
    options: operation =>
    {
        operation.IgnoreColumnOutputNames = new List<string>();
    },
    batchSize: 5000);
``` 

# Cache flushing

When to flush the cache and save all changes can be determined by your own logic.

## All DbSets stored in cache

```SaveChangesMaybeHelper.FlushCache()``` flushes all DbSets stored in the cache. 

Application Exit example:

```c#
public override async Task StopAsync(CancellationToken cancellationToken)
{
    _logger.LogInformation("Stopping");
    SaveChangesMaybeHelper.FlushCache();
    await base.StopAsync(cancellationToken);
}
```

## Specific DbSet

```c#
SaveChangesMaybeHelper.FlushDbSet<Student>();
```

## Fixed time interval

It is recommended that the cache be flushed periodically. 

Create a `SaveChangesMaybeDbSetTimer` for every DbSet and add it to the `SaveChangesMaybeService`.

```c#
public void ConfigureDbSetFlushIntervals(ISaveChangesMaybeServiceFactory maybeServiceFactory, SchoolContext schoolCtx)
{
    var saveChangesMaybeService = maybeServiceFactory.CreateSaveChangesMaybeService();

    var courseTimer = new SaveChangesMaybeDbSetTimer<Course>(5000);

    var studentTimer = new SaveChangesMaybeDbSetTimer<Student>(1000);

    saveChangesMaybeService.AddTimer(courseTimer);
    saveChangesMaybeService.AddTimer(studentTimer);

    saveChangesMaybeService.Start();
}
``` 

# Dependenecy Injection

The library provides a convenient method for adding the `ISaveChangesMaybeServiceFactory` to the IoC provider.

```c#
services.AddSaveChangesMaybeServiceFactory();
```

# Example

See the [Demo Console App](https://github.com/xzuttz/zzz-efplus-buffer-flush/tree/main/src/SaveChangesMaybe.DemoConsole)