# FS.StreamFlow Examples

This directory contains comprehensive examples demonstrating various use cases and patterns with FS.StreamFlow.

## 📁 Examples Overview

### 🏪 E-commerce Examples
- **[Order Processing](order-processing.md)** - Complete order processing workflow with events
- **[Inventory Management](inventory-management.md)** - Real-time inventory updates and reservations
- **[Payment Processing](payment-processing.md)** - Payment workflows with retry and error handling

### 🏢 Enterprise Patterns
- **[Microservices Integration](microservices-integration.md)** - Service-to-service communication
- **[Event-Driven Architecture](event-driven-architecture.md)** - Domain and integration events
- **[Saga Orchestration](saga-orchestration.md)** - Long-running workflow management

### 🔧 Technical Examples
- **[High-Throughput Processing](high-throughput-processing.md)** - Optimized for performance
- **[Error Handling Patterns](error-handling-patterns.md)** - Comprehensive error handling
- **[Monitoring and Observability](monitoring-observability.md)** - Production monitoring setup

### 🚀 Getting Started Examples
- **[Simple Producer-Consumer](simple-producer-consumer.md)** - Basic publish/subscribe
- **[Request-Reply Pattern](request-reply-pattern.md)** - Synchronous communication
- **[Work Queues](work-queues.md)** - Task distribution patterns

## 🎯 How to Use Examples

Each example includes:
- **Complete source code** with detailed comments
- **Configuration setup** for different environments
- **Best practices** and performance tips
- **Common pitfalls** and how to avoid them
- **Unit tests** and integration tests

## 📋 Prerequisites

Before running the examples, ensure you have:
- .NET 9 SDK installed
- RabbitMQ server running (Docker recommended)
- Basic understanding of messaging concepts

## 🐳 Quick Setup with Docker

```bash
# Start RabbitMQ with management UI
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management

# Access management UI at http://localhost:15672 (guest/guest)
```

## 🏃 Running Examples

1. Clone the repository
2. Navigate to the example directory
3. Run the setup script:
   ```bash
   dotnet restore
   dotnet run
   ```

## 🔄 Example Categories

### By Complexity
- **Beginner** 🟢 - Simple concepts, minimal setup
- **Intermediate** 🟡 - Multiple components, some configuration
- **Advanced** 🔴 - Complex patterns, production-ready

### By Use Case
- **Learning** 📚 - Educational examples with detailed explanations
- **Production** 🏭 - Real-world patterns ready for production
- **Performance** ⚡ - High-throughput and optimization examples

## 📝 Contributing Examples

We welcome contributions! If you have a useful example:

1. Create a new markdown file in this directory
2. Follow the example template structure
3. Include complete, runnable code
4. Add tests and documentation
5. Submit a pull request

## 🆘 Getting Help

If you need help with any example:
- Check the troubleshooting section in each example
- Review the main [documentation](../README.md)
- Open an issue on GitHub
- Join our community discussions

## 📊 Example Metrics

| Category | Count | Difficulty | Focus Area |
|----------|-------|------------|------------|
| Getting Started | 3 | 🟢 | Basic patterns |
| E-commerce | 3 | 🟡 | Real-world scenarios |
| Enterprise | 3 | 🔴 | Production patterns |
| Technical | 3 | 🟡 | Advanced features |

Happy coding! 🚀 