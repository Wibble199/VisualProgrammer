namespace VisualProgrammer.Core
{

    public abstract class VisualStatement : VisualNode {

        [VisualNodeProperty(Label = "Next")]
        public virtual StatementReference NextStatement { get; set; }

        /// <summary>
        /// During compilation, this method will be used to get the next statement after this once.
        /// For most circumstances, the default implementation is fine, but for branching statements this will likely return
        /// the first shared node of the branches.
        /// </summary>
        public virtual StatementReference? GetCompilerNextStatement(VisualProgram context) => NextStatement;
    }
}
