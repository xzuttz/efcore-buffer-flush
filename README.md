# What is Entity Framework Buffer Flush 

This is an extension to the [Entity Framework Plus (and Extensions) library](https://entityframework-plus.net/), which can buffer changes locally, before persisting them in the database.
Every EFPlus operation has a corresponding `Maybe` operation, which has a batch size. When the batch size is reached, the library will flush the internal cache and persist the changes. 

# BulkMergeMaybe 

```c#
_schoolCtx.Students.BulkMergeMaybe(students, 
    options: operation =>
    {
        operation.IgnoreColumnOutputNames = new List<string>();
    },
    batchSize: 5000);
``` 

# Flushing the cache in a fixed time interval

The cache should be flushed every now and then. 

Create a `SaveChangesMaybeDbSetTimer` for every DbSet and add it to the `SaveChangesMaybeService`.

```c#
public void ConfigureDbSetFlushIntervals(ISaveChangesMaybeServiceFactory maybeServiceFactory, SchoolContext schoolCtx)
{
    var saveChangesMaybeService = maybeServiceFactory.CreateSaveChangesMaybeService();

    var courseTimer = new SaveChangesMaybeDbSetTimer<Course>(5000)
    {
        DbSetToFlush = schoolCtx.Courses,
    };

    var studentTimer = new SaveChangesMaybeDbSetTimer<Student>(1000)
    {
        DbSetToFlush = schoolCtx.Students,
    };

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
