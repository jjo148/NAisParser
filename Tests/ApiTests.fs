namespace Tests

open NUnit.Framework
open FsUnit

open AisParser
open System

[<TestClass>]
type TestApi () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.TestApiNonFragmentInputReturnsTrue () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let mutable result = ref Ais.defaultAisResult
        let parser = Parser()

        // Act
        let result = parser.TryParse(input, result)

        // Assert
        result |> should be True

    [<Test>]
    member this.TestApiInvalidInputThrowsException () =
        // Arrage
        let input = "BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let mutable result = ref Ais.defaultAisResult
        let parser = Parser()
        let mutable raised = false
        // Act
        try
            parser.TryParse(input, result) |> ignore
        with
            | :? ArgumentException ->
                raised <- true

        // Assert
        raised |> should be True

    [<Test>]
    member this.TestApiFirstFragmentReturnsFalse () =
        // Arrage
        let input = "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
        let mutable result = ref Ais.defaultAisResult
        let parser = Parser()

        // Act
        let result = parser.TryParse(input, result)

        // Assert
        result |> should be False


    [<Test>]
    member this.TestApiLastFragmentReturnsTrue () =
        // Arrage
        let input: string [] = [|
            "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
            "!BSVDM,2,2,2,A,88888888880,2*3F";
            |]
        let mutable result = ref Ais.defaultAisResult
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input.[0], result)
        let result2 = parser.TryParse(input.[1], result)

        // Assert
        result1 |> should be False
        result2 |> should be True

    [<Test>]
    member this.TestApiNonFragmentInputSetsResult () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let mutable aisResult = ref Ais.defaultAisResult

        let parser = Parser()

        // Act

        let result = parser.TryParse(input, aisResult)

        // Assert
        result |> should be True
        aisResult.Value.Channel |> should equal Channel.A
        aisResult.Value.Vdm |> should equal TalkerId.BS
        aisResult.Value.Type |> should equal 1

    [<Test>]
    member this.TestApiParseType1IsSuccess () =
        // Arrage
        let input = "!BSVDM,1,1,,A,13mAwp001m0MMrjSoomG6mWT0<1h,0*16";
        let mutable aisResult = ref Ais.defaultAisResult
        let mutable cnbResult = ref Type123.defaultCommonNavigationBlockResult
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input, aisResult)
        let result2 = parser.TryParse(aisResult.Value, cnbResult)

        // Assert
        result2 |> should be True
        cnbResult.Value.Mmsi |> should equal 257196000
        cnbResult.Value.Latitude |> should equal 6.4372683333333329
        cnbResult.Value.Longitude |> should equal 62.692621666666668

    [<Test>]
    member this.TestApiParseType5IsSuccess () =
        // Arrage
        let input: string [] = [|
            "!BSVDM,2,1,2,A,53mDDD02>EjthmLJ220HtppE>2222222222222164@G:34rdR?QSkSQDp888,0*15";
            "!BSVDM,2,2,2,A,88888888880,2*3F";
            |]
        let mutable aisResult = ref Ais.defaultAisResult
        let mutable savResult = ref Type5.defaultStaticAndVoyageRelatedData
        let parser = Parser()

        // Act
        let result1 = parser.TryParse(input.[0], aisResult)
        let result2 = parser.TryParse(input.[1], aisResult)
        printfn "%A" aisResult
        let result3 = parser.TryParse(aisResult.Value, savResult)

        // Assert
        result1 |> should be False
        result2 |> should be True
        result3 |> should be True
        savResult.Value.Mmsi |> should equal 25719600
