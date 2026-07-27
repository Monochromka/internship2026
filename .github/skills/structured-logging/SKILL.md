---
name: structured-logging
description: Adds structured logging to ASP.NET Core services following the project's logging conventions. Use this when asked to add, improve, or review logging.
---

When asked to add or improve logging, follow these guidelines.

## General principles

- Use `ILogger<T>` through dependency injection.
- Use structured logging with named placeholders.
- Log meaningful business events instead of implementation details.
- Keep log messages concise and focused on a single event.
- Never duplicate the same log in multiple layers.

## Inject ILogger

If a class does not already have a logger, inject one using dependency injection.

Example:

```csharp
private readonly ILogger<TaskService> _logger;

public TaskService(
    AppDbContext context,
    ILogger<TaskService> logger)
{
    _context = context;
    _logger = logger;
}
```

## Always use structured logging

Always write logs like:

```csharp
_logger.LogInformation(
    "Task {TaskId} created for project {ProjectId}",
    task.Id,
    task.ProjectId);
```

Never use string interpolation.

Avoid:

```csharp
_logger.LogInformation($"Task {task.Id} created");
```

## Preferred logging locations

Add logs in the Service layer for business operations, including:

- creating entities
- updating entities
- deleting entities
- changing status
- business validation
- interactions with external services

Add logs in external API clients when:

- making HTTP requests
- external validation fails
- external services are unavailable

Controllers should generally not log successful CRUD operations because they only coordinate HTTP requests.

## Log levels

Use **Information** for successful business operations.

Examples:

- project created
- task created
- task updated
- task deleted
- task status changed

Use **Warning** for handled but unexpected situations.

Examples:

- entity not found
- validation failed
- duplicate entity
- external service returned 404
- operation skipped

Use **Error** only for failed operations or exceptions.

Always include the exception object.

Example:

```csharp
_logger.LogError(
    ex,
    "Failed to create task for project {ProjectId}",
    projectId);
```

Do not log only `ex.Message`.

## Include identifiers

Whenever applicable include identifiers as structured properties.

Examples:

- ProjectId
- TaskId
- UserId
- Status

These properties should be passed as template parameters instead of concatenated into strings.

## Avoid noisy logging

Do not log:

- entering methods
- exiting methods
- every repository call
- every EF Core operation
- every HTTP request handled by controllers
- framework lifecycle events

Avoid logs such as:

```csharp
_logger.LogInformation("Entering Create()");
```

or

```csharp
_logger.LogInformation("Method finished");
```

These provide little diagnostic value.

## Sensitive information

Never log:

- passwords
- tokens
- connection strings
- secrets
- personal or confidential information

## Goal

Logging should make it possible to understand:

- what business operation occurred;
- which entity was affected;
- whether it succeeded or failed;
- why it failed if an error occurred.

The generated logging should follow ASP.NET Core and Azure Application Insights best practices.