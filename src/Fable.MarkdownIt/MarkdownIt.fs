// ts2fable 0.9.0
module rec MarkdownIt

#nowarn "3390" // disable warnings for invalid XML comments

open System
open Fable.Core
open Fable.Core.JS

type KeyOfAny = U2<string, float> // TODO: Also allow symbol?
type Array<'T> = System.Collections.Generic.IList<'T>

/// <summary>
/// Main parser/renderer class.
///
/// ##### Usage
///
/// <code lang="javascript">
/// // node.js, "classic" way:
/// var MarkdownIt = require('markdown-it'),
///     md = new MarkdownIt();
/// var result = md.render('# markdown-it rulezz!');
///
/// // node.js, the same, but with sugar:
/// var md = require('markdown-it')();
/// var result = md.render('# markdown-it rulezz!');
///
/// // browser without AMD, added to "window" on script load
/// // Note, there are no dash.
/// var md = window.markdownit();
/// var result = md.render('# markdown-it rulezz!');
/// </code>
///
/// Single line rendering, without paragraph wrap:
///
/// <code lang="javascript">
/// var md = require('markdown-it')();
/// var result = md.renderInline('__markdown-it__ rulezz!');
/// </code>
///
/// ##### Example
///
/// <code lang="javascript">
/// // commonmark mode
/// var md = require('markdown-it')('commonmark');
///
/// // default mode
/// var md = require('markdown-it')();
///
/// // enable everything
/// var md = require('markdown-it')({
///   html: true,
///   linkify: true,
///   typographer: true
/// });
/// </code>
///
/// ##### Syntax highlighting
///
/// <code lang="js">
/// var hljs = require('highlight.js') // https://highlightjs.org/
///
/// var md = require('markdown-it')({
///   highlight: function (str, lang) {
///     if (lang &amp;&amp; hljs.getLanguage(lang)) {
///       try {
///         return hljs.highlight(lang, str, true).value;
///       } catch (__) {}
///     }
///
///     return ''; // use external default escaping
///   }
/// });
/// </code>
///
/// Or with full wrapper override (if you need assign class to <c>&lt;pre&gt;</c>):
///
/// <code lang="javascript">
/// var hljs = require('highlight.js') // https://highlightjs.org/
///
/// // Actual default values
/// var md = require('markdown-it')({
///   highlight: function (str, lang) {
///     if (lang &amp;&amp; hljs.getLanguage(lang)) {
///       try {
///         return '&lt;pre class="hljs"&gt;&lt;code&gt;' +
///                hljs.highlight(lang, str, true).value +
///                '&lt;/code&gt;&lt;/pre&gt;';
///       } catch (__) {}
///     }
///
///     return '&lt;pre class="hljs"&gt;&lt;code&gt;' + md.utils.escapeHtml(str) + '&lt;/code&gt;&lt;/pre&gt;';
///   }
/// });
/// </code>
/// </summary>
[<ImportDefault("markdown-it")>]
let markdownit: MarkdownItConstructor = jsNative

[<AllowNullLiteral>]
type IExports =
    abstract Token: TokenStatic
    abstract Renderer: RendererStatic
    /// <summary>
    /// Helper class, used by <see cref="MarkdownIt.core" />, <see cref="MarkdownIt.block" /> and
    /// <see cref="MarkdownIt.inline" /> to manage sequences of functions (rules):
    ///
    /// - keep rules in defined order
    /// - assign the name to each rule
    /// - enable/disable rules
    /// - add/replace rules
    /// - allow assign rules to additional named chains (in the same)
    /// - caching lists of active rules
    ///
    /// You will not need use this class directly until write plugins. For simple
    /// rules control use <see cref="MarkdownIt.disable" />, <see cref="MarkdownIt.enable" /> and
    /// <see cref="MarkdownIt.use" />.
    /// </summary>
    abstract Ruler: RulerStatic
    abstract StateCore: StateCoreStatic
    abstract StateBlock: StateBlockStatic
    abstract StateInline: StateInlineStatic
    abstract Core: CoreStatic
    abstract ParserBlock: ParserBlockStatic
    abstract ParserInline: ParserInlineStatic
    abstract MarkdownItConstructor: MarkdownItConstructorStatic

[<AllowNullLiteral>]
type Token =
    /// Type of the token, e.g. "paragraph_open"
    abstract ``type``: string with get, set
    /// HTML tag name, e.g. "p"
    abstract tag: string with get, set
    /// <summary>HTML attributes. Format: <c>[ [ name1, value1 ], [ name2, value2 ] ]</c></summary>
    abstract attrs: Array<string * string> option with get, set
    /// <summary>Source map info. Format: <c>[ line_begin, line_end ]</c></summary>
    abstract map: int * int option with get, set
    /// <summary>
    /// Level change (number in {-1, 0, 1} set), where:
    ///
    /// -  <c>1</c> means the tag is opening
    /// -  <c>0</c> means the tag is self-closing
    /// - <c>-1</c> means the tag is closing
    /// </summary>
    abstract nesting: MarkdownIt.Token.Nesting with get, set
    /// <summary>Nesting level, the same as <c>state.level</c></summary>
    abstract level: int with get, set
    /// An array of child nodes (inline and img tokens)
    abstract children: ResizeArray<Token> option with get, set
    /// In a case of self-closing tag (code, html, fence, etc.),
    /// it has contents of this tag.
    abstract content: string with get, set
    /// '*' or '_' for emphasis, fence string for fence, etc.
    abstract markup: string with get, set
    /// - Info string for "fence" tokens
    /// - The value "auto" for autolink "link_open" and "link_close" tokens
    /// - The string value of the item marker for ordered-list "list_item_open" tokens
    abstract info: string with get, set
    /// A place for plugins to store an arbitrary data
    abstract meta: obj option with get, set
    /// True for block-level tokens, false for inline tokens.
    /// Used in renderer to calculate line breaks
    abstract block: bool with get, set
    /// If it's true, ignore this element when rendering. Used for tight lists
    /// to hide paragraphs.
    abstract hidden: bool with get, set
    /// Search attribute index by name.
    abstract attrIndex: name: string -> int
    /// <summary>Add <c>[ name, value ]</c> attribute to list. Init attrs if necessary</summary>
    abstract attrPush: attrData: string * string -> unit
    /// <summary>Set <c>name</c> attribute to <c>value</c>. Override old value if exists.</summary>
    abstract attrSet: name: string * value: string -> unit
    /// <summary>Get the value of attribute <c>name</c>, or null if it does not exist.</summary>
    abstract attrGet: name: string -> string option
    /// Join value to existing attribute via space. Or create new attribute if not
    /// exists. Useful to operate with token classes.
    abstract attrJoin: name: string * value: string -> unit

