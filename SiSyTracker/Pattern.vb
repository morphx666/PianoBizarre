Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports SimpleSynth

Public Class Pattern
    Implements IDisposable

    Private am As AudioMixer

    Private mBeatResolution As Integer

    Public Property BPM As Integer
    Public ReadOnly Property Channels As New ObservableCollection(Of Channel)

    Public Sub New()
        'am = New AudioMixerSlimDX()
        am = New AudioMixerNAudio()

        BPM = 128
        mBeatResolution = 8 ' Time Signature: 4/4

        AddHandler Channels.CollectionChanged, Sub(s As Object, e As NotifyCollectionChangedEventArgs)
                                                   Select Case e.Action
                                                       Case NotifyCollectionChangedAction.Add
                                                           For Each item As Object In e.NewItems
                                                               am.BufferProviders.Add(CType(item, Channel).Instrument)
                                                           Next
                                                       Case NotifyCollectionChangedAction.Remove
                                                           Dim instrument As SignalGenerator
                                                           For Each item As Object In e.OldItems
                                                               instrument = CType(item, Channel).Instrument
                                                               instrument.Close()
                                                               am.BufferProviders.Remove(instrument)
                                                           Next
                                                       Case NotifyCollectionChangedAction.Reset
                                                           am.BufferProviders.ForEach(Sub(instrument As SignalGenerator) instrument.Close())
                                                           am.BufferProviders.Clear()
                                                   End Select
                                               End Sub

        Channels.Add(New Channel(Me))
    End Sub

    Public Sub New(inheritFrom As Pattern)
        Me.New()

        BPM = inheritFrom.BPM
        mBeatResolution = inheritFrom.BeatResolution
    End Sub

    Public Property BeatResolution As Integer
        Get
            Return mBeatResolution
        End Get
        Set(value As Integer)
            ' ReQuantize
            ReQuantize(mBeatResolution, value)
            mBeatResolution = value
        End Set
    End Property

    Public Property Volume As Double
        Get
            Return am.Volume
        End Get
        Set(value As Double)
            am.Volume = value
        End Set
    End Property

    Public Sub Close()
        Dispose()
    End Sub

    Private Sub ReQuantize(oldValue As Integer, newValue As Integer)
        Dim factor As Double = newValue / oldValue
        For Each c As Channel In Channels
            For i As Integer = 0 To c.Notes.Count - 1
                c.Notes(i).Slot *= factor
            Next
        Next
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                am.Close()
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
