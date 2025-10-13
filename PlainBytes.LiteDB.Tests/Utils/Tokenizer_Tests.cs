using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils;

public class Tokenizer_Tests
{
    private static Token ReadNextToken(Tokenizer tz, bool eatWhitespace = true)
        => tz.ReadToken(eatWhitespace);

    [Fact]
    public void Tokenizes_Punctuation_And_Operators()
    {
        var tz = new Tokenizer("{ } [ ] ( ) , : ; @ # ~ . & $ $word ! != = > >= < <= - + * / \\\n%");

        Assert.Equal(TokenType.OpenBrace, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.CloseBrace, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.OpenBracket, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.CloseBracket, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.OpenParenthesis, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.CloseParenthesis, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Comma, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Colon, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.SemiColon, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.At, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Hashtag, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Til, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Period, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Ampersand, ReadNextToken(tz).Type);

        // bare $ alone is a Dollar token
        Assert.Equal(TokenType.Dollar, ReadNextToken(tz).Type);

        // $ followed by word becomes a Word token value starting with $
        var tok = ReadNextToken(tz);
        Assert.Equal(TokenType.Word, tok.Type);
        Assert.Equal("$", tok.Value[..1]);

        Assert.Equal(TokenType.Exclamation, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.NotEquals, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Equals, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Greater, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.GreaterOrEquals, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Less, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.LessOrEquals, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Minus, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Plus, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Asterisk, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Slash, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Backslash, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Percent, ReadNextToken(tz).Type);

        Assert.Equal(TokenType.EOF, ReadNextToken(tz).Type);
    }

    [Fact]
    public void Words_And_IsWordChar_Behavior()
    {
        Assert.True(Tokenizer.IsWordChar('_', true));
        Assert.True(Tokenizer.IsWordChar('$', true));
        Assert.True(Tokenizer.IsWordChar('A', true));
        Assert.False(Tokenizer.IsWordChar('1', true));

        Assert.True(Tokenizer.IsWordChar('1', false));
        Assert.True(Tokenizer.IsWordChar('_', false));
        Assert.True(Tokenizer.IsWordChar('$', false));

        var tz = new Tokenizer("hello _x $abc$ 123");
        var t1 = ReadNextToken(tz);
        Assert.Equal(TokenType.Word, t1.Type);
        Assert.Equal("hello", t1.Value);

        var t2 = ReadNextToken(tz);
        Assert.Equal(TokenType.Word, t2.Type);
        Assert.Equal("_x", t2.Value);

        var t3 = ReadNextToken(tz);
        Assert.Equal(TokenType.Word, t3.Type);
        Assert.Equal("$abc$", t3.Value);

        var t4 = ReadNextToken(tz);
        Assert.Equal(TokenType.Int, t4.Type);
        Assert.Equal("123", t4.Value);
    }

    [Fact]
    public void Numbers_Int_Double_And_Scientific()
    {
        var tz = new Tokenizer("0 42 3.14 10. 1e10 2.5e-3 9E+2 7e0");
        Assert.Equal(TokenType.Int, ReadNextToken(tz).Type);
        Assert.Equal(TokenType.Int, ReadNextToken(tz).Type);

        var d1 = ReadNextToken(tz);
        Assert.Equal(TokenType.Double, d1.Type);
        Assert.Equal("3.14", d1.Value);

        var d2 = ReadNextToken(tz);
        Assert.Equal(TokenType.Double, d2.Type);
        Assert.Equal("10.", d2.Value);

        var s1 = ReadNextToken(tz);
        Assert.Equal(TokenType.Double, s1.Type);
        Assert.Equal("1e10", s1.Value);

        var s2 = ReadNextToken(tz);
        Assert.Equal(TokenType.Double, s2.Type);
        Assert.Equal("2.5e-3", s2.Value);

        var s3 = ReadNextToken(tz);
        Assert.Equal(TokenType.Double, s3.Type);
        Assert.Equal("9E+2", s3.Value);

        var s4 = ReadNextToken(tz);
        Assert.Equal(TokenType.Double, s4.Type);
        Assert.Equal("7e0", s4.Value);

        Assert.Equal(TokenType.EOF, ReadNextToken(tz).Type);
    }

