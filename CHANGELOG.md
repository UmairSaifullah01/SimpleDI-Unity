# Changelog

All notable changes to the Finite State Machine package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2024-03-31

### Added

- New extension methods for objects to work with dependencies:
  - `GetDependency<T>()` - Get a dependency of specified type
  - `GetDependency<T>(Type implementationType)` - Get a specific implementation
  - `BindDependency<TInterface, TImplementation>()` - Bind a dependency
  - `BindDependency<TInterface, TImplementation>(DependencyFactory factory, Lifetime lifetime)` - Bind with custom factory
  - `InjectDependencies<T>()` - Inject dependencies into an object
  - `BindSingleton<TInterface>(TInterface instance)` - Bind a singleton instance
  - `BindScoped<TInterface>(TInterface instance)` - Bind a scoped instance

### Improvements

- Added support for scoped dependencies
- Enhanced dependency injection with more flexible binding options
- Improved code readability with fluent API for dependency management

## [1.0.0] - 2024-05-14

### Fixes

- 55555555
- 444
- 5555

### Improvements

- ...
- ...
