namespace VisualProgrammer.Core
{

    public abstract class VisualStatement : VisualNode {

        [VisualNodeStatementProperty(Label = "Next")]
        public virtual NodeReference? NextStatement { get; set; }

        /// <summary>
        /// During compilation, this method will be used to get the next statement after this once.
        /// For most circumstances, the default implementation is fine, but for branching statements this will likely return
        /// the first shared node of the branches.
        /// </summary>
        public virtual NodeReference? GetCompilerNextStatement(VisualProgram context) => NextStatement;
    }
}
