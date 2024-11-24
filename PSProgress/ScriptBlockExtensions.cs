using System.Management.Automation;

namespace PSProgress
{
    /// <summary>
    /// Defines helper methods for script blocks.
    /// </summary>
    public static class ScriptBlockExtensions
    {
        /// <summary>
        /// Invokes a script block with the <c>$_</c> automatic variable set to the first argument.
        /// </summary>
        /// <param name="scriptBlock">The script block to invoke.</param>
        /// <param name="args">The arguments to pass to the script block.</param>
        /// <returns>The result of the script block.</returns>
        public static object InvokeInline(this ScriptBlock scriptBlock, params object[] args)
        {
            return ScriptBlock.Create("$_ = $args[0]; " + scriptBlock.ToString()).InvokeReturnAsIs(args);
        }
    }
}
