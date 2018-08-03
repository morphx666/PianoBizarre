Imports SiSyTracker

Public Class FormMain
    Private t As New Tracker()

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim c As Channel

        With t.Patterns.Last()
            .BPM = 50
            '                       1   2   3   4
            .BeatResolution = 8   ' 0 1 2 3 4 5 6 7 8

            c = .Channels.Last()
            With .Channels.Last().Notes
                .Add(New TrackerNote(c, "E 5", 200, 0))
                .Add(New TrackerNote(c, "B 4", 200, 2))
                .Add(New TrackerNote(c, "C 5", 200, 3))
                .Add(New TrackerNote(c, "D 5", 200, 4))
                .Add(New TrackerNote(c, "C 5", 200, 6))
                .Add(New TrackerNote(c, "B 4", 200, 7))
            End With

            .Channels.Add(New Channel(t.Patterns.First()))
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "E 3", 200, 0))
                .Add(New TrackerNote(c, "E 3", 200, 4))
            End With
        End With

        t.Patterns.Add(New Pattern(t.Patterns.First()))
        With t.Patterns.Last()
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "A 4", 200, 0))
                .Add(New TrackerNote(c, "A 4", 200, 2))
                .Add(New TrackerNote(c, "C 5", 200, 3))
                .Add(New TrackerNote(c, "E 5", 200, 4))
                .Add(New TrackerNote(c, "D 5", 200, 6))
                .Add(New TrackerNote(c, "C 5", 200, 7))
            End With

            .Channels.Add(New Channel(t.Patterns.First()))
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "A 3", 200, 3))
                .Add(New TrackerNote(c, "A 3", 200, 4))
            End With
        End With

        t.Patterns.Add(New Pattern(t.Patterns.First()))
        With t.Patterns.Last()
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "B 4", 200, 0))
                .Add(New TrackerNote(c, "C 5", 200, 3))
                .Add(New TrackerNote(c, "D 5", 200, 4))
                .Add(New TrackerNote(c, "E 5", 200, 6))
            End With

            .Channels.Add(New Channel(t.Patterns.First()))
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "G#3", 200, 0))
                .Add(New TrackerNote(c, "E 3", 200, 4))
            End With
        End With

        t.Patterns.Add(New Pattern(t.Patterns.First()))
        With t.Patterns.Last()
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "C 5", 200, 0))
                .Add(New TrackerNote(c, "A 4", 200, 2))
                .Add(New TrackerNote(c, "A 4", 200, 4))
            End With

            .Channels.Add(New Channel(t.Patterns.First()))
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "A 3", 200, 0))
            End With
        End With

        t.Patterns.Add(New Pattern(t.Patterns.First()))
        With t.Patterns.Last()
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "D 5", 200, 2))
                .Add(New TrackerNote(c, "F 5", 200, 3))
                .Add(New TrackerNote(c, "A 5", 200, 4))
                .Add(New TrackerNote(c, "G 5", 200, 6))
                .Add(New TrackerNote(c, "F 5", 200, 7))
            End With

            .Channels.Add(New Channel(t.Patterns.First()))
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "D 3", 200, 0))
                .Add(New TrackerNote(c, "D 3", 200, 4))
            End With
        End With

        t.Patterns.Add(New Pattern(t.Patterns.First()))
        With t.Patterns.Last()
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "E 5", 200, 0))
                .Add(New TrackerNote(c, "C 5", 200, 4))
                .Add(New TrackerNote(c, "E 5", 200, 5))
                .Add(New TrackerNote(c, "D 5", 200, 6))
                .Add(New TrackerNote(c, "C 5", 200, 7))
            End With

            .Channels.Add(New Channel(t.Patterns.First()))
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "C 3", 200, 0))
                .Add(New TrackerNote(c, "C 3", 200, 4))
            End With
        End With

        t.Patterns.Add(New Pattern(t.Patterns.First()))
        With t.Patterns.Last()
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "B 4", 200, 0))
                .Add(New TrackerNote(c, "B 4", 200, 2))
                .Add(New TrackerNote(c, "C 5", 200, 4))
                .Add(New TrackerNote(c, "D 5", 200, 6))
                .Add(New TrackerNote(c, "E 5", 200, 7))
            End With

            .Channels.Add(New Channel(t.Patterns.First()))
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "E 3", 200, 0))
                .Add(New TrackerNote(c, "E 3", 200, 4))
            End With
        End With

        t.Patterns.Add(New Pattern(t.Patterns.First()))
        With t.Patterns.Last()
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "C 5", 200, 0))
                .Add(New TrackerNote(c, "A 4", 200, 2))
                .Add(New TrackerNote(c, "A 4", 200, 4))
            End With

            .Channels.Add(New Channel(t.Patterns.First()))
            c = .Channels.Last()
            With c.Notes
                .Add(New TrackerNote(c, "A 3", 200, 0))
            End With
        End With

        'For Each p In t.Patterns
        '    If p.Channels.Count > 1 Then p.Channels.RemoveAt(1)
        'Next

        t.Play(True)
    End Sub

    Private Sub FormMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        t.Close()
    End Sub
End Class
