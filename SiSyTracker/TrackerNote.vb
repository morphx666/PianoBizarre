Imports System.ComponentModel.DataAnnotations
Imports System.Threading
Imports SimpleSynth

Public Class TrackerNote
    Public Property Note As Note

    <Range(0, Integer.MaxValue, ErrorMessage:="Value for {0} must be between {1} and {2}.")>
    Public Property Duration As Integer

    <Range(0, 1, ErrorMessage:="Value for {0} must be between {1} and {2}.")>
    Public Property Volume As Double

    Public Property Slot As Integer

    Public ReadOnly Property Channel As Channel

    Public Sub New(channel As Channel, note As String, duration As Integer, Optional slot As Integer = -1)
        Me.Channel = channel

        Me.Note = note
        Me.Duration = duration

        If slot <> -1 Then
            If Me.Channel.Notes.Any(Function(n As TrackerNote) n.Slot = slot) Then Throw New ArgumentException($"Slot {slot} already in use")
        ElseIf slot < 0 OrElse slot > Me.Channel.Pattern.BeatResolution Then
            Throw New ArgumentException($"Slot {slot} could not be fit into a pattern with a resolution of {Me.Channel.Pattern.BeatResolution}")
        End If

        Me.Slot = slot
    End Sub

    Public Sub Play(channel As Channel)
        Task.Run(Sub()
                     channel.Instrument.Frequency = Note.Frequency
                     Thread.Sleep(Duration)
                     channel.Instrument.Frequency = 0
                 End Sub)
    End Sub
End Class
