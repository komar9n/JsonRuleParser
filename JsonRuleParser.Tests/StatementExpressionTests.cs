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
                },
                Customer = new Customer { Name = "John Smith" }
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
                },
                Customer = new Customer { Name = "John Camel" }
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
                },
                Customer = new Customer { Name = "John Smith" }
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
                },
                Customer = new Customer { Name = "John Smith" }
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
                },
                Customer = new Customer { Name = "Smith John" }
            },
        };

        private static IEnumerable<TestCaseData> StatementExpressionTestCases()
        {
            yield return new TestCaseData(AndStatementTest1, 2);
            yield return new TestCaseData(AndStatementTest2, 3);
        }

        [Test, TestCaseSource("StatementExpressionTestCases")]
        public void StatementExpressionTests_SuccessParsing(StatementExpression statement, int expectedCount)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var reader = new JsonRuleReader<OrderData>();
            var serializedJsonString = JsonSerializer.Serialize(statement, options);

            var jsonDocument = JsonDocument.Parse(serializedJsonString);

            var predicate = reader.ParsePredicateOf(jsonDocument);

            var filtered = TestList.Where(predicate).ToList();

            Assert.That(filtered.Count == expectedCount);
        }

        #region Test Expressions
        // ("Context.Id" in (123456, 432156)) and ((not "Context.Description" = 'Description') and ("Customer.Name" = 'John Smith'))
        private static StatementExpression AndStatementTest1 => new AndExpression
        {
            Statements = new StatementExpression[]
            {
                new PredicateExpression
                {
                    Attribute = "Context.Id",
                    Operator = Operators.Contains,
                    Value = new ValueExpression
                    {
                        ValueType = nameof(Int32),
                        Values = new object[] { 123456, 432156 }
                    }
                },
                new AndExpression
                {
                    Statements = new StatementExpression[]
                    {
                        new NotExpression
                        {
                            Statement = new PredicateExpression
                            {
                                Attribute = "Context.Description",
                                Operator = Operators.Equals,
                                Value = new ValueExpression
                                {
                                    ValueType = nameof(String),
                                    Values = new object[] { "Description" }
                                }
                            }
                        },
                        new PredicateExpression
                        {
                            Attribute = "Customer.Name",
                            Operator = Operators.Equals,
                            Value = new ValueExpression
                            {
                                ValueType = nameof(String),
                                Values = new object[] { "John Smith" }
                            }
                        }
                    }
                }
            }
        };

        // ("Context.Id" = 123456) and ("Customer.Name" LIKE 'John%')
        private static StatementExpression AndStatementTest2 => new AndExpression
        {
            Statements = new StatementExpression[]
            {
                new PredicateExpression
                {
                    Attribute = "Context.Id",
                    Operator = Operators.Equals,
                    Value = new ValueExpression
                    {
                        ValueType = nameof(Int32),
                        Values = new object[] { 123456 }
                    }
                },
                new AndExpression
                {
                    Statements = new StatementExpression[]
                    {
                        new PredicateExpression
                        {
                            Attribute = "Customer.Name",
                            Operator = Operators.StartWith,
                            Value = new ValueExpression
                            {
                                ValueType = nameof(String),
                                Values = new object[] { "John" }
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

            public Customer Customer { get; set; }
        }

        class Customer
        {
            public string Name { get; set; }
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