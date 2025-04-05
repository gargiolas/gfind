using System.Drawing;
using System.Reflection;
using Colorful;
using GFindDI.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

#pragma warning disable CS8604 // Possible null reference argument.

namespace GFindDI
{
    /// <summary>
    /// Provides extension methods for registering classes implementing specific interfaces into an IServiceCollection.
    /// It facilitates registration with scoped, transient, and singleton lifetimes. Assemblies to scan can either
    /// be specified explicitly or default to the entry assembly.
    /// </summary>
    public static class GFindExtensions
    {
        /// <summary>
        /// Register all classes implementing a specific interface with a scoped lifetime within the specified assembly
        /// or the entry assembly if no assembly is provided.
        /// </summary>
        /// <param name="services">The IServiceCollection instance used to register scoped services.</param>
        /// <param name="assembly">The assembly to scan for classes implementing the interface. Defaults to the entry assembly if not specified.</param>
        /// <typeparam name="TClass">The interface type that the registered classes must implement.</typeparam>
        public static void RegisterScopedServices<TClass>(this IServiceCollection services, Assembly? assembly = null)
            where TClass : class
        {
            var assemblyStartSearch = assembly ?? Assembly.GetEntryAssembly();
            var logger = GetLogger(services);

            FindAndRegisterAllClassesByInterfaceInReferencedAssemblies<TClass>(services, logger, assemblyStartSearch);
        }

        /// <summary>
        /// Register all classes implementing a specific interface with a scoped lifetime across the provided assemblies.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to register services with.</param>
        /// <param name="assemblies">An array of assemblies to scan for classes implementing the interface.</param>
        /// <typeparam name="TClass">The interface type that the implementing classes should adhere to for registration.</typeparam>
        public static void RegisterScopedServices<TClass>(this IServiceCollection services, Assembly[] assemblies)
            where TClass : class
        {
            var logger = GetLogger(services);

            foreach (var assembly in assemblies)
            {
                FindAndRegisterAllClassesByInterfaceInReferencedAssemblies<TClass>(services, logger, assembly);
            }
        }

        /// <summary>
        /// Register all classes implementing a specific interface with a transient lifetime within the specified assembly
        /// or the entry assembly if no assembly is provided.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to register transient services with.</param>
        /// <param name="assembly">The assembly to scan for classes implementing the interface. Defaults to the entry assembly if not specified.</param>
        /// <typeparam name="TClass">The interface type that the registered classes should implement.</typeparam>
        public static void RegisterTransientServices<TClass>(this IServiceCollection services,
            Assembly? assembly = null)
            where TClass : class
        {
            var assemblyStartSearch = assembly ?? Assembly.GetEntryAssembly();


            var logger = GetLogger(services);
            FindAndRegisterAllClassesByInterfaceInReferencedAssemblies<TClass>(services, logger, assemblyStartSearch,
                Lifetime.Transient);
        }

        /// <summary>
        /// Register all classes implementing a specific interface with a transient lifetime within the specified assemblies.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to register transient services with.</param>
        /// <param name="assemblies">The assemblies to scan for classes implementing the interface.</param>
        /// <typeparam name="TClass">The interface type that the registered classes should implement.</typeparam>
        public static void RegisterTransientServices<TClass>(this IServiceCollection services, Assembly[] assemblies)
            where TClass : class
        {
            var logger = GetLogger(services);

            foreach (var assembly in assemblies)
            {
                FindAndRegisterAllClassesByInterfaceInReferencedAssemblies<TClass>(services, logger, assembly,
                    Lifetime.Transient);
            }
        }

        /// <summary>
        /// Register all classes implementing a specific interface with a singleton lifetime within the specified assembly
        /// or the entry assembly if no assembly is provided.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to register singleton services with.</param>
        /// <param name="assembly">The assembly to scan for classes implementing the interface. Defaults to the entry assembly if not specified.</param>
        /// <typeparam name="TClass">The interface type that the registered classes should implement.</typeparam>
        public static void RegisterSingletonServices<TClass>(this IServiceCollection services,
            Assembly? assembly = null)
            where TClass : class
        {
            var assemblyStartSearch = assembly ?? Assembly.GetEntryAssembly();

            var logger = GetLogger(services);

            FindAndRegisterAllClassesByInterfaceInReferencedAssemblies<TClass>(services, logger, assemblyStartSearch,
                Lifetime.Singleton);
        }

