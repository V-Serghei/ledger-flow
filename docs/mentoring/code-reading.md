# How to Read Code

## Process for Analyzing a Change

1. State the observable problem.
2. Find the entry point: an endpoint, test, service, or configuration file.
3. Follow the calls to the place where the relevant decision is made.
4. Separate facts from assumptions.
5. Describe the expected behavior before changing the code.
6. Make the smallest suitable change.
7. Verify it with a focused test, then run broader checks.

## Fact and Assumption

Example of a fact:

> `DevelopmentDataSeeder` stored the email with `ToUpperInvariant()`, while login searched for a lowercase email.

This is directly visible in the code.

Example of an assumption:

> The seed might not have run because the environment was configured incorrectly.

This must be verified through configuration, logs, or database state. It is not a fact until it is verified.

## What `[Fact]` Means

Example xUnit test:

```csharp
[Fact]
public async Task Seeded_customer_can_login()
{
    // Arrange, Act, Assert
}
```

- Square brackets in C# declare an **attribute**: metadata attached to a class, method, or another code element.
- `Fact` is an attribute provided by xUnit.
- It tells the test runner that this method is an individual test without input parameters.
- When `dotnet test` runs, the test runner discovers methods marked with `[Fact]` and executes them.
- `async Task` allows the test to await asynchronous operations.

The attribute does not invoke the method during a normal API run. The test infrastructure reads and acts on it.

## Template for Future Code Notes

```markdown
### Task name

Problem:

Facts from the code or command output:

Assumptions to verify:

Selected solution:

Why the solution is minimal:

Verification:
```
