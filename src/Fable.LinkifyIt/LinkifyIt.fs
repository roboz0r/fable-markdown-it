// ts2fable 0.9.0
namespace rec LinkifyIt

#nowarn "3390" // disable warnings for invalid XML comments

open System
open Fable.Core
open Fable.Core.JS

type RegExp = System.Text.RegularExpressions.Regex


[<AllowNullLiteral>]
type IExports =
    /// <summary>Match result. Single element of array, returned by <see cref="LinkifyIt.match" />.</summary>
    abstract Match: MatchStatic
    abstract LinkifyIt: LinkifyItStatic

type Validate = delegate of text: string * pos: int * self: LinkifyIt -> U2<int, bool>

[<AllowNullLiteral>]
type FullRule =
    abstract validate: U3<string, RegExp, Validate> with get, set
    abstract normalize: (Match -> unit) option with get, set

type Rule = U2<string, FullRule>

/// <summary>
/// An object, where each key/value describes protocol/rule:
///
/// - __key__ - link prefix (usually, protocol name with <c>:</c> at the end, <c>skype:</c>
///   for example). <c>linkify-it</c> makes sure that prefix is not preceded with
///   alphanumeric char and symbols. Only whitespaces and punctuation allowed.
/// - __value__ - rule to check tail after link prefix
///   - _String_ - just alias to existing rule
///   - _Object_
///     - _validate_ - validator function (should return matched length on success),
///       or <c>RegExp</c>.
///     - _normalize_ - optional function to normalize text &amp; url of matched result
///       (for example, for <c>@twitter</c> mentions).
/// </summary>
[<AllowNullLiteral>]
type SchemaRules =
    [<EmitIndexer>]
    abstract Item: schema: string -> Rule with get, set

[<AllowNullLiteral>]
[<Global>]
type Options [<ParamObject; Emit("$0")>] (?fuzzyLink: bool, ?fuzzyEmail: bool, ?fuzzyIP: bool) =
    /// <summary>recognize URL-s without <c>http(s):</c> prefix. Default <c>true</c>.</summary>
    member val fuzzyLink: bool option = jsNative with get, set
    /// <summary>
    /// allow IPs in fuzzy links above. Can conflict with some texts
    /// like version numbers. Default <c>false</c>.
    /// </summary>
    member val fuzzyIP: bool option = jsNative with get, set
    /// <summary>recognize emails without <c>mailto:</c> prefix. Default <c>true</c>.</summary>
    member val fuzzyEmail: bool option = jsNative with get, set

/// <summary>Match result. Single element of array, returned by <see cref="LinkifyIt.match" />.</summary>
[<AllowNullLiteral>]
type Match =
    /// First position of matched string.
    abstract index: int with get, set
    /// Next position after matched string.
    abstract lastIndex: int with get, set
    /// Matched string.
    abstract raw: string with get, set
    /// Prefix (protocol) for matched string.
    abstract schema: string with get, set
    /// Normalized text of matched string.
    abstract text: string with get, set
    /// Normalized url of matched string.
    abstract url: string with get, set

/// <summary>Match result. Single element of array, returned by <see cref="LinkifyIt.match" />.</summary>
[<AllowNullLiteral>]
type MatchStatic =
    [<EmitConstructor>]
    abstract Create: self: LinkifyIt * shift: int -> Match

[<AllowNullLiteral>]
type LinkifyIt =
    /// <summary>Add new rule definition. See constructor description for details.</summary>
    /// <param name="schema">rule name (fixed pattern prefix)</param>
    /// <param name="definition">schema definition</param>
    abstract add: schema: string * definition: string -> LinkifyIt
    abstract add: schema: string * definition: FullRule option -> LinkifyIt
    /// Set recognition options for links without schema.
    abstract set: options: Options -> LinkifyIt
    /// <summary>Searches linkifiable pattern and returns <c>true</c> on success or <c>false</c> on fail.</summary>
    abstract test: text: string -> bool
    /// Very quick check, that can give false positives. Returns true if link MAY BE
    /// can exists. Can be used for speed optimization, when you need to check that
    /// link NOT exists.
    abstract pretest: text: string -> bool
    /// <summary>
    /// Similar to <see cref="LinkifyIt.test" /> but checks only specific protocol tail exactly
    /// at given position. Returns length of found pattern (0 on fail).
    /// </summary>
    /// <param name="text">text to scan</param>
    /// <param name="schema">rule (schema) name</param>
    /// <param name="pos">text offset to check from</param>
    abstract testSchemaAt: text: string * schema: string * pos: int -> int
    /// <summary>
    /// Returns array of found link descriptions or <c>null</c> on fail. We strongly
    /// recommend to use <see cref="LinkifyIt.test" /> first, for best speed.
    /// </summary>
    abstract ``match``: text: string -> array<Match> option
    /// Returns fully-formed (not fuzzy) link if it starts at the beginning
    /// of the string, and null otherwise.
    abstract matchAtStart: text: string -> Match option
    /// <summary>
    /// Load (or merge) new tlds list. Those are user for fuzzy links (without prefix)
    /// to avoid false positives. By default this algorythm used:
    ///
    /// - hostname with any 2-letter root zones are ok.
    /// - biz|com|edu|gov|net|org|pro|web|xxx|aero|asia|coop|info|museum|name|shop|рф
    ///   are ok.
    /// - encoded (<c>xn--...</c>) root zones are ok.
    ///
    /// If list is replaced, then exact match for 2-chars root zones will be checked.
    /// </summary>
    /// <param name="list">list of tlds</param>
    /// <param name="keepOld">merge with current list if <c>true</c> (<c>false</c> by default)</param>
    abstract tlds: list: U2<string, array<string>> * ?keepOld: bool -> LinkifyIt
    /// Default normalizer (if schema does not define it's own).
    abstract normalize: ``match``: Match -> unit
    /// Override to modify basic RegExp-s.
    abstract onCompile: unit -> unit
    abstract re: LinkifyItRe with get, set

[<AllowNullLiteral>]
type LinkifyItStatic =
    /// <summary>
    /// new LinkifyIt(schemas, options)
    /// - schemas (Object): Optional. Additional schemas to validate (prefix/validator)
    /// - options (Object): { fuzzyLink|fuzzyEmail|fuzzyIP: true|false }
    ///
    /// Creates new linkifier instance with optional additional schemas.
    /// Can be called without <c>new</c> keyword for convenience.
    ///
    /// By default understands:
    ///
    /// - <c>http(s)://...</c> , <c>ftp://...</c>, <c>mailto:...</c> &amp; <c>//...</c> links
    /// - "fuzzy" links and emails (example.com, foo@bar.com).
    /// </summary>
    [<EmitConstructor>]
    abstract Create: ?schemas: SchemaRules * ?options: Options -> LinkifyIt

    /// <summary>
    /// new LinkifyIt(schemas, options)
    /// - options (Object): { fuzzyLink|fuzzyEmail|fuzzyIP: true|false }
    ///
    /// Creates new linkifier instance with optional additional schemas.
    /// Can be called without <c>new</c> keyword for convenience.
    ///
    /// By default understands:
    ///
    /// - <c>http(s)://...</c> , <c>ftp://...</c>, <c>mailto:...</c> &amp; <c>//...</c> links
    /// - "fuzzy" links and emails (example.com, foo@bar.com).
    /// </summary>
    [<EmitConstructor>]
    abstract Create: ?options: Options -> LinkifyIt

[<AllowNullLiteral>]
type LinkifyItRe =
    [<EmitIndexer>]
    abstract Item: key: string -> RegExp with get, set
