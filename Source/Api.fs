namespace NAisParser

open System
open System.Collections.Generic
open System.Runtime.InteropServices //for OutAttribute

open FParsec
open NAisParser.Ais

type MessageType =
    | Type123 of MessageType123
    | Type4 of MessageType4
    | Type5 of MessageType5

exception ParseException of string

type ParseResult<'a> =
    | Ok of 'a
    | Error of exn
    | Continue

type public Parser() =
    let fragments = new List<ParserResult<AisResult, unit>>()

    /// <summary>
    /// Parses the AIS data packet outer layer. This is the first stage
    /// of parsing a VDM/VDO data packet.
    /// </summary>
    /// <param name="input">A string to parse.</param>
    /// <param name="result">
    /// An AisResult that will be set if the method returns true.
    /// </param>
    /// <returns>
    /// True if parsing of the packet completed, and the AisResult is valid.
    /// False if the packet is fragmented. If false then this method needs
    /// to be called again with the next packet fragment.
    /// </returns>
    /// <exception cref="System.ArgumentException">Thrown if the data
    /// packet is invalid and parsing of the packet failed.
    /// </exception>
    member public this.TryParse(input: String, [<Out>] result : AisResult byref) : bool =
        let res = run aisParser input

        match res with
        | Success (c, _, _) ->
            fragments.Add(res)

            if c.Number < c.Count then
                false
            else
                let fragment = fragments |> Seq.reduce defragment
                match fragment with
                | Success (c, _, _) ->
                    result <- { c with Type = byte c.Payload.[0]; Payload = c.Payload }
                    fragments.Clear()
                    true
                | Failure (error, _, _)  ->
                    raise (System.ArgumentException(error))
        | Failure (error, _, _)  ->
            raise (System.ArgumentException(error))

    /// <summary>
    /// Parses an AIS data packet inner layer of message type 1, 2 or 3.
    /// This is the second stage of parsing a VDM/VDO data packet.
    /// </summary>
    /// <param name="input">An AisResult to parse the payload of.</param>
    /// <param name="result">
    /// A MessageType123 that will be set if the method returns true.
    /// </param>
    /// <returns>
    /// True if parsing of the packet completed. False if this is not
    /// a message of type 1, 2, or 3.
    /// </returns>
    /// <exception cref="System.ArgumentException">Thrown if the data
    /// payload is invalid and parsing of the packet failed.
    /// </exception>
    member public this.TryParse(input: AisResult, [<Out>] result : MessageType123 byref) : bool =
        let binaryString = Common.intListToBinaryString input.Payload
        let res = run Type123.parseMessageType123 binaryString

        match res with
        | Success (message, _, _) ->
            result <- message
            true
        | Failure (error, _, _)  ->
            raise (System.ArgumentException(error))

    /// <summary>
    /// Parses an AIS data packet inner layer of message type 4.
    /// This is the second stage of parsing a VDM/VDO data packet.
    /// </summary>
    /// <param name="input">An AisResult to parse the payload of.</param>
    /// <param name="result">
    /// A MessageType5 that will be set if the method returns true.
    /// </param>
    /// <returns>
    /// True if parsing of the packet completed.
    /// </returns>
    /// <exception cref="System.ArgumentException">Thrown if the data
    /// payload is invalid and parsing of the packet failed.
    /// </exception>
    member public this.TryParse(input: AisResult, [<Out>] result : MessageType4 byref) : bool =
        let binaryString = Common.intListToBinaryString input.Payload
        let res = run Type4.parseMessageType4 binaryString

        match res with
        | Success (message, _, _) ->
            result <- message
            true
        | Failure (error, _, _)  ->
            raise (System.ArgumentException(error))

    /// <summary>
    /// Parses an AIS data packet inner layer of message type 5.
    /// This is the second stage of parsing a VDM/VDO data packet.
    /// </summary>
    /// <param name="input">An AisResult to parse the payload of.</param>
    /// <param name="result">
    /// A MessageType5 that will be set if the method returns true.
    /// </param>
    /// <returns>
    /// True if parsing of the packet completed.
    /// </returns>
    /// <exception cref="System.ArgumentException">Thrown if the data
    /// payload is invalid and parsing of the packet failed.
    /// </exception>
    member public this.TryParse(input: AisResult, [<Out>] result : MessageType5 byref) : bool =
        let binaryString = Common.intListToBinaryString input.Payload
        let res = run Type5.parseMessageType5 binaryString

        match res with
        | Success (message, _, _) ->
            result <- message
            true
        | Failure (error, _, _)  ->
            raise (System.ArgumentException(error))

    /// <summary>
    /// Parses the AIS data packet outer layer. This is the first stage
    /// of parsing a VDM/VDO data packet.
    /// </summary>
    /// <param name="input">A string to parse.</param>
    /// <param name="result">
    /// An AisResult that will be set if the method returns true.
    /// </param>
    /// <returns>
    /// True if parsing of the packet completed, and the AisResult is valid.
    /// False if the packet is fragmented. If false then this method needs
    /// to be called again with the next packet fragment.
    /// </returns>
    member public this.Parse (input: String) : ParseResult<AisResult> =
        let res = run aisParser input

        match res with
        | Success (c, _, _) ->
            fragments.Add(res)

            if c.Number < c.Count then
                Continue
            else
                let fragment = fragments |> Seq.reduce defragment
                match fragment with
                | Success (c, _, _) ->
                    let res = Ok { c with Type = byte c.Payload.[0]; Payload = c.Payload }
                    fragments.Clear()
                    res
                | Failure (error, _, _)  ->
                    Error (ParseException error)
        | Failure (error, _, _)  ->
            Error (ParseException error)

    member public this.ParseType123(input: AisResult) : Result<MessageType123, exn> =
        let binaryString = Common.intListToBinaryString input.Payload
        let res = run Type123.parseMessageType123 binaryString

        match res with
        | Success (message, _, _) ->
            Result.Ok message
        | Failure (error, err, _)  ->
            ParseException error |> Result.Error

    member public this.ParseType4(input: AisResult) : Result<MessageType4, exn> =
        let binaryString = Common.intListToBinaryString input.Payload
        let res = run Type4.parseMessageType4 binaryString

        match res with
        | Success (message, _, _) ->
            Result.Ok message
        | Failure (error, _, _)  ->
            ParseException error |> Result.Error

    member public this.ParseType5(input: AisResult) : Result<MessageType5, exn> =
        let binaryString = Common.intListToBinaryString input.Payload
        let res = run Type5.parseMessageType5 binaryString

        match res with
        | Success (message, _, _) ->
            Result.Ok message
        | Failure (error, err, _)  ->
            ParseException error |> Result.Error