[<AllowNullLiteral>]
type TokenStatic =
    /// Create new token and fill passed properties.
    [<EmitConstructor>]
    abstract Create: ``type``: string * tag: string * nesting: MarkdownIt.Token.Nesting -> Token

type Token_ = Token

[<AllowNullLiteral>]
type Renderer =
    /// <summary>
    /// Contains render rules for tokens. Can be updated and extended.
    ///
    /// ##### Example
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')();
    ///
    /// md.renderer.rules.strong_open  = function () { return '&lt;b&gt;'; };
    /// md.renderer.rules.strong_close = function () { return '&lt;/b&gt;'; };
    ///
    /// var result = md.renderInline(...);
    /// </code>
    ///
    /// Each rule is called as independent static function with fixed signature:
    ///
    /// <code lang="javascript">
    /// function my_token_render(tokens, idx, options, env, renderer) {
    ///   // ...
    ///   return renderedHTML;
    /// }
    /// </code>
    /// </summary>
    /// <seealso href="https://github.com/markdown-it/markdown-it/blob/master/lib/renderer.mjs" />
    abstract rules: MarkdownIt.Renderer.RenderRuleRecord with get, set
    /// Render token attributes to string.
    abstract renderAttrs: token: Token -> string
    /// <summary>
    /// Default token renderer. Can be overriden by custom function
    /// in <see cref="Renderer.rules" />.
    /// </summary>
    /// <param name="tokens">list of tokens</param>
    /// <param name="idx">token index to render</param>
    /// <param name="options">params of parser instance</param>
    abstract renderToken: tokens: ResizeArray<Token> * idx: int * options: MarkdownIt.Options -> string
    /// <summary>The same as <see cref="Renderer.render" />, but for single token of <c>inline</c> type.</summary>
    /// <param name="tokens">list of block tokens to render</param>
    /// <param name="options">params of parser instance</param>
    /// <param name="env">additional data from parsed input (references, for example)</param>
    abstract renderInline: tokens: ResizeArray<Token> * options: MarkdownIt.Options * env: obj option -> string
    /// <summary>
    /// Special kludge for image <c>alt</c> attributes to conform CommonMark spec.
    /// Don't try to use it! Spec requires to show <c>alt</c> content with stripped markup,
    /// instead of simple escaping.
    /// </summary>
    /// <param name="tokens">list of block tokens to render</param>
    /// <param name="options">params of parser instance</param>
    /// <param name="env">additional data from parsed input (references, for example)</param>
    abstract renderInlineAsText: tokens: ResizeArray<Token> * options: MarkdownIt.Options * env: obj option -> string
    /// <summary>
    /// Takes token stream and generates HTML. Probably, you will never need to call
    /// this method directly.
    /// </summary>
    /// <param name="tokens">list of block tokens to render</param>
    /// <param name="options">params of parser instance</param>
    /// <param name="env">additional data from parsed input (references, for example)</param>
    abstract render: tokens: ResizeArray<Token> * options: MarkdownIt.Options * env: obj option -> string

[<AllowNullLiteral>]
type RendererStatic =
    /// <summary>Creates new <see cref="Renderer" /> instance and fill <see cref="Renderer.rules" /> with defaults.</summary>
    [<EmitConstructor>]
    abstract Create: unit -> Renderer

type Renderer_ = Renderer

