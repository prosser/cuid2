# prosser/Cuid2

C# implementation of the [paralleldrive/cuid2](https://github.com/paralleldrive/cuid2) NPM package.

Makes use of a local implementation of OnixLabs.Security.Cryptography.Sha3 to permit use on pre-net7.0 systems. 

To generate a new CUID2 id:

```csharp
string id = Cuid2.CreateId();
```

To generate a CUID2 id of a different length (2 - 32 characters are valid):

```csharp
string id = Cuid2.CreateId(10);
```

To customize the generation:

```csharp
// The Cuid2.Init method returns a custom CreateId function with the specified
// configuration. All configuration properties are optional.
Func<string> createId = Cuid2.Init(
  // An implementation of System.Security.Cryptography.RandomNumberGenerator
  random: RandomNumberGenerator.Create(),
  // the length of the id
  length: 10,
  // A custom fingerprint for the host environment. This is used to help
  // prevent collisions when generating ids in a distributed system.
  fingerprint: 'a-custom-host-fingerprint',
});

Console.WriteLine($"{createId()} {createId()} {createId()}");
```