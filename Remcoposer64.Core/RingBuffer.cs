namespace Remcoposer64.Core
{
    public class RingBuffer<T>
    {
        public readonly T[] buffer;
        private readonly int capacity;

        private int writeIndex = 0;
        private int readIndex = 0;

        public int OverflowCount { get; private set; } = 0;

        public RingBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = new T[capacity];
        }

        /// <summary>
        /// RmTimer（リアルタイム側）から呼ばれる。絶対にブロックしない。
        /// </summary>
        public void Push(T item)
        {
            int next = (writeIndex + 1) % capacity;

            // バッファ満杯 → 古いデータを捨てる（リアルタイム優先）
            if (next == readIndex)
            {
                OverflowCount++;
                readIndex = (readIndex + 1) % capacity;
            }

            buffer[writeIndex] = item;
            writeIndex = next;
        }

        /// <summary>
        /// 送信側から呼ばれる。データが無ければ false。
        /// </summary>
        public bool Pop(out T item)
        {
            if (readIndex == writeIndex)
            {
                item = default!;
                return false;
            }

            item = buffer[readIndex];
            readIndex = (readIndex + 1) % capacity;
            return true;
        }

        public bool Peek(out T item)
        {
            if (readIndex == writeIndex)
            {
                item = default!;
                return false;
            }

            item = buffer[readIndex];
            return true;
        }

        public void Advance()
        {
            if (readIndex != writeIndex)
            {
                readIndex = (readIndex + 1) % capacity;
            }
        }

        public void Clear()
        {
            writeIndex = 0;
            readIndex = 0;
            OverflowCount = 0;
        }

        public int Count
        {
            get
            {
                if (writeIndex >= readIndex)
                    return writeIndex - readIndex;
                else
                    return (capacity - readIndex) + writeIndex;
            }
        }
    }
}
