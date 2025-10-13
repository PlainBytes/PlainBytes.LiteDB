using System;
using System.Collections.Generic;
using System.Linq;
using PlainBytes.LiteDB.Engine;
using static PlainBytes.LiteDB.Constants;

namespace PlainBytes.LiteDB
{
    internal partial class SqlParser
    {
        /// <summary>
        /// COMMIT [ TRANS | TRANSACTION ]
        /// </summary>
        private BsonDataReader ParseCommit()
        {
            _tokenizer.ReadToken().Expect("COMMIT");

            var token = _tokenizer.ReadToken().Expect(TokenType.Word, TokenType.EOF, TokenType.SemiColon);

            if (token.Is("TRANS") || token.Is("TRANSACTION"))
            {
                _tokenizer.ReadToken().Expect(TokenType.EOF, TokenType.SemiColon);
            }

            var result = _engine.Commit();

            return new BsonDataReader(result);
        }
    }
}