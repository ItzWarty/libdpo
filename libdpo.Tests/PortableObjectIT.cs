using System.Linq;
using ItzWarty;
using ItzWarty.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dargon.PortableObjects.Tests
{
   [TestClass]
   public class PortableObjectIT : MockitoLike
   {
      private readonly PofContext context;

      public PortableObjectIT()
      {
         context = new PofContext();
         context.RegisterPortableObjectType(0x11223344, typeof(PersonKey));
         context.RegisterPortableObjectType(0x55667788, typeof(PersonEntry));
         context.RegisterPortableObjectType(0x778899AA, typeof(RemovalByLevelThresholdProcessor));
         context.RegisterPortableObjectType(0x30291dea, typeof(FriendClearingProcessor));
      }

      [TestMethod]
      public void RunTest()
      {
         const string name1 = "Henry has a first name!";
         const string name2 = "Larry doesn't have a last name!";
         var key1 = new PersonKey(name1);
         var key2 = new PersonKey(name2);

         var personEntry1 = new PersonEntry(key1, 10);
         var personEntry2 = new PersonEntry(key2, 5);

         var thresholdsByKey = new Dictionary<PersonKey, int>();
         thresholdsByKey.Add(key1, 30);
         thresholdsByKey.Add(key2, 2);

         var levelRemovalProcessor = new RemovalByLevelThresholdProcessor(thresholdsByKey);
         var friendClearingProcessor = new FriendClearingProcessor();

         var serializer = new PofSerializer(context);
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
               serializer.Serialize(writer, levelRemovalProcessor);
               serializer.Serialize(writer, friendClearingProcessor);
            }
            ms.Position = 0;
            Console.WriteLine(ms.ToArray().ToHex());
            using (var reader = new BinaryReader(ms)) {
               levelRemovalProcessor = serializer.Deserialize<RemovalByLevelThresholdProcessor>(reader);
               friendClearingProcessor = serializer.Deserialize<FriendClearingProcessor>(reader);
            }
         }

         var entry1 = new Entry<PersonKey, PersonEntry>(key1, personEntry1);
         var entry2 = new Entry<PersonKey, PersonEntry>(key2, personEntry2);

         friendClearingProcessor.Process(entry1);
         friendClearingProcessor.Process(entry2);

         levelRemovalProcessor.Process(entry1);
         levelRemovalProcessor.Process(entry2);

         assertTrue(entry1.IsPresent());
         assertFalse(entry2.IsPresent());
      }

      public class Entry<TKey, TValue> : IPortableObject
      {
         private TKey key;
         private TValue value;
         private bool isPresent;

         public Entry() { }

         public Entry(TKey key, TValue value, bool isPresent = true)
         {
            this.key = key;
            this.value = value;
            this.isPresent = isPresent;
         }

         public TKey Key { get { return key; } }
         public TValue Value { get { return value; } }

         public void Serialize(IPofWriter writer)
         {
            int i = 0;
            writer.WriteObject(i++, key);
            writer.WriteObject(i++, value);
            writer.WriteBoolean(i++, isPresent);
         }

         public void Deserialize(IPofReader reader)
         {
            int i = 0;
            key = reader.ReadObject<TKey>(i++);
            value = reader.ReadObject<TValue>(i++);
            isPresent = reader.ReadBoolean(i++);
         }

         public void Remove()
         {
            isPresent = false;
            value = default(TValue);
         }

         public bool IsPresent() { return isPresent; }
      }

      public class PersonKey : IPortableObject
      {
         private string name;

         public PersonKey() { }
         public PersonKey(string name) { this.name = name; }

         public void Serialize(IPofWriter writer) { writer.WriteString(0, name); }
         public void Deserialize(IPofReader reader) { name = reader.ReadString(0); }

         public override bool Equals(object obj)
         {
            var asKey = obj as PersonKey;
            if (asKey == null) return false;
            else return name.Equals(asKey.name);
         }

         public override int GetHashCode()
         {
            return 17 * name.GetHashCode();
         }

         public override string ToString() { return "[Key " + name + "]"; }
      }

      public class PersonFriend : IPortableObject
      {
         private ulong id;
         private string name;

         public void Serialize(IPofWriter writer)
         {
            int i = 0;
            writer.WriteU64(i++, id);
            writer.WriteString(i++, name);
         }

         public void Deserialize(IPofReader reader)
         {
            int i = 0;
            id = reader.ReadU64(i++);
            name = reader.ReadString(i++);
         }
      }

      public class PersonEntry : IPortableObject
      {
         private PersonKey key;
         private int level;
         private List<PersonFriend> friends;

         public PersonEntry() { }

         public PersonEntry(PersonKey key, int level)
         {
            this.key = key;
            this.level = level;
            this.friends = new List<PersonFriend>();
         }
         
         public PersonKey Key { get { return key; } }
         public int Level { get { return level; } }
         public List<PersonFriend> Friends { get { return friends; } }

         public void Serialize(IPofWriter writer)
         {
            int i = 0;
            writer.WriteObject(i++, key);
            writer.WriteS32(i++, level);
            writer.WriteArray(i++, friends.ToArray());
         }

         public void Deserialize(IPofReader reader)
         {
            int i = 0;
            key = reader.ReadObject<PersonKey>(i++);
            level = reader.ReadS32(i++);
            friends = new List<PersonFriend>(reader.ReadArray<PersonFriend>(i++));
         }
      }

      public class RemovalByLevelThresholdProcessor : IProcessor<PersonKey, PersonEntry>, IPortableObject
      {
         private IDictionary<PersonKey, int> thresholdByPerson;

         public RemovalByLevelThresholdProcessor() { }
         public RemovalByLevelThresholdProcessor(Dictionary<PersonKey, int> thresholdsByPerson) { this.thresholdByPerson = thresholdsByPerson; }

         public void Process(Entry<PersonKey, PersonEntry> entry)
         {
            int levelThreshold;
            if (thresholdByPerson.TryGetValue(entry.Key, out levelThreshold)) {
               if (entry.Value.Level > levelThreshold) {
                  entry.Remove();
               }
            }
         }

         public void Serialize(IPofWriter writer) { writer.WriteMap(0, thresholdByPerson); }
         public void Deserialize(IPofReader reader) { thresholdByPerson = reader.ReadMap<PersonKey, int>(0); }

         public override string ToString() { return "[RemovalByLevelThresholdProcessor ThresholdByPerson={0}]".F(thresholdByPerson.Select((kvp) => kvp.Key + ":" + kvp.Value).Join(",")); }
      }

      private interface IProcessor<TKey, TValue>
      {
         void Process(Entry<TKey, TValue> entry);
      }

      public class FriendClearingProcessor : IProcessor<PersonKey, PersonEntry>, IPortableObject
      {
         public void Process(Entry<PersonKey, PersonEntry> entry)
         {
            var friends = entry.Value.Friends;
            Console.WriteLine(entry.Value + " " + (friends == null));
            friends.Clear();
         }

         public void Serialize(IPofWriter writer) { }

         public void Deserialize(IPofReader reader) { }
      }
   }
}