        /// <summary>
        /// Register all classes implementing a specific interface with a singleton lifetime within the specified assembly
        /// or the entry assembly if no assembly is provided.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to register singleton services with.</param>
        /// <param name="assemblies">The assembly to scan for classes implementing the interface. Defaults to the entry assembly if not specified.</param>
        /// <typeparam name="TClass">The interface type that the registered classes must implement.</typeparam>
        public static void RegisterSingletonServices<TClass>(this IServiceCollection services, Assembly[] assemblies)
            where TClass : class
        {
            var logger = GetLogger(services);
            foreach (var assembly in assemblies)
            {
                FindAndRegisterAllClassesByInterfaceInReferencedAssemblies<TClass>(services, logger, assembly,
                    Lifetime.Singleton);
            }
        }

        /// <summary>
        /// Register all non-abstract classes implementing the specified interface from the provided assembly
        /// with the specified dependency injection lifetime.
        /// </summary>
        /// <param name="services">The IServiceCollection instance used for registering the services.</param>
        /// <param name="rootAssembly">The assembly in which to search for implementations of the specified interface.</param>
        /// <param name="lifetime">The lifetime to assign to the registered services (e.g., Scoped, Transient, or Singleton).</param>
        /// <param name="logger">An ILogger instance for logging the registration process details.</param>
        /// <typeparam name="TClass">The interface type that the implementing classes must implement.</typeparam>
        private static void RegisterClassesByInterface<TClass>(
            IServiceCollection services,
            Assembly rootAssembly,
            Lifetime lifetime,
            ILogger logger)
            where TClass : class
        {
            // Prevent scanning NuGet or system assemblies
            if (IsNuGetOrSystemAssembly(rootAssembly))
                return;

            // Get all types in the given assembly that are not excluded from search
            var allTypes =
                rootAssembly.
                    GetTypes().
                    Where(type => type.GetCustomAttributes(typeof(ExcludeFromSearchAttribute), inherit: true).Length == 0)
                    .ToList();

            foreach (var type in allTypes)
            {
                if (!IsValidType(type))
                    continue;

                // Find all interfaces implemented by the class
                var interfaces = type.GetInterfaces().Where(item => item == typeof(TClass)).ToList();

                if (interfaces.Count <= 0) continue;

                InitializeDependencyInjection(services, lifetime, interfaces, type, logger);
            }
        }

        /// <summary>
        /// Determines whether a given type is a valid candidate for dependency injection registration.
        /// A valid type must be a non-abstract class.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns>True if the type is a non-abstract class; otherwise, false.</returns>
        private static bool IsValidType(Type type)
        {
            return (type is
                {
                    IsClass: true
                } and
                {
                    IsAbstract: false
                });
        }

        /// <summary>
        /// Configures dependency injection by registering an implementation type for the specified interfaces
        /// using the given lifetime in the provided service collection.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to add the service registrations to.</param>
        /// <param name="lifetime">The lifetime to use for the service, such as Scoped, Transient, or Singleton.</param>
        /// <param name="interfaces">The list of interfaces that the implementation type will be registered for.</param>
        /// <param name="type">The concrete type that will be registered as the implementation for the specified interfaces.</param>
        /// <param name="logger">The logger instance to log information about the registration process.</param>
        private static void InitializeDependencyInjection(
            IServiceCollection services,
            Lifetime lifetime,
            List<Type> interfaces,
            Type type,
            ILogger logger)
        {
            foreach (var @interface in interfaces)
            {
                logger.LogInformation(
                    "Registering service {TypeFullName} as {InterfaceFullName} with lifetime {Lifetime}",
                    type.FullName, @interface.FullName, lifetime);

                var message =
                    "Registering service {0} as {1} with lifetime {2}";

                var items = new Formatter[]
                {
                    new(type.FullName, Color.LawnGreen),
                    new(@interface.FullName, Color.Pink),
                    new(lifetime, Color.PeachPuff),
                };
                Console.WriteLineFormatted(message, Color.Blue, items);

                switch (lifetime)
                {
                    case Lifetime.Scoped:
                        services.AddScoped(@interface, type);
                        break;
                    case Lifetime.Transient:
                        services.AddTransient(@interface, type);
                        break;
                    case Lifetime.Singleton:
                    default:
                        services.AddSingleton(@interface, type);
                        break;
                }
            }
        }