/// <summary>
/// Helper class, used by <see cref="MarkdownIt.core" />, <see cref="MarkdownIt.block" /> and
/// <see cref="MarkdownIt.inline" /> to manage sequences of functions (rules):
///
/// - keep rules in defined order
/// - assign the name to each rule
/// - enable/disable rules
/// - add/replace rules
/// - allow assign rules to additional named chains (in the same)
/// - caching lists of active rules
///
/// You will not need use this class directly until write plugins. For simple
/// rules control use <see cref="MarkdownIt.disable" />, <see cref="MarkdownIt.enable" /> and
/// <see cref="MarkdownIt.use" />.
/// </summary>
[<AllowNullLiteral>]
type Ruler<'T> =
    /// <summary>
    /// Replace rule by name with new function &amp; options. Throws error if name not
    /// found.
    ///
    /// ##### Example
    ///
    /// Replace existing typographer replacement rule with new one:
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')();
    ///
    /// md.core.ruler.at('replacements', function replace(state) {
    ///   //...
    /// });
    /// </code>
    /// </summary>
    /// <param name="name">rule name to replace.</param>
    /// <param name="fn">new rule function.</param>
    /// <param name="options">new rule options (not mandatory).</param>
    abstract at: name: string * fn: 'T * ?options: MarkdownIt.Ruler.RuleOptions -> unit
    /// <summary>
    /// Add new rule to chain before one with given name.
    ///
    /// ##### Example
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')();
    ///
    /// md.block.ruler.before('paragraph', 'my_rule', function replace(state) {
    ///   //...
    /// });
    /// </code>
    /// </summary>
    /// <seealso cref="Ruler.after">, <see cref="Ruler.push" /></seealso>
    /// <param name="beforeName">new rule will be added before this one.</param>
    /// <param name="ruleName">name of added rule.</param>
    /// <param name="fn">rule function.</param>
    /// <param name="options">rule options (not mandatory).</param>
    abstract before: beforeName: string * ruleName: string * fn: 'T * ?options: MarkdownIt.Ruler.RuleOptions -> unit
    /// <summary>
    /// Add new rule to chain after one with given name.
    ///
    /// ##### Example
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')();
    ///
    /// md.inline.ruler.after('text', 'my_rule', function replace(state) {
    ///   //...
    /// });
    /// </code>
    /// </summary>
    /// <seealso cref="Ruler.before">, <see cref="Ruler.push" /></seealso>
    /// <param name="afterName">new rule will be added after this one.</param>
    /// <param name="ruleName">name of added rule.</param>
    /// <param name="fn">rule function.</param>
    /// <param name="options">rule options (not mandatory).</param>
    abstract after: afterName: string * ruleName: string * fn: 'T * ?options: MarkdownIt.Ruler.RuleOptions -> unit
    /// <summary>
    /// Push new rule to the end of chain.
    ///
    /// ##### Example
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')();
    ///
    /// md.core.ruler.push('my_rule', function replace(state) {
    ///   //...
    /// });
    /// </code>
    /// </summary>
    /// <seealso cref="Ruler.before">, <see cref="Ruler.after" /></seealso>
    /// <param name="ruleName">name of added rule.</param>
    /// <param name="fn">rule function.</param>
    /// <param name="options">rule options (not mandatory).</param>
    abstract push: ruleName: string * fn: 'T * ?options: MarkdownIt.Ruler.RuleOptions -> unit
    /// <summary>
    /// Enable rules with given names. If any rule name not found - throw Error.
    /// Errors can be disabled by second param.
    ///
    /// Returns list of found rule names (if no exception happened).
    /// </summary>
    /// <seealso cref="Ruler.disable">, <see cref="Ruler.enableOnly" /></seealso>
    /// <param name="list">list of rule names to enable.</param>
    /// <param name="ignoreInvalid">set <c>true</c> to ignore errors when rule not found.</param>
    abstract enable: list: U2<string, ResizeArray<string>> * ?ignoreInvalid: bool -> ResizeArray<string>
    /// <summary>
    /// Enable rules with given names, and disable everything else. If any rule name
    /// not found - throw Error. Errors can be disabled by second param.
    /// </summary>
    /// <seealso cref="Ruler.disable">, <see cref="Ruler.enable" /></seealso>
    /// <param name="list">list of rule names to enable (whitelist).</param>
    /// <param name="ignoreInvalid">set <c>true</c> to ignore errors when rule not found.</param>
    abstract enableOnly: list: U2<string, ResizeArray<string>> * ?ignoreInvalid: bool -> unit
    /// <summary>
    /// Disable rules with given names. If any rule name not found - throw Error.
    /// Errors can be disabled by second param.
    ///
    /// Returns list of found rule names (if no exception happened).
    /// </summary>
    /// <seealso cref="Ruler.enable">, <see cref="Ruler.enableOnly" /></seealso>
    /// <param name="list">list of rule names to disable.</param>
    /// <param name="ignoreInvalid">set <c>true</c> to ignore errors when rule not found.</param>
    abstract disable: list: U2<string, ResizeArray<string>> * ?ignoreInvalid: bool -> ResizeArray<string>
    /// <summary>
    /// Return array of active functions (rules) for given chain name. It analyzes
    /// rules configuration, compiles caches if not exists and returns result.
    ///
    /// Default chain name is <c>''</c> (empty string). It can't be skipped. That's
    /// done intentionally, to keep signature monomorphic for high speed.
    /// </summary>
    abstract getRules: chainName: string -> ResizeArray<'T>

/// <summary>
/// Helper class, used by <see cref="MarkdownIt.core" />, <see cref="MarkdownIt.block" /> and
/// <see cref="MarkdownIt.inline" /> to manage sequences of functions (rules):
///
/// - keep rules in defined order
/// - assign the name to each rule
/// - enable/disable rules
/// - add/replace rules
/// - allow assign rules to additional named chains (in the same)
/// - caching lists of active rules
///
/// You will not need use this class directly until write plugins. For simple
/// rules control use <see cref="MarkdownIt.disable" />, <see cref="MarkdownIt.enable" /> and
/// <see cref="MarkdownIt.use" />.
/// </summary>
[<AllowNullLiteral>]
type RulerStatic =
    [<EmitConstructor>]
    abstract Create: unit -> Ruler<'T>

type Ruler_<'T> = Ruler<'T>

[<AllowNullLiteral>]
type StateCore =
    abstract src: string with get, set
    abstract env: obj option with get, set
    abstract tokens: ResizeArray<Token> with get, set
    abstract inlineMode: bool with get, set
    /// link to parser instance
    abstract md: MarkdownIt with get, set
    abstract Token: obj with get, set

[<AllowNullLiteral>]
type StateCoreStatic =
    [<EmitConstructor>]
    abstract Create: src: string * md: MarkdownIt * env: obj option -> StateCore

type StateCore_ = StateCore

