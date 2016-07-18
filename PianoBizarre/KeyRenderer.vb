Public Class KeyRenderer
    Public Enum KeyTypes
        White
        Black
    End Enum

    Public Enum KeyStates
        Pushed
        Released
    End Enum

    Public Bounds As New Rectangle()
    Public ReadOnly Property KeyType As KeyTypes
    Public ReadOnly Property Note As SimpleSynth.Note
    Public Property State As KeyStates

    Public Sub New(note As SimpleSynth.Note)
        Me.KeyType = If(note.IsSharp, KeyTypes.Black, KeyTypes.White)
        Me.Note = note
        State = KeyStates.Released
    End Sub

    Public Sub New(note As SimpleSynth.Note, bounds As Rectangle)
        Me.New(note)
        Me.Bounds = bounds
    End Sub

    Public Sub Render(g As Graphics)
        Using b As New SolidBrush(If(KeyType = KeyTypes.White, Color.White, Color.Black))
            g.FillRectangle(b, Bounds)
        End Using

        If State = KeyStates.Pushed Then
            Using b As New SolidBrush(Color.FromArgb(128, If(Note.IsSharp, Color.LightGray, Color.Gray)))
                g.FillRectangle(b, Bounds)
            End Using
        End If
    End Sub
End Class
