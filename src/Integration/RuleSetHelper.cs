﻿//-----------------------------------------------------------------------
// <copyright file="RuleSetHelper.cs" company="SonarSource SA and Microsoft Corporation">
//   Copyright (c) SonarSource SA and Microsoft Corporation.  All rights reserved.
//   Licensed under the MIT License. See License.txt in the project root for license information.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.VisualStudio.CodeAnalysis.RuleSets;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SonarLint.VisualStudio.Integration
{
    internal static class RuleSetHelper
    {
        /// <summary>
        /// Updates the <paramref name="ruleSet"/> by deleting all the previously included rule sets 
        /// that were in the same folder as <paramref name="solutionRuleSetPath"/> and then includes 
        /// the rule set specified by <paramref name="solutionRuleSetPath"/>.
        /// </summary>
        /// <remarks>
        /// The update is in-memory to the <paramref name="ruleSet"/> and we rely on the fact that we 
        /// previously generated the 'solutionRuleSet' to the same folder as the updated <paramref name="solutionRuleSetPath"/></remarks>
        /// <param name="ruleSet">Existing project level rule set</param>
        /// <param name="solutionRuleSetPath">Full path of solution level rule set (one that was generated during bind)</param>
        public static void UpdateExistingProjectRuleSet(RuleSet ruleSet, string solutionRuleSetPath)
        {
            Debug.Assert(ruleSet != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(solutionRuleSetPath));

            string projectRuleSetPath = ruleSet.FilePath;
            Debug.Assert(!string.IsNullOrWhiteSpace(projectRuleSetPath));

            // Remove all solution level inclusions
            string solutionRuleSetRoot = PathHelper.ForceDirectoryEnding(Path.GetDirectoryName(solutionRuleSetPath));
            RuleSetHelper.RemoveAllIncludesUnderRoot(ruleSet, solutionRuleSetRoot);

            // Add correct inclusion
            string expectedIncludePath = PathHelper.CalculateRelativePath(projectRuleSetPath, solutionRuleSetPath);
            ruleSet.RuleSetIncludes.Add(new RuleSetInclude(expectedIncludePath, RuleAction.Default));
        }

        /// <summary>
        /// Remove all rule set inclusions which exist under the specified root directory.
        /// </summary>
        internal /* testing purposes */ static void RemoveAllIncludesUnderRoot(RuleSet ruleSet, string rootDirectory)
        {
            Debug.Assert(ruleSet != null, "RuleSet expected");
            Debug.Assert(!string.IsNullOrWhiteSpace(rootDirectory), "Root directory expected");

            string ruleSetRoot = PathHelper.ForceDirectoryEnding(Path.GetDirectoryName(ruleSet.FilePath));

            // List<T> required as RuleSetIncludes will be modified
            List<RuleSetInclude> toRemove = ruleSet.RuleSetIncludes
                                            .Where(include =>
                                            {
                                                string fullIncludePath = PathHelper.ResolveRelativePath(include.FilePath, ruleSetRoot);
                                                return PathHelper.IsPathRootedUnderRoot(fullIncludePath, rootDirectory);
                                            })
                                            .ToList();
            toRemove.ForEach(x => ruleSet.RuleSetIncludes.Remove(x));
        }
    }
}
