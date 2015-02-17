using ItzWarty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Dargon.PortableObjects {
   public unsafe class PofReader : IPofReader
   {
      private readonly IPofContext context;
      private readonly ISlotSource slots;

      private static readonly Dictionary<Type, Func<BinaryReader, object>> RESERVED_TYPE_READERS = new Dictionary<Type, Func<BinaryReader, object>> {
         {typeof(void), reader => null},
         {typeof(sbyte), (reader) => reader.ReadSByte()},
         {typeof(byte), (reader) => reader.ReadByte()},
         {typeof(short), (reader) => reader.ReadInt16()},
         {typeof(ushort), (reader) => reader.ReadUInt16()},
         {typeof(int), (reader) => reader.ReadInt32()},
         {typeof(uint), (reader) => reader.ReadUInt32()},
         {typeof(long), (reader) => reader.ReadInt64()},
         {typeof(ulong), (reader) => reader.ReadUInt64()},
         {typeof(char), (reader) => reader.ReadChar()},
         {typeof(string), (reader) => reader.ReadNullTerminatedString()},
         {typeof(bool), (reader) => reader.ReadByte() != 0 },
         {typeof(Guid), (reader) => reader.ReadGuid() },
         {typeof(DateTime), (reader) => DateTime.FromBinary(reader.ReadInt64()).ToUniversalTime() },
         {typeof(byte[]), (reader) => reader.ReadBytes(reader.ReadInt32()) },
      };

      public PofReader(IPofContext context, ISlotSource slots)
      {
         this.context = context;
         this.slots = slots;
      }

      public IPofContext Context { get { return context; } }

      public sbyte ReadS8(int slot)
      {
         var value = slots[slot][0];
         return *(sbyte*)&value;
      }

      public byte ReadU8(int slot) { return slots[slot][0]; }
      public short ReadS16(int slot) { return BitConverter.ToInt16(slots[slot], 0); }
      public ushort ReadU16(int slot) { return BitConverter.ToUInt16(slots[slot], 0); }
      public int ReadS32(int slot) { return BitConverter.ToInt32(slots[slot], 0); }
      public uint ReadU32(int slot) { return BitConverter.ToUInt32(slots[slot], 0); }
      public long ReadS64(int slot) { return BitConverter.ToInt64(slots[slot], 0); }
      public ulong ReadU64(int slot) { return BitConverter.ToUInt64(slots[slot], 0); }
      public float ReadFloat(int slot) { return BitConverter.ToSingle(slots[slot], 0); }
      public double ReadDouble(int slot) { return BitConverter.ToDouble(slots[slot], 0); }
      public char ReadChar(int slot) { return BitConverter.ToChar(slots[slot], 0); }

      public string ReadString(int slot)
      {
         using (var reader = GetSlotBinaryReader(slot)) {
            return reader.ReadNullTerminatedString();
         }
      }

      public bool ReadBoolean(int slot) { return slots[slot][0] != 0; }
      public Guid ReadGuid(int slot) { return new Guid(slots[slot]); }
      public DateTime ReadDateTime(int slot) { return DateTime.FromBinary(BitConverter.ToInt64(slots[slot], 0)).ToUniversalTime(); }
      public byte[] ReadBytes(int slot) { return slots[slot]; }

      public T ReadObject<T>(int slot) { return (T)ReadObject(slot); }
      public T ReadObjectTypeless<T>(int slot) { return (T)ReadObjectTypeless(slot, typeof(T)); }

      public object ReadObject(int slot) {
         using (var reader = GetSlotBinaryReader(slot)) {
            return ReadObjectInternal(reader, null);
         }
      }

      public object ReadObjectTypeless(int slot, Type type) {
         using (var reader = GetSlotBinaryReader(slot)) {
            return ReadObjectInternal(reader, type);
         }
      }

      private object ReadObjectInternal(BinaryReader reader, Type type) 
      {
         type = type ?? ParseType(reader);
         if (type == typeof(void)) {
            return null;
         } else if (type == typeof(IEnumerable)) {
            return ReadArrayInternal<object>(reader, false);
         } else {
            return ReadObjectWithoutTypeDescription(type, reader);
         }
      }

      private object ReadObjectWithoutTypeDescription(Type type, BinaryReader reader)
      {
         if (context.IsReservedType(type)) {
            return ReadReservedType(type, reader);
         } else {
            var instance = context.CreateInstance(type);
            instance.Deserialize(new PofReader(context, SlotSourceFactory.CreateFromBinaryReader(reader)));
            if (instance is SpecialTypes.Base) {
               return ((SpecialTypes.Base)instance).Unwrap();
            } else {
               return instance;
            }
         }
      }

      private object ReadReservedType(Type type, BinaryReader reader) { return RESERVED_TYPE_READERS[type](reader); }

      public T[] ReadArray<T>(int slot, bool elementsPolymorphic = false)
      {
         using (var stream = CreateSlotMemoryStream(slot))
         using (var reader = new BinaryReader(stream, Encoding.UTF8, true)) {
            return ReadArrayInternal<T>(reader, true);
         }
      }

      private T[] ReadArrayInternal<T>(BinaryReader reader, bool readEnumerableTypeHeader) {
         return (T[])ReadObjectInternal(reader, readEnumerableTypeHeader ? null : typeof(SpecialTypes.PortableArray<T>));
         //         if (readEnumerableTypeHeader) {
         //            ParseType(reader); // throwaway, equals IEnumerable
         //         }
         //         var type = ParseType(reader);
         //         int length = reader.ReadInt32();
         //         Trace.Assert(typeof(T).IsAssignableFrom(type));
         //
         //         var expectedType = elementsPolymorphic ? null : type;
         //         return Util.Generate(length, i => (T)ReadObjectInternal(reader, expectedType));
      }

      public TCollection ReadCollection<T, TCollection>(int slot, bool elementsPolymorphic = false) where TCollection : class, ICollection<T>, new() {
         return ReadCollection<T, TCollection>(slot, new TCollection(), elementsPolymorphic);
      }

      public TCollection ReadCollection<T, TCollection>(int slot, TCollection collection, bool elementsPolymorphic = false) where TCollection : class, ICollection<T> {
         using (var stream = CreateSlotMemoryStream(slot))
         using (var reader = new BinaryReader(stream, Encoding.UTF8, true)) {
            return ReadCollectionInternal<T, TCollection>(reader, collection, elementsPolymorphic, true);
         }
      }

      private TCollection ReadCollectionInternal<T, TCollection>(BinaryReader reader, TCollection collection, bool elementsPolymorphic, bool readIenumerableTypeHeader) where TCollection : class, ICollection<T> {
         var elements = ReadArrayInternal<T>(reader, readIenumerableTypeHeader);
         foreach (var element in elements) {
            collection.Add(element);
         }
         return collection;
      }

      public IDictionary<TKey, TValue> ReadMap<TKey, TValue>(int slot, bool keysPolymorphic = false, bool valuesPolymorphic = false, IDictionary<TKey, TValue> dict = null) {
         using (var stream = CreateSlotMemoryStream(slot))
         using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
         {
            return ReadMapInternal(keysPolymorphic, valuesPolymorphic, dict, reader, true);
         }
      }

      private IDictionary<TKey, TValue> ReadMapInternal<TKey, TValue>(bool keysPolymorphic, bool valuesPolymorphic, IDictionary<TKey, TValue> dict, BinaryReader reader, bool readDictionaryHeader) {
         var readDictionary = (IDictionary<TKey, TValue>)ReadObjectInternal(reader, readDictionaryHeader ? null : typeof(SpecialTypes.PortableMap<TKey, TValue>));
         if (dict == null) {
            return readDictionary;
         } else {
            foreach (var kvp in readDictionary) {
               dict.Add(kvp);
            }
            return dict;
         }
      }

      private BinaryReader GetSlotBinaryReader(int slot)
      {
         return new BinaryReader(CreateSlotMemoryStream(slot));
      }

      private MemoryStream CreateSlotMemoryStream(int slot)
      {
         return new MemoryStream(slots[slot]);
      }

      private Type ParseType(BinaryReader reader)
      {
         var typeDescription = ParseTypeDescription(reader);
         return context.GetTypeFromDescription(typeDescription);
      }

      private PofTypeDescription ParseTypeDescription(BinaryReader reader)
      {
         int typeId = reader.ReadInt32();
         var type = context.GetTypeOrNull(typeId);
         if (type == null)
            throw new TypeIdNotFoundException(typeId);
         if (!type.IsGenericTypeDefinition)
            return new PofTypeDescription(new[] { type });
         else
         {
            var genericArgumentSpecifications = type.GetGenericArguments();
            var genericArguments = Util.Generate(
               genericArgumentSpecifications.Length,
               i => ParseType(reader));
            return new PofTypeDescription(Util.Concat<Type>(type, genericArguments));
         }
      }
   }
}