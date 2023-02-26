using JsonRuleParser.Expressions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace JsonRuleParser.Tests
{
    public class StatementExpressionTests
    {
        List<OrderData> TestList = new List<OrderData>
        {
            new OrderData
            {
                Context = new Context { Id = 123456, Description = "Description1"},
                Events= new List<Event>
                {
                    new Event
                    {
                        EventId = 577,
                        EventName = "Test Event name",
                        Seats = new List<Seat>
                        {
                            new Seat { PriceCodeType = "Regular 2" },
                            new Seat { PriceCodeType = "Regular 2" }
                        }
                    }
                }
            },
            new OrderData
            {
                Context = new Context { Id = 123456, Description = "Description"},
                Events= new List<Event>
                {
                    new Event
                    {
                        EventId = 577,
                        EventName = "Test Event name",
                        Seats = new List<Seat>
                        {
                            new Seat { PriceCodeType = "Regular 3" },
                            new Seat { PriceCodeType = "Regular 3" }
                        }
                    }
                }
            },
            new OrderData
            {
                Context = new Context { Id = 654321, Description = "Description"},
                Events= new List<Event>
                {
                    new Event
                    {
                        EventId = 577,
                        EventName = "Test Event name",
                        Seats = new List<Seat>
                        {
                            new Seat { PriceCodeType = "Regular 2" },
                            new Seat { PriceCodeType = "Regular 2" }
                        }
                    }
                }
            },
            new OrderData
            {
                Context = new Context { Id = 123456, Description = "Description2"},
                Events= new List<Event>
                {
                    new Event
                    {
                        EventId = 277,
                        EventName = "Test Event name",
                        Seats = new List<Seat>
                        {
                            new Seat { PriceCodeType = "Regular 2" },
                            new Seat { PriceCodeType = "Regular 2" }
                        }
                    }
                }
            },
            new OrderData
            {
                Context = new Context { Id = 123456, Description = "Description"},
                Events= new List<Event>
                {
                    new Event
                    {
                        EventId = 577,
                        EventName = "Test Event name",
                        Seats = new List<Seat>
                        {
                            new Seat { PriceCodeType = "Regular 2" }
                        }
                    }
                }
            },
        };

        private static IEnumerable<TestCaseData> StatementExpressionTestCases()
        {
            yield return new TestCaseData(AndStatementTest1, 2);
        }

        [Test, TestCaseSource("StatementExpressionTestCases")]
        public void StatementExpressionTests_SuccessParsing(StatementExpression statement, int expectedCount)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var reader = new JsonRuleReader<OrderData>();
            var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(statement, options));

            var predicate = reader.ParsePredicateOf(jsonDocument);

            var filtered = TestList.Where(predicate).ToList();

            Assert.That(filtered.Count == expectedCount);
        }

        #region Test Expressions
        // ("Context.Id" in (123456, 432156)) and ((not "Context.Description" = 'Description'))
        private static StatementExpression AndStatementTest1 => new AndExpression
        {
            Type = "and",
            Statements = new StatementExpression[]
            {
                new PredicateExpression
                {
                    Attribute = "Context.Id",
                    Operator = Operators.Contains,
                    Type = "predicate",
                    Value = new ValueExpression
                    {
                        ValueType = nameof(Int32),
                        Values = new object[] { 123456, 432156 }
                    }
                },
                new AndExpression
                {
                    Type = "and",
                    Statements = new StatementExpression[]
                    {
                        new NotExpression
                        {
                            Type = "not",
                            Statement = new PredicateExpression
                            {
                                Attribute = "Context.Description",
                                Operator = Operators.Equals,
                                Type = "predicate",
                                Value = new ValueExpression
                                {
                                    ValueType = nameof(String),
                                    Values = new object[] { "Description" }
                                }
                            }
                        }
                    }
                }
            }
        };

        private static StatementExpression AndStatementTest2 => new AndExpression
        {
            Type = "and",
            Statements = new StatementExpression[]
            {
                new PredicateExpression
                {
                    Attribute = "Context.Id",
                    Operator = Operators.Contains,
                    Type = "predicate",
                    Value = new ValueExpression
                    {
                        ValueType = nameof(Int32),
                        Values = new object[] { 123456, 432156 }
                    }
                },
                new AndExpression
                {
                    Type = "and",
                    Statements = new StatementExpression[]
                    {
                        new PredicateExpression
                        {
                            Attribute = "Events.EventId",
                            Operator = Operators.Equals,
                            Type = "predicate",
                            Value = new ValueExpression
                            {
                                ValueType = nameof(Int32),
                                Values = new object[] { 577 }
                            }
                        }
                    }
                }
            }
        };


        #endregion

        #region Test classes
        class OrderData
        {
            public Context Context { get; set; }

            public IList<Event> Events { get; set; }
        }

        class Context
        {
            public int Id { get; set; }

            public string Description { get; set; }
        }

        class Event
        {
            public int EventId { get; set; }

            public string EventName { get; set; }

            public IList<Seat> Seats { get; set; }
        }

        class Seat
        {
            public string PriceCodeType { get; set; }
        }
        #endregion
    }
}