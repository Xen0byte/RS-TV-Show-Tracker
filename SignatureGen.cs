









using System;
using System.Reflection;

[assembly: AssemblyVersion("2.2.0.725")]

namespace RoliSoft.TVShowTracker
{
    /// <summary>
    /// Contains informations about the assembly.
    /// </summary>
    public static partial class Signature
    {
        /// <summary>
        /// Gets the year when the executing assembly was compiled.
        /// </summary>
        /// <value>The compilation year.</value>
        public const string CompileYear = "2019";

        /// <summary>
        /// Gets the date and time when the executing assembly was compiled.
        /// </summary>
        /// <value>The compile time.</value>
        public static DateTime CompileTime
        {
            get { return DateTime.FromBinary(-8586299473346488957); }
        }

        /// <summary>
        /// Gets the git commit hash.
        /// </summary>
        /// <value>The git commit hash.</value>
        public static string GitRevision
        {
            get { return "c45e80512cc5d2a6a3927fa31f95ec0357a337e7"; }
        }

        /// <summary>
        /// Gets the directory in which the project was built.
        /// </summary>
        /// <value>The directory in which the project was built.</value>
        public static string BuildDirectory
        {
            get { return @"C:\Users\VLOOD\Source\Repos\RS-TV-Show-Tracker"; }
        }

        /// <summary>
        /// Gets the name of the machine where the project was built.
        /// </summary>
        /// <value>The name of the machine where the project was built.</value>
        public static string BuildMachine
        {
            get { return @"SADS-810"; }
        }
    }
}
