using System;
using System.IO;
using System.Linq.Expressions;
using ItzWarty;
using ItzWarty.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dargon.PortableObjects.Tests
{
   [TestClass]
   public unsafe class PofReaderTest : MockitoLike
   {
      private const int SLOT_INDEX = 123;

      private PofReader testObj;
      
      [Mock] private readonly IPofContext context;
      [Mock] private readonly ISlotSource slotSource;

      [TestInitialize]
      public void Setup()
      {
         InitializeMocks();
         testObj = new PofReader(context, slotSource);
      }

      [TestMethod]
      public void TestReadS8()
      {
         sbyte value = -123;
         var data = new byte[] { *(byte*)&value };
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadS8(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadU8()
      {
         const byte value = 123;
         var data = new byte[] { value };
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadU8(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadS16()
      {
         const short value = -12356;
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadS16(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadU16()
      {
         const ushort value = 58692;
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadU16(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadS32()
      {
         const int value = int.MinValue;
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadS32(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadU32()
      {
         const uint value = uint.MaxValue;
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadU32(SLOT_INDEX));
      }


      [TestMethod]
      public void TestReadS64()
      {
         const long value = long.MinValue;
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadS64(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadU64()
      {
         const ulong value = ulong.MaxValue;
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadU64(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadFloat()
      {
         const float value = 13.37f;
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadFloat(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadDouble()
      {
         const double value = 13333.333337;
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadDouble(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadChar()
      {
         const char value = 'a';
         var data = BitConverter.GetBytes(value);
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadChar(SLOT_INDEX));
      }

      [TestMethod]
      public void TestReadString()
      {
         const string value = "There is no spoon!";
         byte[] data;
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
               writer.WriteNullTerminatedString(value);
            }
            data = ms.ToArray();
         }
         when(() => slotSource[SLOT_INDEX]).ThenReturn(data);
         assertEquals(value, testObj.ReadString(SLOT_INDEX));
      }
   }
}
