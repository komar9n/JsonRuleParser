using System.Text.Json.Serialization;

namespace JsonRuleParser.Expressions
{
    public interface RuleExpression { }

    [JsonPolymorphic]
    [JsonDerivedType(typeof(PredicateExpression), "predicate")]
    [JsonDerivedType(typeof(AndExpression), "and")]
    [JsonDerivedType(typeof(OrExpression), "or")]
    [JsonDerivedType(typeof(NotExpression), "not")]
    public interface StatementExpression : RuleExpression
    {
    }

    public class ValueExpression : RuleExpression
    {
        public object[] Values { get; set; }
        public string ValueType { get; set; }
    }

    public class PredicateExpression : StatementExpression
    {
        public string Attribute { get; set; }
        public string Operator { get; set; }
        public ValueExpression Value { get; set; }
    }

    public class AndExpression : StatementExpression
    {
        public StatementExpression[] Statements { get; set; }
    }

    public class OrExpression : StatementExpression
    {
        public StatementExpression[] Statements { get; set; }
    }

    public class NotExpression : StatementExpression
    {
        public StatementExpression Statement { get; set; }
    }
}
