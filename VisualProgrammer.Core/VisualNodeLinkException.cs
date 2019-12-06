using System;

namespace VisualProgrammer.Core {

    /// <summary>
    /// Exception that is thrown when there is an error while attempting to link two nodes together.
    /// </summary>
    public class VisualNodeLinkException : Exception {
        public VisualNodeLinkException(string message) : base(message) { }
    }
}
