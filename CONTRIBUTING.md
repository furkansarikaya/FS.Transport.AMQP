# Contributing to FS.RabbitMQ

First off, thank you for considering contributing to FS.RabbitMQ! It's people like you that make FS.RabbitMQ such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the issue list as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

* Use a clear and descriptive title
* Describe the exact steps which reproduce the problem
* Provide specific examples to demonstrate the steps
* Describe the behavior you observed after following the steps
* Explain which behavior you expected to see instead and why
* Include stack traces and error messages
* Include your environment details (OS, .NET version, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

* Use a clear and descriptive title
* Provide a step-by-step description of the suggested enhancement
* Provide specific examples to demonstrate the steps
* Describe the current behavior and explain which behavior you expected to see instead
* Explain why this enhancement would be useful
* List some other libraries or applications where this enhancement exists

### Pull Requests

* Fill in the required template
* Do not include issue numbers in the PR title
* Include screenshots and animated GIFs in your pull request whenever possible
* Follow the C# coding style
* Include unit tests
* Document new code
* End all files with a newline

## Development Process

1. Fork the repo
2. Create a new branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Run the tests (`dotnet test`)
5. Commit your changes (`git commit -m 'Add some amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Setup Development Environment

```bash
# Clone the repository
git clone https://github.com/furkansarikaya/FS.RabbitMQ.git

# Navigate to the directory
cd FS.RabbitMQ

# Install dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

### Coding Style

* Use 4 spaces for indentation
* Use PascalCase for class names and public members
* Use camelCase for private members and parameters
* Use meaningful names for variables and methods
* Keep methods focused and small
* Add XML documentation for public APIs
* Follow Microsoft's C# coding conventions

### Testing

* Write unit tests for new code
* Ensure all tests pass before submitting PR
* Include integration tests where appropriate
* Follow the AAA (Arrange, Act, Assert) pattern
* Use meaningful test names that describe the scenario

Example test:

```csharp
[Fact]
public async Task PublishMessage_WithValidMessage_PublishesSuccessfully()
{
    // Arrange
    var producer = new MessageProducer(_connectionManager);
    var message = new TestMessage { Content = "Hello" };

    // Act
    var result = await producer.PublishAsync(
        "test-exchange",
        "test.route",
        message);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.MessageId);
}
```

### Documentation

* Update README.md with details of changes to the interface
* Update API documentation for modified public APIs
* Include code examples for new features
* Update any relevant examples in the docs folder

## Project Structure

```
FS.RabbitMQ/
├── src/
│   └── FS.Transport.AMQP/
│       ├── Configuration/    # Configuration classes
│       ├── Connection/       # Connection management
│       ├── Consumer/         # Message consumer
│       ├── Core/            # Core interfaces and classes
│       ├── ErrorHandling/   # Error handling and retry policies
│       ├── EventBus/        # Event bus implementation
│       ├── EventStore/      # Event sourcing
│       ├── Producer/        # Message producer
│       ├── Saga/           # Saga orchestration
│       └── Serialization/   # Message serialization
├── tests/
│   └── FS.Transport.AMQP.Tests/
│       ├── Integration/     # Integration tests
│       └── Unit/           # Unit tests
└── docs/                   # Documentation
```

## Release Process

1. Update version numbers
2. Update CHANGELOG.md
3. Create release notes
4. Create a new release on GitHub
5. Publish to NuGet

### Version Numbers

We use [Semantic Versioning](https://semver.org/). Given a version number MAJOR.MINOR.PATCH:

* MAJOR version for incompatible API changes
* MINOR version for backwards-compatible functionality
* PATCH version for backwards-compatible bug fixes

### Creating a Release

```bash
# Update version in .csproj
<Version>1.2.3</Version>

# Create release notes
git tag -a v1.2.3 -m "Release version 1.2.3"
git push origin v1.2.3

# Build and pack
dotnet build -c Release
dotnet pack -c Release

# Publish to NuGet
dotnet nuget push bin/Release/FS.Transport.AMQP.1.2.3.nupkg -k [API_KEY] -s https://api.nuget.org/v3/index.json
```

## License

By contributing, you agree that your contributions will be licensed under the MIT License. 