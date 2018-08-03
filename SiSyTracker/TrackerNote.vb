Imports System.ComponentModel.DataAnnotations
Imports System.Threading
Imports SimpleSynth

Public Class TrackerNote
    Public Property Note As Note

    <Range(0, Integer.MaxValue, ErrorMessage:="Value for {0} must be between {1} and {2}.")>
    Public Property Duration As Integer

    <Range(0, 1, ErrorMessage:="Value for {0} must be between {1} and {2}.")>
    Public Property Volume As Double

    Private mSlot As Integer

    Public ReadOnly Property Channel As Channel

    Public Sub New(channel As Channel, note As String, duration As Integer, Optional slot As Integer = -1)
        Me.Channel = channel

        Me.Note = note
        Me.Duration = duration

        If Me.Channel.Notes.Any(Function(n As TrackerNote) n.Slot = slot) Then Throw New ArgumentException($"Slot {slot} already in use")
        Me.Slot = slot
    End Sub

    Public Property Slot As Integer
        Get
            Return mSlot
        End Get
        Set(value As Integer)
            mSlot = value
        End Set
    End Property

    Public Sub Play(channel As Channel)
        Task.Run(Sub()
                     channel.Instrument.Frequency = Note.Frequency
                     Thread.Sleep(Duration)
                     channel.Instrument.Frequency = 0
                 End Sub)
    End Sub
End Class
