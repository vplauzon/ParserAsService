using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasLib;

namespace PasLibTest
{
    [TestClass]
    public class RuleTest
    {
        #region Trivial
        [TestMethod]
        public void EmptyNone()
        {
            var rule = MatchNoneRule.Instance;
            var match = rule.Match("", RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNull(match);
        }

        [TestMethod]
        public void EmptyAny()
        {
            var rule = new MatchAnyCharacterRule(null) as IRule;
            var match = rule.Match("", RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNull(match);
        }
        #endregion

        #region Any
        [TestMethod]
        public void Any()
        {
            var rule = new MatchAnyCharacterRule("Any") as IRule;
            var samples = new[]
            {
                "g",
                "K",
                "@",
                "*",
                "/"
            };

            for (int i = 0; i != samples.Length; ++i)
            {
                var match = rule.Match(samples[i], RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

                Assert.IsNotNull(match, $"Success - {i}");
                Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                Assert.AreEqual(samples[i].Length, match.Text.Length, $"MatchLength - {i}");
                Assert.AreEqual(1, match.Text.Length, $"Content - {i}");
            }
        }
        #endregion

        #region Literal
        [TestMethod]
        public void Literal()
        {
            var rule = new LiteralRule("Lit", "great") as IRule;
            var match = rule.Match("great", RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();
            var nomatch = rule.Match("h", RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Rule");
            Assert.AreEqual(5, match.Text.Length, "Content");

            Assert.IsNull(nomatch, "Failure");
        }
        #endregion

        #region Range
        [TestMethod]
        public void Range()
        {
            var samples = new[]
            {
                Tuple.Create('a', 'e', 'c', true),
                Tuple.Create('a', 'e', 'e', true),
                Tuple.Create('a', 'e', 'a', true),
                Tuple.Create('a', 'e', 'f', false),
                Tuple.Create('b', 'e', 'a', false),
                Tuple.Create('b', 'e', 'C', false)
            };

            for (int i = 0; i != samples.Length; ++i)
            {
                var first = samples[i].Item1;
                var last = samples[i].Item2;
                var test = samples[i].Item3;
                var isSuccess = samples[i].Item4;
                var rule = new RangeRule("Range", first, last) as IRule;
                var match = rule.Match(test.ToString(), RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

                if (isSuccess)
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(1, match.Text.Length, $"MatchLength - {i}");
                }
                else
                {
                    Assert.IsNull(match, $"Failure - {i}");
                }
            }
        }
        #endregion

        #region Repeat
        [TestMethod]
        public void EmptyRepeat()
        {
            var oneCharRule = new LiteralRule("oneChar", "g");
            var rule = new RepeatRule("Repeat", oneCharRule, null, null) as IRule;
            var match = rule.Match("", RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Rule");
            Assert.AreEqual(0, match.Text.Length, "MatchLength");
            Assert.AreEqual(0, match.Repeats.Count(), "Contents");
        }

        [TestMethod]
        public void RepeatOneCharacter()
        {
            var oneCharRule = new LiteralRule("oneChar", "g");
            var rule = new RepeatRule("Repeat", oneCharRule, null, null) as IRule;
            var match = rule.Match("ggggg", RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Rule");
            Assert.AreEqual(5, match.Text.Length, "MatchLength");
            Assert.AreEqual(5, match.Repeats.Count(), "Contents");
        }

        [TestMethod]
        public void RepeatCharacterWithCardinality()
        {
            var oneCharRule = new LiteralRule("oneChar", "g");
            var testSet = new[]
            {
                Tuple.Create<string, int?, int?, bool>("gg", 2, 2, true),
                Tuple.Create<string, int?, int?, bool>("gg", 1, 2, true),
                Tuple.Create<string, int?, int?, bool>("gg", null, 2, true),
                Tuple.Create<string, int?, int?, bool>("gg", 2, null, true),
                Tuple.Create<string, int?, int?, bool>("ggg", 2, 2, false),
                Tuple.Create<string, int?, int?, bool>("ggg", 2, 3, true),
                Tuple.Create<string, int?, int?, bool>("g", 2, 2, false),
                Tuple.Create<string, int?, int?, bool>("g", 2, 3, false)
            };

            for (int i = 0; i != testSet.Length; ++i)
            {
                var text = testSet[i].Item1;
                var min = testSet[i].Item2;
                var max = testSet[i].Item3;
                var isSuccess = testSet[i].Item4;
                var rule = new RepeatRule("Repeat", oneCharRule, min, max) as IRule;
                var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsTrue(
                        match == null || match.Text.Length != text.Length,
                        $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");
                    Assert.IsFalse(match.Text.IsNull, $"Content - {i}");
                    Assert.AreEqual(text.Length, match.Repeats.Count(), $"Contents - {i}");
                }
            }
        }
        #endregion

        #region Disjunction
        [TestMethod]
        public void DisjunctionWithoutTags()
        {
            var samples = new[]
            {
                Tuple.Create("Alice", true),
                Tuple.Create("Bob", true),
                Tuple.Create("Charles", true),
                Tuple.Create("Didier", false)
            };
            //  Alice | Bob | Charles
            var rule = new DisjunctionRule("Disjunction", new[]
            {
                new TaggedRule(null, new LiteralRule("Alice", "Alice")),
                new TaggedRule(null, new LiteralRule("Bob", "Bob")),
                new TaggedRule(null, new LiteralRule("Charles", "Charles"))
            });

            for (int i = 0; i != samples.Length; ++i)
            {
                var text = samples[i].Item1;
                var isSuccess = samples[i].Item2;
                var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsNull(match, $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");
                    Assert.AreEqual(text, match.Text.ToString(), $"Content - {i}");
                    Assert.IsNull(match.Fragments, $"Fragments - {i}");
                    Assert.IsFalse(match.Repeats.Any(), $"Repeats - {i}");
                }
            }
        }

        [TestMethod]
        public void DisjunctionWithTags()
        {
            var samples = new[]
            {
                Tuple.Create("Alice", true),
                Tuple.Create("Bob", true),
                Tuple.Create("Charles", true),
                Tuple.Create("Didier", false),
                Tuple.Create("Darwin", true),
                Tuple.Create("Ernest", true),
                Tuple.Create("Ephreme", false)
            };
            //  a:Alice | s1:(b:Bob | c:Charles) | s2::(d:Darwin | e:Ernest)
            var subRule1 = new DisjunctionRule("Disjunction", new[]
            {
                new TaggedRule("b", new LiteralRule("Bob", "Bob")),
                new TaggedRule("c", new LiteralRule("Charles", "Charles"))
            });
            var subRule2 = new DisjunctionRule("Disjunction", new[]
            {
                new TaggedRule("d", new LiteralRule("Darwin", "Darwin")),
                new TaggedRule("e", new LiteralRule("Ernest", "Ernest"))
            });
            var rule = new DisjunctionRule("Disjunction", new[]
            {
                new TaggedRule("a", new LiteralRule("Alice", "Alice")),
                new TaggedRule("s1", subRule1, true),
                new TaggedRule("s2", subRule2, false)
            });

            for (int i = 0; i != samples.Length; ++i)
            {
                var text = samples[i].Item1;
                var isSuccess = samples[i].Item2;
                var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsNull(match, $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");
                    Assert.IsNotNull(match.Fragments, $"Fragments - {i}");
                    Assert.AreEqual(1, match.Fragments.Count, $"Fragments Count - {i}");
                    if (match.Fragments.Keys.First() == "s1")
                    {
                        Assert.IsNotNull(match.Fragments["s1"].Fragments, $"Sub Fragments 1 - {i}");
                    }
                    if (match.Fragments.Keys.First() == "s2")
                    {
                        Assert.IsNull(match.Fragments["s2"].Fragments, $"Sub Fragments 2 - {i}");
                    }
                }
            }
        }

        [TestMethod]
        public void DisjunctionWithRepeat()
        {
            var samples = new[]
            {
                Tuple.Create("ababab", true),
                Tuple.Create("aaaaaa", true),
                Tuple.Create("bbbbbbb", true),
                Tuple.Create("bbbbaaaabbbaaaa", true),
                Tuple.Create("", true),
                Tuple.Create("kaaaaabbbbb", false),
                Tuple.Create("kaaaa", false)
            };
            //  ('a'* | 'b'*)*
            var aRule = new RepeatRule("RepeatA", new LiteralRule("A", "a"), null, null);
            var bRule = new RepeatRule("RepeatB", new LiteralRule("B", "b"), null, null);
            var disjunction = new DisjunctionRule("Disjunction", new[]
            {
                new TaggedRule(null, aRule),
                new TaggedRule(null, bRule)
            });
            var rule = new RepeatRule("MasterRepeat", disjunction, null, null);

            for (int i = 0; i != samples.Length; ++i)
            {
                var text = samples[i].Item1;
                var isSuccess = samples[i].Item2;
                var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsTrue(
                        match == null || match.Text.Length != text.Length,
                        $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");
                    Assert.IsNotNull(match.Repeats, $"Contents - {i}");
                }
            }
        }
        #endregion

        #region Sequence
        [TestMethod]
        public void SequenceWithoutTags()
        {
            var rule = new SequenceRule("Seq", new[]
            {
                new TaggedRule(null, new LiteralRule(null, "Hi")),
                new TaggedRule(null, new LiteralRule(null, "Bob")),
                new TaggedRule(null, new LiteralRule(null, "!"))
            });
            var text = "HiBob!";
            var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Seq");
            Assert.AreEqual(text.Length, match.Text.Length, "MatchLength");
            Assert.AreEqual(text.ToString().TrimStart(), match.Text.ToString(), "Content");
            Assert.IsNull(match.Fragments, "Fragments");
        }

        [TestMethod]
        public void SequenceWithTags()
        {
            var rule = new SequenceRule("Seq", new[]
            {
                new TaggedRule("h", new LiteralRule(null, "Hi")),
                new TaggedRule("b", new LiteralRule(null, "Bob")),
                new TaggedRule(null, new LiteralRule(null, "!"))
            });
            var text = "HiBob!";
            var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Seq");
            Assert.AreEqual(text.Length, match.Text.Length, "MatchLength");
            Assert.AreEqual(2, match.Fragments.Count(), "Fragments");
            Assert.AreEqual("Hi", match.Fragments["h"].Text.ToString(), "Fragments - Hi");
            Assert.AreEqual("Bob", match.Fragments["b"].Text.ToString(), "Fragments - Bob");
        }

        [TestMethod]
        public void SequenceWithNoChildrenTags()
        {
            var rule = new SequenceRule("Seq", new[]
            {
                new TaggedRule("a", new RepeatRule(null, new LiteralRule(null, "a"), 1, null), true),
                new TaggedRule("b", new RepeatRule(null, new LiteralRule(null, "b"), 1, null), false)
            });
            var text = "aaaabb";
            var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNotNull(match, "Success");
            Assert.AreEqual(rule.RuleName, match.Rule.RuleName, "Seq");
            Assert.AreEqual(text.Length, match.Text.Length, "MatchLength");
            Assert.AreEqual(2, match.Fragments.Count(), "Fragments");
            Assert.AreEqual("aaaa", match.Fragments["a"].Text.ToString(), "Fragments text - a");
            Assert.IsNotNull(match.Fragments["a"].Repeats, "Fragments - Repeats - a");
            Assert.AreEqual(4, match.Fragments["a"].Repeats.Count(), "Fragments - Counts - a");
            Assert.AreEqual("bb", match.Fragments["b"].Text.ToString(), "Fragments text - b");
            Assert.IsNotNull(match.Fragments["b"].Repeats, "Fragments - Repeats - b");
        }
        #endregion

        #region Substract
        [TestMethod]
        public void Substract()
        {
            var samples = new[]
            {
                Tuple.Create('a', true),
                Tuple.Create('z', true),
                Tuple.Create('t', true),
                Tuple.Create('f', false),
                Tuple.Create('g', false),
                Tuple.Create('h', false)
            };
            var range = new RangeRule(null, 'a', 'z');
            var exclusion = new RangeRule(null, 'f', 'h');
            var rule = new SubstractRule("Substract", new TaggedRule(null, range), exclusion);

            for (int i = 0; i != samples.Length; ++i)
            {
                var text = samples[i].Item1.ToString();
                var isSuccess = samples[i].Item2;
                var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsNull(match, $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");
                    Assert.AreEqual(text, match.Text.ToString(), $"Content - {i}");
                }
            }
        }

        [TestMethod]
        public void SubstractWithTags()
        {
            var samples = new[]
            {
                Tuple.Create('a', true),
                Tuple.Create('z', true),
                Tuple.Create('t', true),
                Tuple.Create('f', false),
                Tuple.Create('g', false),
                Tuple.Create('h', false)
            };
            var range = new RangeRule(null, 'a', 'z');
            var exclusion = new RangeRule(null, 'f', 'h');
            var rule = new SubstractRule("Substract", new TaggedRule("mine", range), exclusion);

            for (int i = 0; i != samples.Length; ++i)
            {
                var text = samples[i].Item1.ToString();
                var isSuccess = samples[i].Item2;
                var match = rule.Match(text, RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

                if (!isSuccess)
                {
                    Assert.IsNull(match, $"Test case #{i} should have failed");
                }
                else
                {
                    Assert.IsNotNull(match, $"Success - {i}");
                    Assert.AreEqual(rule.RuleName, match.Rule.RuleName, $"Rule - {i}");
                    Assert.AreEqual(text.Length, match.Text.Length, $"MatchLength - {i}");
                    Assert.IsNotNull(match.Fragments, $"Fragments - {i}");
                    Assert.AreEqual(1, match.Fragments.Count, $"Fragments Count - {i}");
                }
            }
        }
        #endregion

        #region Recursion
        [TestMethod]
        public void PotentialInfiniteRecurse()
        {
            //  Equivalent to:
            //  rule A = "a".."z";
            //  rule B = C "," C;
            //  rule C = A | B;
            //  Try to match "a" with C
            var ruleA = new RangeRule("A", 'a', 'z');
            var proxyC = new RuleProxy();
            var ruleB = new SequenceRule("B", new[]{
                new TaggedRule(proxyC),
                new TaggedRule(new LiteralRule(null, ",")),
                new TaggedRule(proxyC)
            });
            var ruleC = new DisjunctionRule("C", new[] {
                new TaggedRule(ruleA),
                new TaggedRule(ruleB)
            });

            proxyC.ReferencedRule = ruleC;

            var match = ruleC.Match("a", RuleSet.DEFAULT_MAX_DEPTH).FirstOrDefault();

            Assert.IsNotNull(match, "Should be a success");
        }
        #endregion
    }
}