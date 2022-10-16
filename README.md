# What is Entity Framework Buffer Flush 

This is an extension to the [Entity Framework Plus (and Extensions) library](https://entityframework-plus.net/), which can buffer changes locally, before persisting them in the database.
Every bulk operation has a corresponding `Maybe` operation, which has a batch size as parameter. When the batch size is reached, the library will flush the internal cache and persist the changes using the Entity Framework Plus (and Extensions) library. 

# Currently supported operations

| Operations          |
| --------------------|
|BulkMergeMaybe       |
|BulkUpdateMaybe      |

# BulkMergeMaybe example

```c#
_schoolCtx.Students.BulkMergeMaybe(students, 
    options: operation =>
    {
        operation.IgnoreColumnOutputNames = new List<string>();
    },
    batchSize: 5000);
``` 

# Additional information

The `Maybe` operations in this library use an internal buffer to store entities, until the buffer has reached its limit, or until the buffer is flushed by the timer. The buffer will not be used or flushed when calling any of the original non-`Maybe`-functions of the ZZZ library, as they do not know about the buffer. If you first use BulkMergeMaybe and then the original BulkMerge afterwards, the entities from BulkMergeMaybe will not be taken into account. The `Maybe` operations reuse all of the existing functionality of the ZZZ library, they act as a proxy, just with an internal buffer and logic to determine when to flush it.

# Flushing the cache in a fixed time interval

The cache should be flushed every now and then. 

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

The library offers a convenient method for adding the `ISaveChangesMaybeServiceFactory` to the IoC provider.

```c#
services.AddSaveChangesMaybeServiceFactory();
```

# Example

See the [Demo Console App](https://github.com/xzuttz/zzz-efplus-buffer-flush/tree/main/src/SaveChangesMaybe.DemoConsole)
