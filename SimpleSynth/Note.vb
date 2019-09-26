''' <summary>
''' This is a helper class to convert from frequency values to musical notes and viceversa.
''' The note notation is as follows:
''' NOTE SHARP OCTAVE
''' Examples:
''' <list type="table">
''' <item>
''' <description>A 4 = 440.0000 Hz</description>
''' </item>
''' <item>
''' <description>C#4 = 277.1826 Hz</description>
''' </item>
''' </list> 
''' </summary>
Public Class Note
    ' FIXME: This whole code is horrendous!
    Private mFrequency As Double
    Private mValue As String

    Public Sub New()
    End Sub

    Public Sub New(value As String)
        Me.Value = value
    End Sub

    Public Sub New(frequency As Double)
        Me.Value = FrequencyToNote(frequency)
    End Sub

    Public Property Value As String
        Get
            Return mValue
        End Get
        Set(value As String)
            mValue = value
            mFrequency = NoteToFrequency(value)
        End Set
    End Property

    Public Property Frequency As Double
        Get
            Return mFrequency
        End Get
        Set(value As Double)
            ' https://en.wikipedia.org/wiki/Piano_key_frequencies
            Me.Value = FrequencyToNote(value)
        End Set
    End Property

    Public ReadOnly Property IsSharp As Boolean
        Get
            Return Value(1) = "#"
        End Get
    End Property

    Public Shared Function NoteToFrequency(note As String) As Double
        If note = "" Then Return 0

        Try
            Dim ns As String = note(0)
            Dim ss As String = note(1)
            Dim no As Integer = Integer.Parse(note(2))

            Dim n As Integer = "C D EF G A B".IndexOf(ns) + 1
            If ss = "#" Then n += 1
            n += no * 12 - 9

            Return (2 ^ ((n - 49) / 12.0)) * 440.0
        Catch
            Return 0
        End Try
    End Function

    Public Shared Function FrequencyToNote(f As Double) As Note
        If f = 0 Then Return ""

        Dim n As Integer = 12.0 * Math.Log10(f / 440.0) / Math.Log10(2) + 49
        n -= 1 - 9
        Dim ss As String = " # #  # # # "(n Mod 12)
        Dim ns As String = "CCDDEFFGGAAB"(n Mod 12)
        Dim no As Integer = n \ 12

        Return New Note($"{ns}{ss}{no}")
    End Function

    Public Shared Narrowing Operator CType(n As Note) As String
        Return n.Value
    End Operator

    Public Shared Narrowing Operator CType(n As Note) As Double
        Return n.Frequency
    End Operator

    Public Shared Widening Operator CType(n As String) As Note
        Return New Note(n)
    End Operator

    Public Shared Widening Operator CType(n As Double) As Note
        Return New Note(n)
    End Operator

    Public Shared Operator +(n As Note, i As Integer) As Note
        If n.Value = "" Then Return n
        Dim notes() = {"C ", "C#", "D ", "D#", "E ", "F ", "F#", "G ", "G#", "A ", "A#", "B ", "C "}
        Dim newNote As String = n.Value
        Dim s As Integer = If(i > 0, -1, 1)

        If s = 1 Then notes = notes.Reverse().ToArray()

        While i <> 0
            For j As Integer = 0 To notes.Length - 1
                If notes(j) = newNote.Substring(0, 2) Then
                    j += 1
                    If notes(j) = notes.Last() Then
                        newNote = $"{notes(j)}{Integer.Parse(newNote(2)) + (-s)}"
                    Else
                        newNote = $"{notes(j)}{Integer.Parse(newNote(2))}"
                    End If
                    Exit For
                End If
            Next
            i += s
        End While

        Return newNote
    End Operator

    Public Shared Operator -(n As Note, i As Integer) As Note
        Return n + (-i)
    End Operator

    Public Overrides Function ToString() As String
        Return Value
    End Function
End Class
