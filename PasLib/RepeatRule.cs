﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PasLib
{
    internal class RepeatRule : RuleBase
    {
        private readonly IRule _rule;
        private readonly int? _min;
        private readonly int? _max;

        public RepeatRule(
            string ruleName,
            Func<IOutputExtractor> outputExtractorFactory,
            IRule rule,
            int? min,
            int? max,
            bool? hasInterleave = null,
            bool? isRecursive = null,
            bool? hasChildrenDetails = null)
            : base(
                  ruleName,
                  outputExtractorFactory,
                  hasInterleave,
                  isRecursive,
                  false,
                  hasChildrenDetails)
        {
            _rule = rule ?? throw new ArgumentNullException(nameof(rule));
            if (min.HasValue && max.HasValue && min.Value > max.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(max), "Must be larger than min");
            }

            _min = min;
            _max = max;
        }

        protected override IEnumerable<RuleMatch> OnMatch(ExplorerContext context)
        {
            return RecurseMatch(
                context.ContextID,
                context,
                context.Text,
                0,
                0,
                ImmutableStack<RuleMatch>.Empty);
        }

        protected override object DefaultExtractOutput(
            SubString text,
            IImmutableList<RuleMatch> children,
            IImmutableDictionary<string, RuleMatch> namedChildren)
        {
            var outputs = from c in children
                          select c.ComputeOutput();

            return outputs.ToImmutableArray();
        }

        public override string ToString()
        {
            var min = _min.HasValue ? _min.Value.ToString() : string.Empty;
            var max = _max.HasValue ? _max.Value.ToString() : string.Empty;

            return ToStringRuleName() + $" ({ToString(_rule)}){{{min}, {max}}}";
        }

        private IEnumerable<RuleMatch> RecurseMatch(
            //  Used only for debugging purposes, to hook on the context ID of the entire sequence
            int masterContextID,
            ExplorerContext context,
            SubString originalText,
            int totalMatchLength,
            int iteration,
            ImmutableStack<RuleMatch> childenStack)
        {
            var matches = context.InvokeRule(_rule);
            var nonEmptyMatches = from m in matches
                                  where m.Text.HasContent
                                  select m;
            var hasOneMatchSentinel = false;

            foreach (var match in nonEmptyMatches)
            {
                var newChildenStack = childenStack.Push(match);
                var newTotalMatchLength = totalMatchLength + match.LengthWithInterleaves;

                hasOneMatchSentinel = true;
                if (!_max.HasValue || iteration + 1 < _max.Value)
                {   //  We can still repeat
                    var newContext = context.MoveForward(match);
                    var downstreamMatches = RecurseMatch(
                        masterContextID,
                        newContext,
                        originalText,
                        newTotalMatchLength,
                        iteration + 1,
                        newChildenStack);

                    foreach (var m in downstreamMatches)
                    {
                        yield return m;
                    }
                }
                //  We have reached our max:  end recursion
                //  Have we reached our min?
                else if ((!_min.HasValue || iteration + 1 >= _min.Value))
                {
                    var content = originalText.Take(newTotalMatchLength);

                    yield return new RuleMatch(
                        this,
                        content,
                        newChildenStack.Reverse());
                }
            }
            //  Repeat didn't work, but if we already reached our min, we're good
            //  (even if no content)
            if (!hasOneMatchSentinel
                && (!_min.HasValue || iteration >= _min.Value))
            {
                var content = originalText.Take(totalMatchLength);

                yield return new RuleMatch(
                    this,
                    content,
                    childenStack.Reverse());
            }
        }
    }
}