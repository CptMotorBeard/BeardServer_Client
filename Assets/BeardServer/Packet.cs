using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BeardServer
{
    public class Packet : IDisposable
    {
        private List<byte>  mBuffer;
        private byte[]      mReadableBuffer;
        private int         mReadPos;

        private const int kSizeOfBool   = 1;
        private const int kSizeOfByte   = 1;
        private const int kSizeOfShort  = 2;
        private const int kSizeOfFloat  = 4;
        private const int kSizeOfInt    = 4;
        private const int kSizeOfLong   = 8;

        public Packet()
        {
            mBuffer = new List<byte>();
            mReadPos = 0;
        }

        public Packet(int id)
        {
            mBuffer = new List<byte>();
            mReadPos = 0;

            Write(id);
        }

        public Packet(byte[] data)
        {
            mBuffer = new List<byte>();
            mReadPos = 0;

            SetBytes(data);
        }

        #region Functions
        public void SetBytes(byte[] data)
        {
            Write(data);
            mReadableBuffer = mBuffer.ToArray();
        }

        // Insert the byte length of the packet at the very beginning
        public void WriteLength()
        {
            mBuffer.InsertRange(0, BitConverter.GetBytes(mBuffer.Count));
        }

        public void InsertInt(int _value)
        {
            mBuffer.InsertRange(0, BitConverter.GetBytes(_value));
        }

        public byte[] ToArray()
        {
            mReadableBuffer = mBuffer.ToArray();
            return mReadableBuffer;
        }

        public int Length()
        {
            return mBuffer.Count;
        }

        public int UnreadLength()
        {
            return Length() - mReadPos;
        }

        public void Reset(bool shouldReset = true)
        {
            if (shouldReset)
            {
                mBuffer.Clear();
                mReadableBuffer = null;
                mReadPos = 0;
            }
            else
            {
                mReadPos -= kSizeOfInt;
            }
        }
        #endregion

        #region Write Data
        public void Write(JObject data)
        {
            string stringValue = data.ToString(Newtonsoft.Json.Formatting.None);
            Write(stringValue);
        }

        public void Write(byte value)
        {
            mBuffer.Add(value);
        }

        public void Write(byte[] value)
        {
            mBuffer.AddRange(value);
        }

        public void Write(short value)
        {
            mBuffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(int value)
        {
            mBuffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(long value)
        {
            mBuffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(float value)
        {
            mBuffer.AddRange(BitConverter.GetBytes(value));
        }
        public void Write(bool value)
        {
            mBuffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(string value)
        {
            Write(value.Length);
            mBuffer.AddRange(Encoding.ASCII.GetBytes(value));
        }
        #endregion

        #region Read Data
        public JObject Read(bool moveReadPos = true)
        {
            string stringValue = ReadString(moveReadPos);
            JObject jValue = JObject.Parse(stringValue);

            return jValue;
        }

        public byte ReadByte(bool moveReadPos = true)
        {
            if (mBuffer.Count > mReadPos)
            {
                byte value = mReadableBuffer[mReadPos];
                if (moveReadPos)
                {
                    mReadPos += kSizeOfByte;
                }
                return value;
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        public byte[] ReadBytes(int length, bool moveReadPos = true)
        {
            if (mBuffer.Count > mReadPos)
            {
                byte[] value = mBuffer.GetRange(mReadPos, length).ToArray();
                if (moveReadPos)
                {
                    mReadPos += length;
                }
                return value;
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        public short ReadShort(bool moveReadPos = true)
        {
            if (mBuffer.Count > mReadPos)
            {
                short value = BitConverter.ToInt16(mReadableBuffer, mReadPos);
                if (moveReadPos)
                {
                    mReadPos += kSizeOfShort;
                }
                return value;
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        public int ReadInt(bool moveReadPos = true)
        {
            if (mBuffer.Count > mReadPos)
            {
                int value = BitConverter.ToInt32(mReadableBuffer, mReadPos);
                if (moveReadPos)
                {
                    mReadPos += kSizeOfInt;
                }
                return value;
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        public long ReadLong(bool moveReadPos = true)
        {
            if (mBuffer.Count > mReadPos)
            {
                long value = BitConverter.ToInt64(mReadableBuffer, mReadPos);
                if (moveReadPos)
                {
                    mReadPos += kSizeOfLong;
                }
                return value;
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        public float ReadFloat(bool moveReadPos = true)
        {
            if (mBuffer.Count > mReadPos)
            {
                float _value = BitConverter.ToSingle(mReadableBuffer, mReadPos);
                if (moveReadPos)
                {
                    mReadPos += kSizeOfFloat;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        public bool ReadBool(bool moveReadPos = true)
        {
            if (mBuffer.Count > mReadPos)
            {
                bool value = BitConverter.ToBoolean(mReadableBuffer, mReadPos);
                if (moveReadPos)
                {
                    mReadPos += kSizeOfBool;
                }
                return value;
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        public string ReadString(bool moveReadPos = true)
        {
            try
            {
                int length = ReadInt();
                string value = Encoding.ASCII.GetString(mReadableBuffer, mReadPos, length);
                if (moveReadPos && value.Length > 0)
                {
                    mReadPos += length;
                }
                return value;
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }
        #endregion

        #region IDisposable
        private bool mDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!mDisposed)
            {
                if (disposing)
                {
                    mBuffer = null;
                    mReadableBuffer = null;
                    mReadPos = 0;
                }

                mDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}