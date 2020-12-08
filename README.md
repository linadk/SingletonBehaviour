# Singleton Monobehaviour

This SingletonBehaviour class is a Singleton implementation of the Unity3D Monobehaviour that is unit tested and thread-safe.

## Features

* Thread safe double lock on instance access
* Obeys OnApplicationQuit and editor flow to avoid creating phantom objects
* Fully unit tested
* Built on the latest LTS Unity
* Works well with Unity's new playmodes

## Usage

New classes are created with :

```csharp
    public class YourClass : SingletonBehaviour<YourClass>
    {
        public int playerLives = 2;
        // ... Your methods and variables here ...
    }
```

New classes can be utilized with :

```csharp
    YourClass.Instance.playerLives -= 1;
```

If you want your Singleton to persist across scene loads:
```csharp
    YourClass.Persistent = true;
```

## Caveats
* The first time you call YourClass.Instance, do it from the main thread.
  * If you don't understand threads this is likely not something you will deal with.
* *OnApplicationQuit()* has been sealed to control *Instance* access.
* Override *OnSingletonApplicationQuit()* instead for the same experience.

## Performance - Todo
Some dislike lock checking as it can incur a performance penalty. To keep things simple I've only tested this against the most 'popular' method I've seen in use.

Text output

You can find more data about how the test was constructed in the Tests folder.

## Dependencies

Built and Tested Using
* Unity 2019.4.14.1
* .Net 2.0 Subset
* Todo : Test more platforms

## Installing

* How/where to download your program
* Any modifications needed to be made to files/folders

## Contributions & Sources

[C# In Depth Singleton Implementations](https://csharpindepth.com/articles/singleton)
[Stack Exchange Thread(2016) - Various Authors](https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern)

## Version History

* 0.1
    * Initial Release

## License

This project is licensed under the MIT License - see the LICENSE.md file for details