    [Fact]
    public void Strings_Handle_Escapes_And_Unicode()
    {
        // double quotes with escapes (escaped quote in the middle to avoid end-of-string edge)
        var tz1 = new Tokenizer("\"A\\\"B\\n\\t\\\\C\""); // "A\"B\n\t\\C"
        var tok1 = ReadNextToken(tz1);
        Assert.Equal(TokenType.String, tok1.Type);
        Assert.Equal("A\"B\n\t\\C", tok1.Value);

        // single quotes with escapes and unicode \u0041 (A)
        var tz2 = new Tokenizer("'abc\\u0041\\'def'");
        var tok2 = ReadNextToken(tz2);
        Assert.Equal(TokenType.String, tok2.Type);
        Assert.Equal("abcA'def", tok2.Value);
    }

    [Fact]
    public void Line_Comments_Are_Skipped()
    {
        var tz = new Tokenizer("-- comment here\nword");
        var t = ReadNextToken(tz);
        Assert.Equal(TokenType.Word, t.Type);
        Assert.Equal("word", t.Value);
        Assert.Equal(TokenType.EOF, ReadNextToken(tz).Type);
    }

    [Fact]
    public void Whitespace_Token_Returned_When_Not_Eating()
    {
        var tz = new Tokenizer("  \t\nword");
        var ws = tz.ReadToken(eatWhitespace: false);
        Assert.Equal(TokenType.Whitespace, ws.Type);
        Assert.True(ws.Value.Length >= 2);

        // Next token with default should eat remaining whitespace and return the word
        var w = tz.ReadToken();
        Assert.Equal(TokenType.Word, w.Type);
        Assert.Equal("word", w.Value);
    }

    [Fact]
    public void LookAhead_Does_Not_Consume()
    {
        var tz = new Tokenizer("123 456");
        var a = tz.LookAhead();
        Assert.Equal(TokenType.Int, a.Type);
        Assert.Equal("123", a.Value);

        var first = tz.ReadToken();
        Assert.Same(a, first);

        var secondAhead = tz.LookAhead();
        Assert.Equal(TokenType.Int, secondAhead.Type);
        Assert.Equal("456", secondAhead.Value);

        var second = tz.ReadToken();
        Assert.Same(secondAhead, second);
        Assert.Equal(TokenType.EOF, tz.ReadToken().Type);
    }

    [Fact]
    public void EOF_And_CheckEOF_Behavior()
    {
        var tz = new Tokenizer("");
        var eof = tz.ReadToken();
        Assert.Equal(TokenType.EOF, eof.Type);

        // CheckEOF throws UnexpectedToken when already at EOF
        var ex = Assert.Throws<LiteException>(() => tz.CheckEOF());
        Assert.Equal(LiteException.UNEXPECTED_TOKEN, ex.ErrorCode);
    }

    [Fact]
    public void Expect_And_IsOperand_Semantics()
    {
        var tz = new Tokenizer("1 + 2 AND 3");
        var t1 = tz.ReadToken();
        t1.Expect(TokenType.Int);

        var plus = tz.ReadToken();
        Assert.True(plus.IsOperand);
        plus.Expect(TokenType.Plus);

        var two = tz.ReadToken();
        two.Expect(TokenType.Int);

        var andTok = tz.ReadToken();
        Assert.True(andTok.IsOperand); // "AND" is considered operand via keywords set
        Assert.Equal(TokenType.Word, andTok.Type);

        // Force mismatch to ensure Expect throws UnexpectedToken
        var ex = Assert.Throws<LiteException>(() => andTok.Expect(TokenType.Int));
        Assert.Equal(LiteException.UNEXPECTED_TOKEN, ex.ErrorCode);
    }

    [Fact]
    public void ParseUnicode_And_SingleChar()
    {
        uint code = Tokenizer.ParseUnicode('0', '0', '4', '1'); // 0x0041 = 'A'
        Assert.Equal('A', (char)code);

        Assert.Equal(0u, Tokenizer.ParseSingleChar('g', 1)); // invalid hex returns 0
        Assert.Equal(0xA0u, Tokenizer.ParseSingleChar('A', 0x10)); // 10 * 0x10 = 0xA0
        Assert.Equal(0xF000u, Tokenizer.ParseSingleChar('F', 0x1000));
    }
}