        /// <summary>
        /// Finds and registers all classes implementing a specific interface in the specified root assembly
        /// and its referenced assemblies with the specified service lifetime.
        /// </summary>
        /// <param name="services">The IServiceCollection instance to register services with.</param>
        /// <param name="logger">The ILogger instance used to log diagnostic information during the registration process.</param>
        /// <param name="rootAssembly">The root assembly from which the scanning process begins.</param>
        /// <param name="lifetime">The desired service lifetime for the registrations (Scoped, Transient, or Singleton). Defaults to Scope.</param>
        /// <typeparam name="TClass">The interface type that the classes must implement to be registered.</typeparam>
        private static void FindAndRegisterAllClassesByInterfaceInReferencedAssemblies<TClass>(
            this IServiceCollection services,
            ILogger logger,
            Assembly rootAssembly,
            Lifetime lifetime = Lifetime.Scoped)
            where TClass : class
        {
            InterfaceValidator.ValidateInterface<TClass>();

            ArgumentNullException.ThrowIfNull(rootAssembly);

            // Keep track of processed assemblies to avoid duplicates
            var analyzedAssemblies = new HashSet<string>();
            var assembliesToAnalyze = new Stack<Assembly>();

            // Start with the root assembly
            assembliesToAnalyze.Push(rootAssembly);

            while (assembliesToAnalyze.Count > 0)
            {
                var currentAssembly = assembliesToAnalyze.Pop();

                // Skip already analyzed assemblies
                if (!analyzedAssemblies.Add(currentAssembly.FullName ?? throw new InvalidOperationException()))
                    continue;

                // Register classes and interfaces for the current assembly
                RegisterClassesByInterface<TClass>(services, currentAssembly, lifetime, logger);

                // Analyze referenced assemblies
                LoadReferencedAssemblySafely<TClass>(logger, currentAssembly, assembliesToAnalyze);
            }
        }

        /// <summary>
        /// Safely loads referenced assemblies of the given assembly, adds them to the stack for further analysis, and logs any errors that occur during the loading process.
        /// </summary>
        /// <param name="logger">The ILogger instance used for logging errors during the assembly loading process.</param>
        /// <param name="currentAssembly">The assembly whose referenced assemblies are to be loaded.</param>
        /// <param name="assembliesToAnalyze">The stack used to store assemblies awaiting analysis.</param>
        /// <typeparam name="TClass">The interface type used in filtering classes during registration.</typeparam>
        private static void LoadReferencedAssemblySafely<TClass>(ILogger logger, Assembly currentAssembly,
            Stack<Assembly> assembliesToAnalyze) where TClass : class
        {
            foreach (var referencedAssemblyName in currentAssembly.GetReferencedAssemblies())
            {
                try
                {
                    var referencedAssembly = Assembly.Load(referencedAssemblyName);
                    assembliesToAnalyze.Push(referencedAssembly);
                }
                catch
                {
                    // Handle errors in loading referenced assemblies gracefully
                    logger.LogError("Error: Unable to load assembly {FullName}", referencedAssemblyName.FullName);
                    Console.WriteLine($"Error: Unable to load assembly {referencedAssemblyName.FullName}",
                        Color.Red);
                }
            }
        }

        /// <summary>
        /// Determines if the specified assembly is a NuGet or system assembly by analyzing its file path location.
        /// </summary>
        /// <param name="assembly">The assembly to evaluate.</param>
        /// <returns>True if the assembly is determined to be a NuGet or system assembly; otherwise, false.</returns>
        private static bool IsNuGetOrSystemAssembly(Assembly assembly)
        {
            const string packages = "packages";
            const string nuget = ".nuget";
            const string windows = @"C:\\Windows";

            var location = assembly.Location;
            return location.Contains(packages) || location.Contains(nuget) || location.StartsWith(windows);
        }

        /// <summary>
        /// Retrieves an ILogger instance from the provided IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection used to build the service provider and retrieve the ILogger.</param>
        /// <returns>An ILogger instance for logging purposes.</returns>
        private static ILogger GetLogger(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            return provider.GetService<ILogger>() ?? InitializeFallbackLogger(provider);
        }

        /// <summary>
        /// Initializes and returns a fallback logger using the provided service provider in scenarios where no logger is configured in the service collection.
        /// </summary>
        /// <param name="provider">The IServiceProvider instance used to get the logger factory and create the fallback logger.</param>
        /// <returns>An ILogger instance acting as a fallback logger.</returns>
        private static ILogger InitializeFallbackLogger(IServiceProvider provider)
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger("FallbackLogger");
        }
    }
}