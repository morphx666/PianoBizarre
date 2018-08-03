Imports SiSyTracker

Public Class FormMain
    Private t As New Tracker()

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        With t.Patterns(0).Channels(0).Notes
            .Add(New TrackerNote(t.Patterns(0).Channels(0), "E 5", 500, 0))
            .Add(New TrackerNote(t.Patterns(0).Channels(0), "B 4", 200, 4))
        End With

        t.Play(False)
    End Sub

    Private Sub FormMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        t.Close()
    End Sub
End Class
