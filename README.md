# gfind

The purpose of this library is to simplify the process of scanning assemblies for types (classes or implementations) of particular interfaces and registering those types into a dependency injection container managed by Microsoft's `IServiceCollection`.

This library simplifies **dependency injection registration** by:
1. Scanning assemblies to find classes implementing specific interfaces (`TClass`).
2. Allowing specific lifetimes (`Scoped`, `Transient`, or `Singleton`) for the registration.
3. Automating the process and reducing boilerplate code.
4. Supporting extensibility by allowing multiple assemblies to be scanned.

It’s especially useful in scenarios where there are many services implementing multiple interfaces, and you want a scalable, centralized way to handle DI registration.

Example

     services.RegisterScopedServices<IRandomService>(assemblies);