[<AllowNullLiteral>]
type StateBlock =
    abstract src: string with get, set
    /// link to parser instance
    abstract md: MarkdownIt with get, set
    abstract env: obj option with get, set
    abstract tokens: ResizeArray<Token> with get, set
    /// line begin offsets for fast jumps
    abstract bMarks: ResizeArray<int> with get, set
    /// line end offsets for fast jumps
    abstract eMarks: ResizeArray<int> with get, set
    /// offsets of the first non-space characters (tabs not expanded)
    abstract tShift: ResizeArray<int> with get, set
    /// indents for each line (tabs expanded)
    abstract sCount: ResizeArray<int> with get, set
    /// <summary>
    /// An amount of virtual spaces (tabs expanded) between beginning
    /// of each line (bMarks) and real beginning of that line.
    ///
    /// It exists only as a hack because blockquotes override bMarks
    /// losing information in the process.
    ///
    /// It's used only when expanding tabs, you can think about it as
    /// an initial tab length, e.g. bsCount=21 applied to string <c>\t123</c>
    /// means first tab should be expanded to 4-21%4 === 3 spaces.
    /// </summary>
    abstract bsCount: ResizeArray<int> with get, set
    /// required block content indent (for example, if we are
    /// inside a list, it would be positioned after list marker)
    abstract blkIndent: int with get, set
    /// line index in src
    abstract line: int with get, set
    /// lines count
    abstract lineMax: int with get, set
    /// loose/tight mode for lists
    abstract tight: bool with get, set
    /// indent of the current dd block (-1 if there isn't any)
    abstract ddIndent: int with get, set
    /// indent of the current list block (-1 if there isn't any)
    abstract listIndent: int with get, set
    /// used in lists to determine if they interrupt a paragraph
    abstract parentType: MarkdownIt.StateBlock.ParentType with get, set
    abstract level: int with get, set
    /// Push new token to "stream".
    abstract push: ``type``: string * tag: string * nesting: MarkdownIt.Token.Nesting -> Token
    abstract isEmpty: line: int -> bool
    abstract skipEmptyLines: from: int -> int
    /// Skip spaces from given position.
    abstract skipSpaces: pos: int -> int
    /// Skip spaces from given position in reverse.
    abstract skipSpacesBack: pos: int * min: int -> int
    /// Skip char codes from given position
    abstract skipChars: pos: int * code: int -> int
    /// Skip char codes reverse from given position - 1
    abstract skipCharsBack: pos: int * code: int * min: int -> int
    /// cut lines range from source.
    abstract getLines: ``begin``: int * ``end``: int * indent: int * keepLastLF: bool -> string
    abstract Token: obj with get, set

[<AllowNullLiteral>]
type StateBlockStatic =
    [<EmitConstructor>]
    abstract Create: src: string * md: MarkdownIt * env: obj option * tokens: ResizeArray<Token> -> StateBlock

type StateBlock_ = StateBlock

[<AllowNullLiteral>]
type StateInline =
    abstract src: string with get, set
    abstract env: obj option with get, set
    abstract md: MarkdownIt with get, set
    abstract tokens: ResizeArray<Token> with get, set
    abstract tokens_meta: Array<MarkdownIt.StateInline.TokenMeta option> with get, set
    abstract pos: int with get, set
    abstract posMax: int with get, set
    abstract level: int with get, set
    abstract pending: string with get, set
    abstract pendingLevel: int with get, set
    /// Stores { start: end } pairs. Useful for backtrack
    /// optimization of pairs parse (emphasis, strikes).
    abstract cache: obj option with get, set
    /// List of emphasis-like delimiters for current tag
    abstract delimiters: ResizeArray<MarkdownIt.StateInline.Delimiter> with get, set
    /// Flush pending text
    abstract pushPending: unit -> Token
    /// Push new token to "stream".
    /// If pending text exists - flush it as text token
    abstract push: ``type``: string * tag: string * nesting: MarkdownIt.Token.Nesting -> Token
    /// <summary>
    /// Scan a sequence of emphasis-like markers, and determine whether
    /// it can start an emphasis sequence or end an emphasis sequence.
    /// </summary>
    /// <param name="start">position to scan from (it should point at a valid marker)</param>
    /// <param name="canSplitWord">determine if these markers can be found inside a word</param>
    abstract scanDelims: start: int * canSplitWord: bool -> MarkdownIt.StateInline.Scanned
    abstract Token: obj with get, set

[<AllowNullLiteral>]
type StateInlineStatic =
    [<EmitConstructor>]
    abstract Create: src: string * md: MarkdownIt * env: obj option * outTokens: ResizeArray<Token> -> StateInline

type StateInline_ = StateInline

[<AllowNullLiteral>]
type Core =
    /// <summary><see cref="Ruler" /> instance. Keep configuration of core rules.</summary>
    abstract ruler: Ruler<MarkdownIt.Core.RuleCore> with get, set
    /// Executes core chain rules.
    abstract ``process``: state: StateCore -> unit
    abstract State: obj with get, set

[<AllowNullLiteral>]
type CoreStatic =
    [<EmitConstructor>]
    abstract Create: unit -> Core

type Core_ = Core

[<AllowNullLiteral>]
type ParserBlock =
    /// <summary><see cref="Ruler" /> instance. Keep configuration of block rules.</summary>
    abstract ruler: Ruler<MarkdownIt.ParserBlock.RuleBlock> with get, set
    /// Generate tokens for input range
    abstract tokenize: state: StateBlock * startLine: int * endLine: int -> unit
    /// <summary>Process input string and push block tokens into <c>outTokens</c></summary>
    abstract parse: str: string * md: MarkdownIt * env: obj option * outTokens: ResizeArray<Token> -> unit
    abstract State: obj with get, set

[<AllowNullLiteral>]
type ParserBlockStatic =
    [<EmitConstructor>]
    abstract Create: unit -> ParserBlock

type ParserBlock_ = ParserBlock

[<AllowNullLiteral>]
type ParserInline =
    /// <summary><see cref="Ruler" /> instance. Keep configuration of inline rules.</summary>
    abstract ruler: Ruler<MarkdownIt.ParserInline.RuleInline> with get, set
    /// <summary>
    /// <see cref="Ruler" /> instance. Second ruler used for post-processing
    /// (e.g. in emphasis-like rules).
    /// </summary>
    abstract ruler2: Ruler<MarkdownIt.ParserInline.RuleInline2> with get, set
    /// <summary>
    /// Skip single token by running all rules in validation mode;
    /// returns <c>true</c> if any rule reported success
    /// </summary>
    abstract skipToken: state: StateInline -> unit
    /// Generate tokens for input range
    abstract tokenize: state: StateInline -> unit
    /// <summary>Process input string and push inline tokens into <c>outTokens</c></summary>
    abstract parse: str: string * md: MarkdownIt * env: obj option * outTokens: ResizeArray<Token> -> unit
    abstract State: obj with get, set

