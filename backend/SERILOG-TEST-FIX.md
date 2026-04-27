# Serilog "Logger Already Frozen" Fix

## Problem

Integration tests using `WebApplicationFactory` were failing with:
```
System.InvalidOperationException: The logger is already frozen.
at Serilog.Extensions.Hosting.ReloadableLogger.Freeze()
```

This occurred in the `CorsConfigurationTests.PreflightRequest_WithAllowedOrigin_ReturnsSuccessWithCorsHeaders` test and would affect any test that creates multiple `WebApplicationFactory` instances.

## Root Cause

**xUnit Parallelization + Serilog's Global Static Logger = Race Condition**

1. By default, xUnit runs test classes **in parallel** to improve performance
2. Each test class using `WebApplicationFactory` boots up the entire API in memory
3. `Program.cs` initializes Serilog's **global static logger** (`Log.Logger`) at startup
4. When multiple tests run simultaneously:
   - Test A boots the API and **freezes** the Serilog logger
   - Test B (running in parallel) tries to boot the API
   - Test B tries to configure Serilog, but it's already frozen by Test A
   - **Fatal crash**: `InvalidOperationException: The logger is already frozen`
   - Test runner can't connect: `The entry point exited without ever building an IHost`

## Solution

**Disable xUnit test parallelization** to run tests sequentially.

### Implementation

Added a single assembly-level attribute to `TestWebApplicationFactory.cs`:

```csharp
// DISABLE XUNIT PARALLELIZATION TO PREVENT SERILOG "LOGGER ALREADY FROZEN" ERRORS
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

This tells xUnit to run tests one at a time:
1. Test A boots the API → runs → shuts down
2. Test B boots the API → runs → shuts down
3. No Serilog collisions

### Why This Is the Best Solution

1. **Simple**: One line of code, no complex workarounds
2. **Robust**: Prevents race conditions with Serilog AND InMemory database
3. **Minimal performance impact**: Tests complete in ~36 seconds (already fast)
4. **No code changes needed**: Program.cs and test code remain clean
5. **Industry standard**: Recommended approach for WebApplicationFactory + Serilog

### Alternative Solutions (Not Recommended)

❌ **Guard bootstrap logger initialization** - Fragile, doesn't prevent other race conditions  
❌ **Use different logger per test** - Breaks Serilog's architecture  
❌ **Mock Serilog in tests** - Loses valuable diagnostic information  
✅ **Disable parallelization** - Clean, simple, effective

## Performance Impact

- **Before**: Tests run in parallel, ~36 seconds, 1 failure
- **After**: Tests run sequentially, ~36 seconds, 0 failures
- **Verdict**: No noticeable performance difference (tests are already fast)

## Testing

Run all tests to verify:
```bash
dotnet test backend/Velocify.Tests/Velocify.Tests.csproj --configuration Release
```

Expected result:
```
Total tests: 110
Passed: 107
Failed: 0
Skipped: 3
```

## Related Files

- `backend/Velocify.Tests/TestWebApplicationFactory.cs` - Added `[assembly: CollectionBehavior(DisableTestParallelization = true)]`
- `backend/Velocify.API/Program.cs` - No changes needed (kept original Serilog configuration)

## References

- [xUnit Parallelization Documentation](https://xunit.net/docs/running-tests-in-parallel)
- [Serilog Two-Stage Initialization](https://github.com/serilog/serilog-aspnetcore#two-stage-initialization)
- [WebApplicationFactory Best Practices](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Common Issue: Serilog + xUnit + WebApplicationFactory](https://github.com/serilog/serilog-aspnetcore/issues/271)
