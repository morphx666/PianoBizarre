Imports System.ComponentModel.DataAnnotations

''' <summary>
''' This is the main component in <see cref="SimpleSynth"/> and it works by evaluating
''' a series of <see cref="BufferProvider"/>s, combining their results and
''' outputting the resulting waveform through a <see cref="SlimDX.DirectSound"/> audio buffer.
''' </summary>
Public MustInherit Class AudioMixer
    Implements IAudioMixer, IDisposable

    Public Const SampleRate As Integer = 44100

    Protected mAudioBuffer() As Integer

    Protected cancelAllThreads As Boolean

    Protected mVolume As Double = 1.0
    Protected mBufferProviders As New List(Of IBufferProvider)

    Public Shared ReadOnly Property SyncObject As New Object()

    Public Sub New()
        Initialize()
    End Sub

    ''' <summary>
    ''' 16bit normalized (<see cref="Short.MinValue"/> to <see cref="Short.MaxValue"/>) audio buffer
    ''' </summary>
    ''' <returns>Array of <see cref="Integer"/>s</returns>
    Public ReadOnly Property AudioBuffer As Integer() Implements IAudioMixer.AudioBuffer
        Get
            Return mAudioBuffer
        End Get
    End Property

    ''' <summary>
    ''' List of <see cref="BufferProvider"/>s 
    ''' </summary>
    ''' <returns>A list of <see cref="IBufferProvider"/>s</returns>
    Public ReadOnly Property BufferProviders As List(Of IBufferProvider) Implements IAudioMixer.BufferProviders
        Get
            Return mBufferProviders
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the attenuation value applied to the combined <see cref="BufferProviders"/> 
    ''' </summary>
    ''' <returns><see cref="Double"/></returns>
    <RangeAttribute(0.0, 1.0)>
    Public Property Volume As Double Implements IAudioMixer.Volume
        Get
            Return mVolume
        End Get
        Set(value As Double)
            mVolume = Math.Min(1, Math.Max(0, value))
        End Set
    End Property

    ''' <summary>
    ''' Use to gracefully dispose all used resources.
    ''' This member is called by the <see cref="Dispose()"/> method. 
    ''' </summary>
    Public Overridable Sub Close() Implements IAudioMixer.Close
        cancelAllThreads = True

        mBufferProviders.ForEach(Sub(bp) bp.Close())
    End Sub

    Protected MustOverride Sub Initialize()

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
