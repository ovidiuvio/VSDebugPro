using System;
using System.Collections.Generic;

namespace VSDebugCoreLib.Console
{
    /// <summary>
    ///     Implements the buffer used for the history in the console window.
    /// </summary>
    public class HistoryBuffer
    {
        // Default size of the buffer. This is the size used when the buffer is
        // built without any parameter.
        internal const int DefaultBufferSize = 500;

        // The array of strings that stores the history of the commands.

        // Current position inside the buffer.
        private int _currentPosition;

        // Flag true if the current item was returned.
        private bool _currentReturned;

        /// <summary>
        ///     Creates an HistoryBuffer object with the default size.
        /// </summary>
        public HistoryBuffer() :
            this(DefaultBufferSize)
        {
        }

        /// <summary>
        ///     Creates a HistoryBuffer object of a specifuc size.
        /// </summary>
        public HistoryBuffer(int bufferSize)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException("bufferSize");
            Cmds = new List<string>(bufferSize);
        }

        public List<string> Cmds { get; }

        /// <summary>
        ///     Search in the buffer if there is an item and returns its index.
        ///     Returns -1 if the item is not in the buffer.
        /// </summary>
        private int FindIndex(string entry)
        {
            for (var i = 0; i < Cmds.Count; i++)
                if (string.CompareOrdinal(entry, Cmds[i]) == 0)
                    return i;
            return -1;
        }

        /// <summary>
        ///     Add a new entry in the list.
        /// </summary>
        public void AddEntry(string entry)
        {
            // Check if this entry is in the buffer.
            var index = FindIndex(entry);
            _currentReturned = false;
            if (-1 != index)
                Cmds.RemoveAt(index);
            else if (Cmds.Count == Cmds.Capacity) Cmds.RemoveAt(0);

            // Add the new entry at the end of the buffer.
            Cmds.Add(entry);

            // Set the current position at the end of the buffer.
            _currentPosition = Cmds.Count - 1;
        }

        /// <summary>
        ///     Returns the previous element in the history or null if there is no
        ///     previous entry.
        /// </summary>
        public string PreviousEntry()
        {
            if (Cmds.Count == 0 || _currentPosition < 0) return null;
            if (!_currentReturned)
            {
                _currentReturned = true;
                return Cmds[_currentPosition];
            }

            _currentPosition -= 1;
            if (_currentPosition < 0)
            {
                _currentPosition = 0;
                return null;
            }

            return Cmds[_currentPosition];
        }

        /// <summary>
        ///     Return the next entry in the history or null if there is no entry.
        /// </summary>
        public string NextEntry()
        {
            if (_currentPosition >= Cmds.Count - 1) return null;
            _currentReturned = true;
            _currentPosition += 1;
            return Cmds[_currentPosition];
        }
    }
}