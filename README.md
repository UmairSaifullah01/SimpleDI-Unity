
# Simple Dependency Injection For Unity
 
A Unity dependency injection container for managing bindings, resolving, and injecting dependencies..


## Installation

This library is distributed via Unity's built-in package manager. Required Unity 2018.3 or later.

```
https://github.com/UmairSaifullah01/SimpleDI-Unity.git

```

### Unity Package
- Open Unity project
- Download and run .unitypackage file from the latest release  
## Usage

![SimpleDI](https://raw.githubusercontent.com/UmairSaifullah01/Images/master/SimpleDI.jpg)
## Example

Create a installer class and add binding to container 
```csharp
public class WeaponInstaller : Installer
    {
    	
    	protected override void InstallBindings()
    	{
    		// Bind the IWeapon interface to the Sword implementation
            // Bind the IWeapon interface to the Gun implementation
    		 _container.Bind<IWeapon, Sword>();
    		 _container.Bind<IWeapon, Gun>();
    		
    	}
    
    }
```
Use Inject PropertyAttribute to mark field that should inject in class
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

Use Installer to install all bindings to inject marked fields

```csharp
    var installer = new WeaponInstaller();
    var player    = new Player();
    installer.Install(player);
    player.Attack(); // Output Weapon1 Attack, Weapon2 Attack
```
## Features

- Binding Interfaces: Allows binding interfaces to specific implementations with optional factory methods and singleton settings.
- Dependency Resolution: Resolves instances of dependencies, supporting both singleton and non-singleton instances.
- Injection: Injects dependencies into fields marked with the [Inject] attribute.
- Multiple Implementations: Supports resolving specific implementations of an interface.
- Singleton Management: Manages singleton instances to ensure only one instance is created and reused.


## 🚀 About Me
Umair Saifullah ~ a unity developer from Pakistan.


## License

[MIT](https://github.com/UmairSaifullah01/SimpleDI-Unity/blob/master/LICENSE)

