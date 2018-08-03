Imports System.Threading

Public Class Tracker
    Implements IDisposable

    Public Enum States
        Stopped
        Playing
        Paused
        Looping
    End Enum

    Public Property Name As String
    Public ReadOnly Property Patterns As New List(Of Pattern)

    Private mState As States
    Private playerThread As Thread
    Private abortThreads As Boolean

    Private patternIndex As Integer
    Private noteIndex As Integer
    Private noteInterval As Integer

    Public Sub New()
        Name = "<Untitled>"
        mState = States.Stopped
        Patterns.Add(New Pattern())

        playerThread = New Thread(AddressOf Player)
        playerThread.Start()
    End Sub

    Public Sub New(name As String)
        MyBase.New()
        name = name
    End Sub

    Public ReadOnly Property State As States
        Get
            Return mState
        End Get
    End Property

    Public Sub Play(looping As Boolean)
        If looping Then
            mState = States.Looping
        Else
            mState = States.Playing
        End If
    End Sub

    Public Sub Pause()
        If mState = States.Paused Then
            mState = States.Playing
        Else
            mState = States.Paused
        End If
    End Sub

    Public Sub [Stop]()
        mState = States.Stopped
    End Sub

    Public Sub Close()
        If Not abortThreads Then
            abortThreads = True
            [Stop]()
            Patterns.ForEach(Sub(p As Pattern) p.Close())
            Patterns.Clear()
        End If
    End Sub

    Private Sub Player()
        Dim sw As New Stopwatch()
        Dim lastPatternIndex As Integer = -1
        Dim delay As Integer

        Do
            Select Case mState
                Case States.Paused
                    Thread.Sleep(100)
                Case States.Stopped
                    patternIndex = 0
                    noteIndex = 0
                    lastPatternIndex = -1
                    Thread.Sleep(100)
                Case States.Playing, States.Looping
                    sw.Restart()

                    If lastPatternIndex <> patternIndex Then
                        noteInterval = 60000.0 / ((Patterns(patternIndex).BPM * Patterns(patternIndex).BeatResolution) / 2.0)
                        lastPatternIndex = patternIndex
                    End If

                    For Each c As Channel In Patterns(patternIndex).Channels
                        For Each n As TrackerNote In c.Notes
                            If n.Slot = noteIndex Then
                                n.Play(c)
                                Exit For
                            End If
                        Next
                    Next

                    noteIndex += 1
                    If noteIndex >= Patterns(patternIndex).BeatResolution Then
                        noteIndex = 0
                        patternIndex += 1
                        If patternIndex = Patterns.Count Then
                            If mState = States.Playing Then
                                mState = States.Stopped
                            Else
                                patternIndex = 0
                            End If
                        End If
                    End If

                    delay = noteInterval - sw.ElapsedMilliseconds
                    If delay > 0 Then Thread.Sleep(delay)
            End Select
        Loop Until abortThreads
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Close()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class