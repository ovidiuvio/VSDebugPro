using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSDebugCoreLib
{
    public class BreakpointCommandAssociation
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string Command { get; set; }
        public bool Enabled { get; set; }
        public bool Continue { get; set; }
    }

    public class BreakpointCommandManager
    {
        private List<BreakpointCommandAssociation> _associations = new List<BreakpointCommandAssociation>();

        public void AddAssociation(string filePath, int lineNumber, string command, bool autoStep)
        {
            _associations.Add(new BreakpointCommandAssociation
            {
                FilePath = filePath,
                LineNumber = lineNumber,
                Command = command,
                Continue = autoStep,
                Enabled = true
            });
        }

        public string GetCommandForBreakpoint(string filePath, int lineNumber)
        {
            return _associations.FirstOrDefault(a =>
                a.FilePath == filePath &&
                a.LineNumber == lineNumber)?.Command;
        }

        public BreakpointCommandAssociation GetAssociation(string filePath, int lineNumber)
        {
            return _associations.FirstOrDefault(a =>
                a.FilePath == filePath &&
                a.LineNumber == lineNumber);
        }

        public List<BreakpointCommandAssociation> GetAllAssociations()
        {
            return new List<BreakpointCommandAssociation>(_associations);
        }
    }
}