[<AllowNullLiteral>]
type ParserInlineStatic =
    [<EmitConstructor>]
    abstract Create: unit -> ParserInline

type ParserInline_ = ParserInline

/// <summary>
/// Main parser/renderer class.
///
/// ##### Usage
///
/// <code lang="javascript">
/// // node.js, "classic" way:
/// var MarkdownIt = require('markdown-it'),
///     md = new MarkdownIt();
/// var result = md.render('# markdown-it rulezz!');
///
/// // node.js, the same, but with sugar:
/// var md = require('markdown-it')();
/// var result = md.render('# markdown-it rulezz!');
///
/// // browser without AMD, added to "window" on script load
/// // Note, there are no dash.
/// var md = window.markdownit();
/// var result = md.render('# markdown-it rulezz!');
/// </code>
///
/// Single line rendering, without paragraph wrap:
///
/// <code lang="javascript">
/// var md = require('markdown-it')();
/// var result = md.renderInline('__markdown-it__ rulezz!');
/// </code>
///
/// ##### Example
///
/// <code lang="javascript">
/// // commonmark mode
/// var md = require('markdown-it')('commonmark');
///
/// // default mode
/// var md = require('markdown-it')();
///
/// // enable everything
/// var md = require('markdown-it')({
///   html: true,
///   linkify: true,
///   typographer: true
/// });
/// </code>
///
/// ##### Syntax highlighting
///
/// <code lang="js">
/// var hljs = require('highlight.js') // https://highlightjs.org/
///
/// var md = require('markdown-it')({
///   highlight: function (str, lang) {
///     if (lang &amp;&amp; hljs.getLanguage(lang)) {
///       try {
///         return hljs.highlight(lang, str, true).value;
///       } catch (__) {}
///     }
///
///     return ''; // use external default escaping
///   }
/// });
/// </code>
///
/// Or with full wrapper override (if you need assign class to <c>&lt;pre&gt;</c>):
///
/// <code lang="javascript">
/// var hljs = require('highlight.js') // https://highlightjs.org/
///
/// // Actual default values
/// var md = require('markdown-it')({
///   highlight: function (str, lang) {
///     if (lang &amp;&amp; hljs.getLanguage(lang)) {
///       try {
///         return '&lt;pre class="hljs"&gt;&lt;code&gt;' +
///                hljs.highlight(lang, str, true).value +
///                '&lt;/code&gt;&lt;/pre&gt;';
///       } catch (__) {}
///     }
///
///     return '&lt;pre class="hljs"&gt;&lt;code&gt;' + md.utils.escapeHtml(str) + '&lt;/code&gt;&lt;/pre&gt;';
///   }
/// });
/// </code>
/// </summary>
module MarkdownIt =

    [<AllowNullLiteral>]
    type Utils =
        abstract lib: {| mdurl: obj; ucmicro: obj option |} with get, set
        /// Merge objects
        abstract assign: obj: obj option * [<ParamArray>] from: obj option[] -> obj option
        abstract isString: obj: obj option -> bool
        abstract has: obj: obj option * key: KeyOfAny -> bool
        abstract unescapeMd: str: string -> string
        abstract unescapeAll: str: string -> string
        abstract isValidEntityCode: c: int -> bool
        abstract fromCodePoint: c: int -> string
        abstract escapeHtml: str: string -> string
        /// Remove element from array and put another array at those position.
        /// Useful for some operations with tokens
        abstract arrayReplaceAt: src: ResizeArray<'T> * pos: int * newElements: ResizeArray<'T> -> ResizeArray<'T>
        abstract isSpace: code: int -> bool
        /// Zs (unicode class) || [\t\f\v\r\n]
        abstract isWhiteSpace: code: int -> bool
        /// <summary>
        /// Markdown ASCII punctuation characters.
        ///
        /// !, ", #, $, %, &amp;, ', (, ), *, +, ,, -, ., /, :, ;, &lt;, =, &gt;, ?,
        /// </summary>
        /// <seealso href="http://spec.commonmark.org/0.15/#ascii-punctuation-character" />
        abstract isMdAsciiPunct: code: int -> bool
        /// Currently without astral characters support.
        abstract isPunctChar: ch: string -> bool
        abstract escapeRE: str: string -> string
        /// Helper to unify [reference labels].
        abstract normalizeReference: str: string -> string

    [<AllowNullLiteral>]
    type ParseLinkDestinationResult =
        abstract ok: bool with get, set
        abstract pos: int with get, set
        abstract str: string with get, set

    [<AllowNullLiteral>]
    type ParseLinkTitleResult =
        /// <summary>if <c>true</c>, this is a valid link title</summary>
        abstract ok: bool with get, set
        /// <summary>if <c>true</c>, this link can be continued on the next line</summary>
        abstract can_continue: bool with get, set
        /// <summary>if <c>ok</c>, it's the position of the first character after the closing marker</summary>
        abstract pos: int with get, set
        /// <summary>if <c>ok</c>, it's the unescaped title</summary>
        abstract str: string with get, set
        /// expected closing marker character code
        abstract marker: int with get, set

    [<AllowNullLiteral>]
    type Helpers =
        abstract parseLinkLabel: state: StateInline * start: int * ?disableNested: bool -> int
        abstract parseLinkDestination: str: string * start: int * max: int -> ParseLinkDestinationResult

        abstract parseLinkTitle:
            str: string * start: int * max: int * ?prev_state: ParseLinkTitleResult -> ParseLinkTitleResult

    /// <summary>
    /// MarkdownIt provides named presets as a convenience to quickly
    /// enable/disable active syntax rules and options for common use cases.
    ///
    /// - <see href="https://github.com/markdown-it/markdown-it/blob/master/lib/presets/commonmark.js">"commonmark"</see> -
    ///   configures parser to strict <see href="http://commonmark.org/">CommonMark</see> mode.
    /// - <see href="https://github.com/markdown-it/markdown-it/blob/master/lib/presets/default.js">default</see> -
    ///   similar to GFM, used when no preset name given. Enables all available rules,
    ///   but still without html, typographer &amp; autolinker.
    /// - <see href="https://github.com/markdown-it/markdown-it/blob/master/lib/presets/zero.js">"zero"</see> -
    ///   all rules disabled. Useful to quickly setup your config via <c>.enable()</c>.
    ///   For example, when you need only <c>bold</c> and <c>italic</c> markup and nothing else.
    /// </summary>
    [<StringEnum>]
    [<RequireQualifiedAccess>]
    type PresetName =
        | Default
        | Zero
        | Commonmark

    type HighlightOptions = delegate of str: string * lang: string * attrs: string -> string

    [<AllowNullLiteral>]
    [<Global>]
    type Options
        [<ParamObject; Emit("$0")>]
        (
            ?html: bool,
            ?xhtmlOut: bool,
            ?breaks: bool,
            ?langPrefix: string,
            ?linkify: bool,
            ?typographer: bool,
            ?quotes: U2<string, ResizeArray<string>>,
            ?highlight: HighlightOptions
        ) =
        /// <summary>
        /// Set <c>true</c> to enable HTML tags in source. Be careful!
        /// That's not safe! You may need external sanitizer to protect output from XSS.
        /// It's better to extend features via plugins, instead of enabling HTML.
        /// </summary>
        /// <default>false</default>
        member val html: bool option = jsNative with get, set
        /// <summary>
        /// Set <c>true</c> to add '/' when closing single tags
        /// (<c>&lt;br /&gt;</c>). This is needed only for full CommonMark compatibility. In real
        /// world you will need HTML output.
        /// </summary>
        /// <default>false</default>
        member val xhtmlOut: bool option = jsNative with get, set
        /// <summary>Set <c>true</c> to convert <c>\n</c> in paragraphs into <c>&lt;br&gt;</c>.</summary>
        /// <default>false</default>
        member val breaks: bool option = jsNative with get, set
        /// <summary>
        /// CSS language class prefix for fenced blocks.
        /// Can be useful for external highlighters.
        /// </summary>
        /// <default>'language-'</default>
        member val langPrefix: string option = jsNative with get, set
        /// <summary>Set <c>true</c> to autoconvert URL-like text to links.</summary>
        /// <default>false</default>
        member val linkify: bool option = jsNative with get, set
        /// <summary>
        /// Set <c>true</c> to enable <see href="https://github.com/markdown-it/markdown-it/blob/master/lib/rules_core/replacements.js">some language-neutral replacement</see> +
        /// quotes beautification (smartquotes).
        /// </summary>
        /// <default>false</default>
        member val typographer: bool option = jsNative with get, set
        /// <summary>
        /// Double + single quotes replacement
        /// pairs, when typographer enabled and smartquotes on. For example, you can
        /// use <c>'«»„“'</c> for Russian, <c>'„“‚‘'</c> for German, and
        /// <c>['«\xA0', '\xA0»', '‹\xA0', '\xA0›']</c> for French (including nbsp).
        /// </summary>
        /// <default>'“”‘’'</default>
        member val quotes: U2<string, ResizeArray<string>> option = jsNative with get, set
        /// <summary>
        /// Highlighter function for fenced code blocks.
        /// Highlighter <c>function (str, lang, attrs)</c> should return escaped HTML. It can
        /// also return empty string if the source was not changed and should be escaped
        /// externally. If result starts with &lt;pre... internal wrapper is skipped.
        /// </summary>
        /// <default>null</default>
        member val highlight: HighlightOptions option = jsNative with get, set

    type Token = Token_

    module Token =
        /// Level change (number in {-1, 0, 1} set)
        [<RequireQualifiedAccess>]
        type Nesting =
            /// Closing tag
            | Closing = -1
            /// Self-closing tag
            | SelfClosing = 0
            /// Opening tag
            | Opening = 1

    type Renderer = Renderer_

    module Renderer =

        type RenderRule =
            delegate of
                tokens: ResizeArray<Token> * idx: int * options: Options * env: obj option * self: Renderer -> string

        [<AllowNullLiteral>]
        type RenderRuleRecord =
            [<EmitIndexer>]
            abstract Item: ``type``: string -> RenderRule option with get, set

            abstract code_inline: RenderRule option with get, set
            abstract code_block: RenderRule option with get, set
            abstract fence: RenderRule option with get, set
            abstract image: RenderRule option with get, set
            abstract hardbreak: RenderRule option with get, set
            abstract softbreak: RenderRule option with get, set
            abstract text: RenderRule option with get, set
            abstract html_block: RenderRule option with get, set
            abstract html_inline: RenderRule option with get, set

    type Ruler<'T> = Ruler_<'T>

    module Ruler =

        [<AllowNullLiteral>]
        type RuleOptions =
            /// array with names of "alternate" chains.
            abstract alt: ResizeArray<string> with get, set

    type StateCore = StateCore_

    type StateBlock = StateBlock_

    module StateBlock =

        [<StringEnum>]
        [<RequireQualifiedAccess>]
        type ParentType =
            | Blockquote
            | List
            | Root
            | Paragraph
            | Reference

    type StateInline = StateInline_

    module StateInline =

        [<AllowNullLiteral>]
        type Scanned =
            abstract can_open: bool with get, set
            abstract can_close: bool with get, set
            abstract length: int with get, set

        [<AllowNullLiteral>]
        type Delimiter =
            abstract marker: int with get, set
            abstract length: int with get, set
            abstract token: int with get, set
            abstract ``end``: int with get, set
            abstract ``open``: bool with get, set
            abstract close: bool with get, set

        [<AllowNullLiteral>]
        type TokenMeta =
            abstract delimiters: ResizeArray<Delimiter> with get, set

    type Core = Core_

    module Core =

        type RuleCore = delegate of state: StateCore -> unit

    type ParserBlock = ParserBlock_

    module ParserBlock =

        type RuleBlock = delegate of state: StateBlock * startLine: int * endLine: int * silent: bool -> bool

    type ParserInline = ParserInline_

    module ParserInline =

        type RuleInline = delegate of state: StateInline * silent: bool -> bool

        type RuleInline2 = delegate of state: StateInline -> bool

    type PluginSimple = delegate of md: MarkdownIt -> unit
    type PluginWithOptions<'T> = delegate of md: MarkdownIt * ?options: 'T -> unit
    type PluginWithOptions = PluginWithOptions<obj>
    type PluginWithParams = delegate of md: MarkdownIt * [<ParamArray>] ``params``: obj[] -> unit

