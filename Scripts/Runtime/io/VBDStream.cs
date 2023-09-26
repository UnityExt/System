using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityExt.Core;

namespace UnityExt.Sys { 

    /// <summary>
    /// Class that abstracts a stream of data encoded following block descriptions from the stream.
    /// </summary>
    public abstract class VBDStream<T> : IDisposable  {

        #region class Block

        /// <summary>
        /// Base class that represents a data block to be encoded in the variable byte stream.
        /// </summary>        
        public class Block {
        
            /// <summary>
            /// Minimum Value of the Range
            /// </summary>
            public T min { get; protected set; }

            /// <summary>
            /// Maximum Value of the Range
            /// </summary>
            public T max { get; protected set; }

            /// <summary>
            /// Bits of precision for the data range.
            /// </summary>
            public int bits { get; protected set; }

            /// <summary>
            /// Internal
            /// </summary>
            protected T m_range_max;

            /// <summary>
            /// Creates a new block instance.
            /// </summary>
            /// <param name="p_min">Minimum value from the block range</param>
            /// <param name="p_max">Maximum value from the block range</param>
            /// <param name="p_bits">Bits of precision to encode the range.</param>
            public Block(T p_min,T p_max,int p_bits=0) {
                double rng=0f;
                double vmin;
                double vmax;
                double v0=0f;
                double v1=0f;
                if(typeof(T) == typeof(float)) {
                    v0 = (float)(object)p_min;
                    v1 = (float)(object)p_max;                    
                }
                if(typeof(T) == typeof(double)) {
                    v0 = (double)(object)p_min;
                    v1 = (double)(object)p_max;                    
                }
                vmax    = v1<v0 ? v0 : v1;
                vmin    = v1<v0 ? v1 : v0;
                rng     = vmax-vmin;
                bits    = p_bits>0 ? p_bits : BitStream.GetMSB((ulong)rng)+1;
                if(bits<=0) bits = 1;
                m_range_max = (T)(object)((1<<bits)-1);
            }
        
        }

        #endregion

        /// <summary>
        /// Reference to the underlying bit stream being used.
        /// </summary>
        public BitStream BitStream { get; private set; }

        /// <summary>
        /// Current position, measured in data blocks.
        /// </summary>
        public long Position {
            get { return m_position; }
        }
        private long m_position;

        /// <summary>
        /// Length of data blocks written in this stream.
        /// </summary>
        public long Length {
            get { return m_length; }
        }
        private long m_length;

        /// <summary>
        /// List of block data descriptior.
        /// </summary>
        public List<Block> Blocks { get { return m_blocks; } }
        private List<Block> m_blocks;

        /// <summary>
        /// Given a numeric value, returns the most closely associated block or null.
        /// </summary>
        /// <param name="p_value">Value to search inside blocks and their range.</param>
        /// <returns></returns>
        public Block GetBlockByValue(T p_value) {
            return null;
        }

        /// <summary>
        /// Returns how many bits are needed to encode the block index.
        /// </summary>
        public int BitsPerBlock { get { return BitStream.GetMSB((ulong)Blocks.Count); } }

        #region Flags

        /// <summary>
        /// Flag that tells there is a stream available.
        /// </summary>
        public bool HasStream { get { return m_has_stream; } }
        private bool m_has_stream;

        /// <summary>
        /// Flag that tells this stream can be written.
        /// </summary>
        public bool CanWrite { get { return m_can_write; } }
        private bool m_can_write;

        /// <summary>
        /// Flag that tells this stream can be read.
        /// </summary>
        public bool CanRead  { get { return m_can_read;  } }
        private bool m_can_read;

        /// <summary>
        /// Flag that tells this stream can be seeked.
        /// </summary>
        public bool CanSeek  { get { return m_can_seek;  } }
        private bool m_can_seek;

        #endregion

        #region CTOR

        /// <summary>
        /// Internal CTOR.
        /// </summary>
        private VBDStream() {
            m_has_stream    = false;
            m_can_write     = false;
            m_can_read      = false;
            m_can_seek      = false;      
            m_blocks        = new List<Block>();
        }

        /// <summary>
        /// Creates a new variable byte data stream passing the desired stream. A new bitstream will be created using the passed stream.
        /// </summary>
        /// <param name="p_stream">Stream to read/write</param>
        /// <param name="p_bit_offset">Bit offset of the underlying bit stream.</param>
        public VBDStream(Stream p_stream,long p_bit_offset) : this(new BitStream(p_stream,p_bit_offset)) { }

        /// <summary>
        /// Creates a new variable byte data stream passing a custom bit stream.
        /// </summary>
        /// <param name="p_stream">Stream to read/write</param>        
        public VBDStream(BitStream p_stream) : this() {
            BitStream = p_stream;
            m_has_stream = BitStream!=null;
            if(m_has_stream) {
                m_can_read  = BitStream.CanRead;
                m_can_write = BitStream.CanWrite;
                m_can_seek  = BitStream.CanSeek;
            }
            m_position = 0;
        }

        #endregion

        /// <summary>
        /// Writes a new value in the stream.
        /// </summary>
        /// <param name="p_value"></param>
        /// <returns></returns>
        public long Write(T p_value) {
            if(!m_has_stream) return 0;
            long b0 = BitStream.BitPosition;

            long b1 = BitStream.BitPosition;
            return b1-b0;
        }

        /// <summary>
        /// Flushes the stream.
        /// </summary>
        public void Flush() {            
            if(!m_has_stream) return;
            BitStream.Flush();
        }

        /// <summary>
        /// Async flushes the stream.
        /// </summary>
        public async Task FlushAsync() {
            if(!m_has_stream) return;            
            await BitStream.FlushAsync();
        }

        /// <summary>
        /// Disposes all resources.
        /// </summary>
        public void Dispose() {            
            m_position=0;
            if(m_has_stream) BitStream.Dispose();
        }

        /// <summary>
        /// Returns the written data as string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            if(m_sb==null) m_sb = new StringBuilder();            
            return m_sb.ToString();
        }
        private StringBuilder m_sb;

    }
}