# What is Entity Framework Plus Buffer Flush 

This is an extension to the Entity Framework Plus library, which can buffer changes locally, before persisting them in the database.
Every operation allows a batch size. When the batch size is reached, the library will flush the internal cache and persist the changes. 

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

The library offers a convention method for adding the `ISaveChangesMaybeServiceFactory` to the IoC provider.

```c#
services.AddSaveChangesMaybeServiceFactory();
```