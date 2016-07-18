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
    Public Shared Function NoteToFrequency(note As String) As Double
        Dim ns As String = note(0)
        Dim ss As String = note(1)
        Dim no As Integer = Integer.Parse(note(2))

        Dim n As Integer = "C D EF G A B".IndexOf(ns) + 1
        If ss = "#" Then n += 1
        n += no * 12 - 9

        Return 2 ^ ((n - 49) / 12.0) * 440.0
    End Function

    Public Shared Function FrequencyToNote(f As Double) As String
        If f = 0 Then Return ""

        Dim n As Integer = 12.0 * Math.Log10(f / 440.0) / Math.Log10(2) + 49
        n -= 1 - 9
        Dim ss As String = " # #  # # # "(n Mod 12)
        Dim ns As String = "CCDDEFFGGAAB"(n Mod 12)
        Dim no As Integer = n \ 12

        Return $"{ns}{ss}{no}"
    End Function
End Class
