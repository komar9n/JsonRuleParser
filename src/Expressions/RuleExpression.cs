using System.Text.Json.Serialization;

namespace JsonRuleParser.Expressions
{
    public interface RuleExpression { }

    [JsonPolymorphic]
    [JsonDerivedType(typeof(PredicateExpression), "PredicateExpression")]
    [JsonDerivedType(typeof(AndExpression), "AndExpression")]
    [JsonDerivedType(typeof(OrExpression), "OrExpression")]
    [JsonDerivedType(typeof(NotExpression), "NotExpression")]
    public interface StatementExpression : RuleExpression
    {
        string Type { get; set; }
    }

    public class ValueExpression : RuleExpression
    {
        public object[] Values { get; set; }
        public string ValueType { get; set; }
    }

    public class PredicateExpression : StatementExpression
    {
        public string Type { get; set; }
        public string Attribute { get; set; }
        public string Operator { get; set; }
        public ValueExpression Value { get; set; }
    }

    public class AndExpression : StatementExpression
    {
        public string Type { get; set; }
        public StatementExpression[] Statements { get; set; }
    }

    public class OrExpression : StatementExpression
    {
        public string Type { get; set; }
        public StatementExpression[] Statements { get; set; }
    }

    public class NotExpression : StatementExpression
    {
        public string Type { get; set; }
        public StatementExpression Statement { get; set; }
    }
}
