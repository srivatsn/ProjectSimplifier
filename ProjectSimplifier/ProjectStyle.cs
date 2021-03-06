﻿namespace ProjectSimplifier
{
    internal enum ProjectStyle
    {
        /// <summary>
        /// The project has an import of Common.props and CSharp.targets. 
        /// </summary>
        Default,

        /// <summary>
        /// The project imports props and targets but not the default ones. 
        /// </summary>
        DefaultWithCustomTargets,

        /// <summary>
        /// Has more imports and the shape is unknown.
        /// </summary>
        Custom
    }
}