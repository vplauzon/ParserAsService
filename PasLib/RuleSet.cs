﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PasLib
{
    internal class RuleSet
    {
        private readonly IDictionary<string, IRule> _ruleMap;
        private readonly IRule _interleaveRule;

        public RuleSet(IEnumerable<IRule> rules, IRule interleaveRule)
        {
            if (rules == null || !rules.Any())
            {
                throw new ArgumentNullException(nameof(rules));
            }

            _ruleMap = rules.ToDictionary(r => r.RuleName, r => r);
            _interleaveRule = interleaveRule;
        }

        public IEnumerable<IRule> Rules { get { return _ruleMap.Values; } }

        public RuleMatch Match(string ruleName, SubString text, int? depth = null)
        {
            if (text.IsNull)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (!_ruleMap.ContainsKey(ruleName))
            {
                throw new ArgumentOutOfRangeException(nameof(ruleName), "Unknown rule");
            }

            var rule = _ruleMap[ruleName];
            var context = new ExplorerContext(text, _interleaveRule, depth);
            var matches = GetMatches(rule, context);
            var exactLengthMatches = from m in matches
                                     where m.Text.Length == text.Length
                                     select m;
            //  Take the first available match (of right length)
            var match = exactLengthMatches.FirstOrDefault();

            return match;
        }

        private static IEnumerable<RuleMatch> GetMatches(
            IRule rule,
            ExplorerContext context)
        {
            var leftMatches = context.InvokeRule(rule);

            foreach (var m in leftMatches)
            {
                var fullMatch = context.MoveInterleaveRight(m);

                yield return fullMatch;
            }
        }
    }
}