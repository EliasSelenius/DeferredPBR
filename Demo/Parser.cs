
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

record State(string input, int index, dynamic result, bool isError, string errorMsg);
delegate State transformStateFunc(State state);

class Parser {

    transformStateFunc stf;
    bool isAndUnary = false;

    public Parser(transformStateFunc stf) {
        init(stf);
    }

    public Parser() {}

    public void init(transformStateFunc stf) {
        this.stf = stf;
    }

    public void init(Parser parser) {
        this.stf = parser.stf;
    }

    public State run(string input) {
        var state = new State(input, 0, null, false, null);
        return stf(state);
    }

    public Parser map(Func<dynamic, dynamic> func) => new Parser(state => {
        var nextState = this.stf(state);
        if (nextState.isError) return nextState;
        return updateState(nextState, nextState.index, func(nextState.result));
    });

    private static State updateState(State state, int index, dynamic res) => state with {
        index = index,
        result = res
    };

    private static State updateError(State state, string msg) => state with {
        isError = true,
        errorMsg = msg
    };

    //public static Parser operator <(Parser parser, Parser parser2) => null;
    //public static Parser operator >(Parser parser, Parser parser2) => null;

    //public Parser this[int _] => null;

    public static Parser operator -(Parser parser, Func<dynamic, dynamic> func) => parser.map(func);

    public static Parser operator ++(Parser parser) => many1(parser);
    public static Parser operator +(Parser parser) => many(parser);
    public static implicit operator Parser(string s) => str(s);

    public static Parser operator &(Parser left, Parser right) => new Parser(state => {
        if (state.isError) return state;

        var results = new List<dynamic>();

        var nextState = state;

        nextState = left.stf(nextState);
        if (nextState.isError) return nextState;
        if (left.isAndUnary) {
            for (int i = 0; i < nextState.result.Length; i++) results.Add(nextState.result[i]);
        } else results.Add(nextState.result);
        
        nextState = right.stf(nextState);
        if (nextState.isError) return nextState;
        if (right.isAndUnary) {
            for (int i = 0; i < nextState.result.Length; i++) results.Add(nextState.result[i]);
        } else results.Add(nextState.result);

        return updateState(nextState, nextState.index, results.ToArray());
    }) {
        isAndUnary = true
    };


    public static Parser operator |(Parser left, Parser right) => new Parser(state => {
        if (state.isError) return state;

        var s = left.stf(state);
        if (!s.isError) return s;

        s = right.stf(state);
        if (!s.isError) return s;

        return updateError(state, "Neither left or right parser passed");
    });

    public static Parser operator /(Parser value, Parser seperator) => sepby(seperator)(value);

    public static Parser str(string s) => new Parser(state => {    
        if (state.isError) return state;

        if (state.input.Substring(state.index).StartsWith(s)) {
            return updateState(state, state.index + s.Length, s);
        }

        return updateError(state, $"Did not get expected token {s}");
    });

    public static Parser sequence(params Parser[] parsers) => new Parser(state => {
        if (state.isError) return state;

        var nextState = state;
        var results = new dynamic[parsers.Length];

        for (int i = 0; i < parsers.Length; i++) {
            nextState = parsers[i].stf(nextState);
            results[i] = nextState.result;
        }

        return updateState(nextState, nextState.index, results);
    });

    public static Parser regex(string pattern) => new Parser(state => {
        if (state.isError) return state;

        var s = state.input.Substring(state.index);

        var match = Regex.Match(s, pattern);
        if (match.Success) {
            return updateState(state, state.index + match.Length, match.Value);
        }

        return updateError(state, $"Did not get expected token");
    });

    public static readonly Parser letters = regex("^[a-zA-Z]+");
    public static readonly Parser digits = regex("^[0-9]+");
    public static readonly Parser whitespace = regex("^\\s+");

    public static Parser many(Parser parser) => new Parser(state => {
        if (state.isError) return state;

        var results = new List<dynamic>();
        var nextState = state;
        while (true) {
            var s = parser.stf(nextState);
            if (s.isError) break;
            results.Add(s.result);
            nextState = s;
        }

        return updateState(nextState, nextState.index, results.ToArray());
    });

    public static Parser many1(Parser parser) => new Parser(state => {
        if (state.isError) return state;

        var results = new List<dynamic>();
        var nextState = state;
        while (true) {
            var s = parser.stf(nextState);
            if (s.isError) break;
            results.Add(s.result);
            nextState = s;
        }

        if (results.Count == 0) return updateError(nextState, "many1 error");

        return updateState(nextState, nextState.index, results.ToArray());
    });

    public static Parser choice(params Parser[] parsers) => new Parser(state => {
        if (state.isError) return state;

        for (int i = 0; i < parsers.Length; i++) {
            var nextState = parsers[i].stf(state);
            if (!nextState.isError) return nextState;
        }

        return updateError(state, "choice error");
    });

    public static Func<Parser, Parser> between(Parser left, Parser right) => (Parser center) => sequence(left, center, right).map(r => r[1]);
    public static readonly Func<Parser, Parser> inParentheses = between(str("("), str(")"));
    public static readonly Func<Parser, Parser> inSquareBrackets = between(str("["), str("]"));
    public static readonly Func<Parser, Parser> inCurlyBrackets = between(str("{"), str("}"));
    public static readonly Func<Parser, Parser> inAngleBrackets = between(str("<"), str(">"));

    public static Func<Parser, Parser> sepby(Parser seperator) => (Parser parser) => new Parser(state => {
        if (state.isError) return state;

        var nextState = state;
        var results = new List<dynamic>();

        while (true) {
            var valueState = parser.stf(nextState);
            if (valueState.isError) break;
            results.Add(valueState.result);
            nextState = valueState;

            var seperatorState = seperator.stf(nextState);
            if (seperatorState.isError) break;
            nextState = seperatorState;
        }

        return updateState(nextState, nextState.index, results.ToArray());
    });

    public static Func<Parser, Parser> sepby1(Parser seperator) => (Parser parser) => new Parser(state => {
        if (state.isError) return state;

        var nextState = state;
        var results = new List<dynamic>();

        while (true) {
            var valueState = parser.stf(nextState);
            if (valueState.isError) break;
            results.Add(valueState.result);
            nextState = valueState;

            var seperatorState = seperator.stf(nextState);
            if (seperatorState.isError) break;
            nextState = seperatorState;
        }

        if (results.Count == 0) return updateError(state, "sepby1: unable to capture any results");

        return updateState(nextState, nextState.index, results.ToArray());
    });

    public static readonly Parser comma = str(",");
    public static readonly Parser period = str(".");
    public static readonly Parser colon = str(":");
    public static readonly Parser semicolon = str(";");
    
    
    

}