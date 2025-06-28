# Simple Dependency Injection For Unity

A lightweight and powerful dependency injection container for Unity that makes it easy to manage dependencies, bindings, and object lifecycle.

## Installation

This library is distributed via Unity's built-in package manager. Required Unity 2018.3 or later.

```
https://github.com/UmairSaifullah01/SimpleDI-Unity.git
```

### Unity Package

- Open Unity project
- Download and run .unitypackage file from the latest release

![SimpleDI](https://raw.githubusercontent.com/UmairSaifullah01/Images/master/SimpleDI.jpg)

## Features

### Core Features

- **Multiple Lifetime Scopes**

  - Transient: New instance created for each request
  - Singleton: Single instance shared across all requests
  - Scoped: One instance per scope (e.g., per scene)

- **Dependency Injection Types**

  - Constructor Injection
  - Field Injection (using [Inject] attribute)
  - Property Injection (using [Inject] attribute)
  - Method Injection (using [Inject] attribute)

- **Binding Capabilities**

  - Interface to Implementation binding
  - Multiple implementations for single interface
  - Custom factory methods
  - Conditional bindings
  - Decorator pattern support

- **Installation Types**
  - Standard Installer (for non-MonoBehaviour scenarios)
  - MonoInstaller (for Unity components)
  - SOInstaller (ScriptableObject-based configuration)

### Advanced Features

- **Automatic Registration**

  - [Injectable] attribute for automatic registration
  - Automatic interface binding
  - Scene and Project context support

- **Unity Integration**

  - MonoBehaviour support
  - ScriptableObject support
  - Scene-based dependency management
  - Project-wide dependency management

- **Extension Methods**
  - Fluent API for binding
  - Convenient dependency resolution
  - Unity-specific instantiation helpers

## Usage

### Basic Example

Create an installer class and add bindings to the container:

```csharp
public class WeaponInstaller : Installer
{
    protected override void InstallBindings()
    {
        // Bind the IWeapon interface to multiple implementations
        _container.Bind<IWeapon, Sword>();
        _container.Bind<IWeapon, Gun>();
    }
}
```

Use [Inject] attribute to mark fields that should be injected:

```csharp
public class Player
{
    [Inject] public IWeapon[] _weapons;

    public void Attack()
    {
        _weapons[0].Attack();
        _weapons[1].Attack();
    }
}
```

Install and use:

```csharp
var installer = new WeaponInstaller();
var player = new Player();
installer.Install(player);
player.Attack(); // Output: Weapon1 Attack, Weapon2 Attack
```

### Advanced Usage

#### Fluent API

```csharp
// Bind with custom factory
_container.Bind<IWeapon>()
    .To<Sword>(() => new Sword())
    .WithLifetime(Lifetime.Singleton);

// Conditional binding
_container.BindConditional<IWeapon, Gun>(
    () => Application.platform == RuntimePlatform.Android
);
```

#### Automatic Registration

```csharp
[Injectable(Lifetime.Singleton)]
public class GameManager : IGameManager
{
    [Inject] private IWeapon _weapon;
    // ...
}
```

#### Unity Integration

```csharp
public class GameInstaller : MonoInstaller
{
    [SerializeField] private Player playerPrefab;

    protected override void InstallBindings()
    {
        Container.Bind<IPlayer>()
            .To<Player>(() => Instantiate(playerPrefab))
            .WithLifetime(Lifetime.Singleton);
    }
}
```

#### Extension Methods

```csharp
// Get a dependency
var service = this.GetDependency<IMyService>();

// Get a specific implementation
var specificService = this.GetDependency<IMyService>(typeof(MySpecificService));

// Bind a dependency
this.BindDependency<IMyService, MyService>();

// Bind with custom lifetime
this.BindDependency<IMyService, MyService>(Lifetime.Singleton);

// Bind with custom factory
this.BindDependency<IMyService, MyService>(() => new MyService(), Lifetime.Transient);

// Inject dependencies
var myObject = new MyClass().InjectDependencies();

// Bind a singleton instance
this.BindSingleton<IMyService>(new MyService());

// Bind a scoped instance
this.BindScoped<IMyService>(new MyService());
```

## Best Practices

1. **Use Installers**: Organize your bindings in installer classes for better maintainability
2. **Lifetime Management**: Choose appropriate lifetime scopes for your dependencies
3. **Interface-based Design**: Program to interfaces rather than concrete implementations
4. **Automatic Registration**: Use [Injectable] attribute for automatic registration when appropriate
5. **Scene Organization**: Use SceneContext for scene-specific dependencies
6. **Project-wide Dependencies**: Use ProjectContext for dependencies shared across scenes

## 🚀 About Me

Umair Saifullah ~ a unity developer from Pakistan.

## License

[MIT](https://github.com/UmairSaifullah01/SimpleDI-Unity/blob/master/LICENSE)
