using ItzWarty;
using NMockito;
using System;
using System.IO;
using Xunit;

namespace Dargon.PortableObjects.Tests
{
   public unsafe class PofReaderTest : NMockitoInstance
   {
      private readonly PofReader testObj;
      
      [Mock] private readonly IPofContext context = null;
      [Mock] private readonly ISlotSource slotSource = null;

      private const int kSlotIndex = 123;

      public PofReaderTest()
      {
         testObj = new PofReader(context, slotSource);
      }

      [Fact]
      public void ReadS8Test()
      {
         sbyte value = -123;
         var data = new byte[] { *(byte*)&value };
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadS8(kSlotIndex));
      }

      [Fact]
      public void ReadU8Test()
      {
         const byte value = 123;
         var data = new byte[] { value };
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadU8(kSlotIndex));
      }

      [Fact]
      public void TestReadS16()
      {
         const short value = -12356;
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadS16(kSlotIndex));
      }

      [Fact]
      public void ReadU16Test()
      {
         const ushort value = 58692;
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadU16(kSlotIndex));
      }

      [Fact]
      public void ReadS32Test()
      {
         const int value = int.MinValue;
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadS32(kSlotIndex));
      }

      [Fact]
      public void ReadU32Test()
      {
         const uint value = uint.MaxValue;
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadU32(kSlotIndex));
      }


      [Fact]
      public void ReadS64Test()
      {
         const long value = long.MinValue;
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadS64(kSlotIndex));
      }

      [Fact]
      public void ReadU64Test()
      {
         const ulong value = ulong.MaxValue;
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadU64(kSlotIndex));
      }

      [Fact]
      public void ReadFloatTest()
      {
         const float value = 13.37f;
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadFloat(kSlotIndex));
      }

      [Fact]
      public void ReadDoubleTest()
      {
         const double value = 13333.333337;
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadDouble(kSlotIndex));
      }

      [Fact]
      public void ReadCharTest()
      {
         const char value = 'a';
         var data = BitConverter.GetBytes(value);
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadChar(kSlotIndex));
      }

      [Fact]
      public void ReadStringTest()
      {
         const string value = "There is no spoon!";
         byte[] data;
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
               writer.WriteNullTerminatedString(value);
            }
            data = ms.ToArray();
         }
         When(slotSource[kSlotIndex]).ThenReturn(data);
         AssertEquals(value, testObj.ReadString(kSlotIndex));
      }
   }
}
