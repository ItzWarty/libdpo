# An Introduction to Dargon.PortableObjects

Dargon.PortableObjects is an open-source serialization library for the .net ecosystem released under Version 3 of the GNU Public License and maintained by [The Dargon Project](https://www.github.com/the-dargon-project) developer [ItzWarty](https://www.twitter.com/ItzWarty).

## NOTICE
We don't often use Dargon.PortableObjects's serializer directly - check out Dargon.PortableObject.Streams, Dargon.Courier, and Dargon.Services for useful wrappers around Dargon.PortableObjects.

# Installing Dargon.PortableObjects
Dargon.PortableObjects is released as a NuGet package via the Dargon Package Source.

* Add `https://nuget.dargon.io/` as a NuGet package source.
* Run `Install-Package Dargon.PortableObjects` from the package management console.

# Concepts
## The Portable Object
For now, all serializable objects must be common .net types (simple value types, strings, collections, dictionaries, datetimes, timespans, guids) or implementers of IPortableObject.

To exemplify, imagine this hypothetical joystick state data-transfer-object that might be sent across the wire to a remotely controlled device:

``` csharp
public class JoystickStateDto : IPortableObject { 
   public JoystickStateDto() { }
   public JoystickStateDto(int x, int y) { X = x; Y = y; }
   public int X { get; set; }
   public int Y { get; set; }
   public void Serialize(IPofWriter writer) {
      writer.WriteS32(0, X);
      writer.WriteS32(1, Y);
   }
   public void Deserialize(IPofReader reader) {
      X = reader.readS32(0);
      Y = reader.readS32(1);
   }
}
```

In the future, we'll support external serializers and this syntax:

``` csharp
[Autoserializable]
public class JoystickStateDto : IPortableObject { 
   public JoystickStateDto() { }
   public JoystickStateDto(int x, int y) { X = x; Y = y; }
   public int X { get; set; }
   public int Y { get; set; }
}
```

Note the required empty constructor - this is required by us so that we can instantiate instances of the portable type!

## The POF Context
You register your portable object types in POF Contexts!

```csharp
public class CustomPofContext : PofContext {
   private int kBasePofId = {number};
   public CustomPofContext() {
      RegisterPortableObjectType(kBasePofId, typeof(JoystickStateDto));
      // alternatively!
      RegisterPortableObjectType(kBasePofId, () => new JoystickStateDto([.. dependencies]));
   }
}
```

### Merging POF Contexts
You can combine many POF Contexts into one as follows:
```csharp
new PofContext().With(x => {
   x.MergeContext(new CustomPofContext1());
   x.MergeContext(new CustomPofContext2());
});
```

## The POF Serializer
And you can serialize/deserialize objects with the POF Serializer as follows:
``` csharp
pofSerializer = new PofSerializer(pofContext);
using (var ms = new MemoryStream()) {
   pofSerializer.Serialize(ms, joystickState);
   ms.Position = 0;
   var joystickState = pofSerializer.Deserialize<JoystickStateDto>(ms);
   // alternatively:
   ms.Position = 0;
   object something = pofSerializer.Deserialize(ms);
}
```

# Internals
## DPO Structures
When serializing a CustomClass<T1, T2>, the POF Serializer writes data 'frames' as follows:
``` csharp
struct pof_frame {
   int length;  // e.g. 132
   pof_type[] type; // e.g. { CustomClass<,>, T1, T2 }
   pof_slot[] slots;
}
```
pof_type values are essentially your registered types.

Pof slots look as follows:
``` csharp
struct pof_slot {
   int slot_count;
   int[] slot_lengths;
   byte[][] slot_datas;
}
```