[<AllowNullLiteral>]
type MarkdownItConstructor =
    [<Emit("$0($1...)")>]
    abstract Invoke: unit -> MarkdownIt

    [<Emit("$0($1...)")>]
    abstract Invoke: presetName: MarkdownIt.PresetName * ?options: MarkdownIt.Options -> MarkdownIt

    [<Emit("$0($1...)")>]
    abstract Invoke: options: MarkdownIt.Options -> MarkdownIt

[<AllowNullLiteral>]
type MarkdownItConstructorStatic =
    [<EmitConstructor>]
    abstract Create: unit -> MarkdownItConstructor

    [<EmitConstructor>]
    abstract Create: presetName: MarkdownIt.PresetName * ?options: MarkdownIt.Options -> MarkdownItConstructor

    [<EmitConstructor>]
    abstract Create: options: MarkdownIt.Options -> MarkdownItConstructor

/// <summary>
/// Main parser/renderer class.
///
/// ##### Usage
///
/// <code lang="javascript">
/// // node.js, "classic" way:
/// var MarkdownIt = require('markdown-it'),
///     md = new MarkdownIt();
/// var result = md.render('# markdown-it rulezz!');
///
/// // node.js, the same, but with sugar:
/// var md = require('markdown-it')();
/// var result = md.render('# markdown-it rulezz!');
///
/// // browser without AMD, added to "window" on script load
/// // Note, there are no dash.
/// var md = window.markdownit();
/// var result = md.render('# markdown-it rulezz!');
/// </code>
///
/// Single line rendering, without paragraph wrap:
///
/// <code lang="javascript">
/// var md = require('markdown-it')();
/// var result = md.renderInline('__markdown-it__ rulezz!');
/// </code>
///
/// ##### Example
///
/// <code lang="javascript">
/// // commonmark mode
/// var md = require('markdown-it')('commonmark');
///
/// // default mode
/// var md = require('markdown-it')();
///
/// // enable everything
/// var md = require('markdown-it')({
///   html: true,
///   linkify: true,
///   typographer: true
/// });
/// </code>
///
/// ##### Syntax highlighting
///
/// <code lang="js">
/// var hljs = require('highlight.js') // https://highlightjs.org/
///
/// var md = require('markdown-it')({
///   highlight: function (str, lang) {
///     if (lang &amp;&amp; hljs.getLanguage(lang)) {
///       try {
///         return hljs.highlight(lang, str, true).value;
///       } catch (__) {}
///     }
///
///     return ''; // use external default escaping
///   }
/// });
/// </code>
///
/// Or with full wrapper override (if you need assign class to <c>&lt;pre&gt;</c>):
///
/// <code lang="javascript">
/// var hljs = require('highlight.js') // https://highlightjs.org/
///
/// // Actual default values
/// var md = require('markdown-it')({
///   highlight: function (str, lang) {
///     if (lang &amp;&amp; hljs.getLanguage(lang)) {
///       try {
///         return '&lt;pre class="hljs"&gt;&lt;code&gt;' +
///                hljs.highlight(lang, str, true).value +
///                '&lt;/code&gt;&lt;/pre&gt;';
///       } catch (__) {}
///     }
///
///     return '&lt;pre class="hljs"&gt;&lt;code&gt;' + md.utils.escapeHtml(str) + '&lt;/code&gt;&lt;/pre&gt;';
///   }
/// });
/// </code>
/// </summary>
[<AllowNullLiteral>]
type MarkdownIt =
    /// <summary>
    /// Instance of <see cref="ParserInline" />. You may need it to add new rules when
    /// writing plugins. For simple rules control use <see cref="MarkdownIt.disable" /> and
    /// <see cref="MarkdownIt.enable" />.
    /// </summary>
    abstract ``inline``: ParserInline
    /// <summary>
    /// Instance of <see cref="ParserBlock" />. You may need it to add new rules when
    /// writing plugins. For simple rules control use <see cref="MarkdownIt.disable" /> and
    /// <see cref="MarkdownIt.enable" />.
    /// </summary>
    abstract block: ParserBlock
    /// <summary>
    /// Instance of <see cref="Core" /> chain executor. You may need it to add new rules when
    /// writing plugins. For simple rules control use <see cref="MarkdownIt.disable" /> and
    /// <see cref="MarkdownIt.enable" />.
    /// </summary>
    abstract core: Core
    /// <summary>
    /// Instance of <see cref="Renderer" />. Use it to modify output look. Or to add rendering
    /// rules for new token types, generated by plugins.
    ///
    /// ##### Example
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')();
    ///
    /// function myToken(tokens, idx, options, env, self) {
    ///   //...
    ///   return result;
    /// };
    ///
    /// md.renderer.rules['my_token'] = myToken
    /// </code>
    ///
    /// See <see cref="Renderer" /> docs and <see href="https://github.com/markdown-it/markdown-it/blob/master/lib/renderer.js">source code</see>.
    /// </summary>
    abstract renderer: Renderer
    /// <summary>
    /// <see href="https://github.com/markdown-it/linkify-it">linkify-it</see> instance.
    /// Used by <see href="https://github.com/markdown-it/markdown-it/blob/master/lib/rules_core/linkify.js">linkify</see>
    /// rule.
    /// </summary>
    abstract linkify: LinkifyIt.LinkifyIt
    /// <summary>
    /// Link validation function. CommonMark allows too much in links. By default
    /// we disable <c>javascript:</c>, <c>vbscript:</c>, <c>file:</c> schemas, and almost all <c>data:...</c> schemas
    /// except some embedded image types.
    ///
    /// You can change this behaviour:
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')();
    /// // enable everything
    /// md.validateLink = function () { return true; }
    /// </code>
    /// </summary>
    abstract validateLink: url: string -> bool
    /// Function used to encode link url to a machine-readable format,
    /// which includes url-encoding, punycode, etc.
    abstract normalizeLink: url: string -> string
    /// Function used to decode link url to a human-readable format`
    abstract normalizeLinkText: url: string -> string
    abstract utils: MarkdownIt.Utils
    abstract helpers: MarkdownIt.Helpers
    abstract options: MarkdownIt.Options
    /// <summary>
    /// *chainable*
    ///
    /// Set parser options (in the same format as in constructor). Probably, you
    /// will never need it, but you can change options after constructor call.
    ///
    /// ##### Example
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')()
    ///             .set({ html: true, breaks: true })
    ///             .set({ typographer: true });
    /// </code>
    ///
    /// __Note:__ To achieve the best possible performance, don't modify a
    /// <c>markdown-it</c> instance options on the fly. If you need multiple configurations
    /// it's best to create multiple instances and initialize each with separate
    /// config.
    /// </summary>
    abstract set: options: MarkdownIt.Options -> MarkdownIt
    /// <summary>
    /// *chainable*, *internal*
    ///
    /// Batch load of all options and compenent settings. This is internal method,
    /// and you probably will not need it. But if you with - see available presets
    /// and data structure <see href="https://github.com/markdown-it/markdown-it/tree/master/lib/presets">here</see>
    ///
    /// We strongly recommend to use presets instead of direct config loads. That
    /// will give better compatibility with next versions.
    /// </summary>
    abstract configure: presets: MarkdownIt.PresetName -> MarkdownIt
    /// <summary>
    /// *chainable*
    ///
    /// Enable list or rules. It will automatically find appropriate components,
    /// containing rules with given names. If rule not found, and <c>ignoreInvalid</c>
    /// not set - throws exception.
    ///
    /// ##### Example
    ///
    /// <code lang="javascript">
    /// var md = require('markdown-it')()
    ///             .enable(['sub', 'sup'])
    ///             .disable('smartquotes');
    /// </code>
    /// </summary>
    /// <param name="list">rule name or list of rule names to enable</param>
    /// <param name="ignoreInvalid">set <c>true</c> to ignore errors when rule not found.</param>
    abstract enable: list: U2<string, ResizeArray<string>> * ?ignoreInvalid: bool -> MarkdownIt
    /// <summary>
    /// *chainable*
    ///
    /// The same as <see cref="MarkdownIt.enable" />, but turn specified rules off.
    /// </summary>
    /// <param name="list">rule name or list of rule names to disable.</param>
    /// <param name="ignoreInvalid">set <c>true</c> to ignore errors when rule not found.</param>
    abstract disable: list: U2<string, ResizeArray<string>> * ?ignoreInvalid: bool -> MarkdownIt
    /// <summary>
    /// *chainable*
    ///
    /// Load specified plugin with given params into current parser instance.
    /// It's just a sugar to call <c>plugin(md, params)</c> with curring.
    ///
    /// ##### Example
    ///
    /// <code lang="javascript">
    /// var iterator = require('markdown-it-for-inline');
    /// var md = require('markdown-it')()
    ///             .use(iterator, 'foo_replace', 'text', function (tokens, idx) {
    ///               tokens[idx].content = tokens[idx].content.replace(/foo/g, 'bar');
    ///             });
    /// </code>
    /// </summary>
    abstract ``use``: plugin: MarkdownIt.PluginSimple -> MarkdownIt
    abstract ``use``: plugin: MarkdownIt.PluginWithOptions<'T> * ?options: 'T -> MarkdownIt
    abstract ``use``: plugin: MarkdownIt.PluginWithParams * [<ParamArray>] ``params``: obj[] -> MarkdownIt
    /// <summary>
    /// *internal*
    ///
    /// Parse input string and returns list of block tokens (special token type
    /// "inline" will contain list of inline tokens). You should not call this
    /// method directly, until you write custom renderer (for example, to produce
    /// AST).
    ///
    /// <c>env</c> is used to pass data between "distributed" rules and return additional
    /// metadata like reference info, needed for the renderer. It also can be used to
    /// inject data in specific cases. Usually, you will be ok to pass <c>{}</c>,
    /// and then pass updated object to renderer.
    /// </summary>
    /// <param name="src">source string</param>
    /// <param name="env">environment sandbox</param>
    abstract parse: src: string * env: obj option -> ResizeArray<Token>
    /// <summary>
    /// Render markdown string into html. It does all magic for you :).
    ///
    /// <c>env</c> can be used to inject additional metadata (<c>{}</c> by default).
    /// But you will not need it with high probability. See also comment
    /// in <see cref="MarkdownIt.parse" />.
    /// </summary>
    /// <param name="src">source string</param>
    /// <param name="env">environment sandbox</param>
    abstract render: src: string * ?env: obj -> string
    /// <summary>
    /// *internal*
    ///
    /// The same as <see cref="MarkdownIt.parse" /> but skip all block rules. It returns the
    /// block tokens list with the single <c>inline</c> element, containing parsed inline
    /// tokens in <c>children</c> property. Also updates <c>env</c> object.
    /// </summary>
    /// <param name="src">source string</param>
    /// <param name="env">environment sandbox</param>
    abstract parseInline: src: string * env: obj option -> ResizeArray<Token>
    /// <summary>
    /// Similar to <see cref="MarkdownIt.render" /> but for single paragraph content. Result
    /// will NOT be wrapped into <c>&lt;p&gt;</c> tags.
    /// </summary>
    /// <param name="src">source string</param>
    /// <param name="env">environment sandbox</param>
    abstract renderInline: src: string * ?env: obj -> string
