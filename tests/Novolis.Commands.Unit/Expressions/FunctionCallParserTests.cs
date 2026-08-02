using Novolis.Commands.Expressions;
using TUnit.Core;

namespace Novolis.Commands.Unit.Expressions;

public sealed class FunctionCallParserTests
{
    [Test]
    public async Task TryParse_Line_With_Four_Numbers()
    {
        var result = FunctionCallParser.TryParse("Line(0, 1, 2, 3)");
        await Assert.That(result.Success).IsTrue();
        var call = result.Call!;
        await Assert.That(call.Name).IsEqualTo("Line");
        await Assert.That(call.HasParentheses).IsTrue();
        await Assert.That(call.Arguments.Count).IsEqualTo(4);
        await Assert.That(call.Arguments[0].Number).IsEqualTo(0);
        await Assert.That(call.Arguments[1].Number).IsEqualTo(1);
        await Assert.That(call.Arguments[2].Number).IsEqualTo(2);
        await Assert.That(call.Arguments[3].Number).IsEqualTo(3);
    }

    [Test]
    public async Task TryParse_Bare_Verb_Has_No_Parentheses()
    {
        var result = FunctionCallParser.TryParse("Undo");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Call!.Name).IsEqualTo("Undo");
        await Assert.That(result.Call.HasParentheses).IsFalse();
        await Assert.That(result.Call.Arguments.Count).IsEqualTo(0);
    }

    [Test]
    public async Task TryParse_Empty_Parens()
    {
        var result = FunctionCallParser.TryParse("Fit()");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Call!.HasParentheses).IsTrue();
        await Assert.That(result.Call.Arguments.Count).IsEqualTo(0);
    }

    [Test]
    public async Task TryParse_Quoted_String_Arg()
    {
        var result = FunctionCallParser.TryParse("Rename(\"hull\")");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Call!.Arguments[0].Text).IsEqualTo("hull");
        await Assert.That(result.Call.Arguments[0].IsNumber).IsFalse();
    }

    [Test]
    public async Task TryParse_Negative_And_Decimal()
    {
        var result = FunctionCallParser.TryParse("Circle(-1.5, 2, 3.25)");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Call!.Arguments[0].Number).IsEqualTo(-1.5);
        await Assert.That(result.Call.Arguments[2].Number).IsEqualTo(3.25);
    }

    [Test]
    public async Task TryParse_Empty_Fails()
    {
        var result = FunctionCallParser.TryParse("   ");
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Error).IsEqualTo(FunctionCallParseError.Empty);
    }

    [Test]
    public async Task TryParse_Unbalanced_Fails()
    {
        var result = FunctionCallParser.TryParse("Line(1, 2");
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Error).IsEqualTo(FunctionCallParseError.UnbalancedParentheses);
    }

    [Test]
    public async Task TryParse_Trailing_Semicolon_Ok()
    {
        var result = FunctionCallParser.TryParse("Line(1, 2, 3, 4);");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Call!.Name).IsEqualTo("Line");
    }

    [Test]
    public async Task TryParse_Nested_Point_Args()
    {
        var result = FunctionCallParser.TryParse("Line(Point(0.0,1.0), Point(1.0,1.0))");
        await Assert.That(result.Success).IsTrue();
        var call = result.Call!;
        await Assert.That(call.Arguments.Count).IsEqualTo(2);
        await Assert.That(call.Arguments[0].IsCall).IsTrue();
        await Assert.That(call.Arguments[0].Call!.Name).IsEqualTo("Point");
        await Assert.That(call.Arguments[0].Call!.Arguments[0].Number).IsEqualTo(0.0);
        await Assert.That(call.Arguments[1].Call!.Arguments[0].Number).IsEqualTo(1.0);
    }

    [Test]
    public async Task TryParseScript_Multiple_Semicolon_Separated()
    {
        var result = FunctionCallParser.TryParseScript(
            "Line(Point(0,1), Point(1,1)); Circle(Point(2,2), 0.5); Box(1,1,1);");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Calls.Count).IsEqualTo(3);
        await Assert.That(result.Calls[0].Name).IsEqualTo("Line");
        await Assert.That(result.Calls[1].Name).IsEqualTo("Circle");
        await Assert.That(result.Calls[2].Name).IsEqualTo("Box");
    }

    [Test]
    public async Task TryParse_Trailing_Text_Fails()
    {
        var result = FunctionCallParser.TryParse("Line(1, 2) extra");
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Error).IsEqualTo(FunctionCallParseError.TrailingText);
    }

    [Test]
    public async Task TryParse_Trailing_Comma_Fails()
    {
        var result = FunctionCallParser.TryParse("Line(1,)");
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Error).IsEqualTo(FunctionCallParseError.InvalidArgument);
    }

    [Test]
    public async Task TryParse_Escaped_Quote_In_String()
    {
        var result = FunctionCallParser.TryParse("Rename(\"h\\ull\")");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Call!.Arguments[0].Text).IsEqualTo("hull");
    }

    [Test]
    public async Task TryParse_Unquoted_Text_Argument()
    {
        var result = FunctionCallParser.TryParse("Tag(layer1)");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Call!.Arguments[0].Text).IsEqualTo("layer1");
    }

    [Test]
    public async Task TryParseScript_Empty_Fails()
    {
        var result = FunctionCallParser.TryParseScript("   ");
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Error).IsEqualTo(FunctionCallParseError.Empty);
    }

    [Test]
    public async Task TryParseScript_Missing_Semicolon_Fails()
    {
        var result = FunctionCallParser.TryParseScript("Line(1); Circle(2) Box(3)");
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Error).IsEqualTo(FunctionCallParseError.TrailingText);
    }

    [Test]
    public async Task TryParseScript_Single_Call_Succeeds()
    {
        var result = FunctionCallParser.TryParseScript("Undo");
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Calls.Count).IsEqualTo(1);
    }

    [Test]
    public async Task TryParse_Unclosed_Quote_Fails()
    {
        var result = FunctionCallParser.TryParse("Rename(\"hull)");
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Error).IsEqualTo(FunctionCallParseError.InvalidArgument);
    }

    [Test]
    public async Task TryParse_Invalid_Name_Fails()
    {
        var result = FunctionCallParser.TryParse("123()");
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Error).IsEqualTo(FunctionCallParseError.InvalidName);
    }

    [Test]
    public async Task TryParseScript_Parse_Error_Mid_Script_Fails()
    {
        var result = FunctionCallParser.TryParseScript("Line(1,2); Bad(;");
        await Assert.That(result.Success).IsFalse();
    }
}
