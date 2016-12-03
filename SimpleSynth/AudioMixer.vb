Imports SlimDX.DirectSound
Imports SlimDX.Multimedia
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.ComponentModel.DataAnnotations

''' <summary>
''' This is the main component in <see cref="SimpleSynth"/> and it works by evaluating
''' a series of <see cref="BufferProvider"/>s, combining their results and
''' outputting the resulting waveform through a <see cref="DirectSound"/> audio buffer.
''' </summary>
Public Class AudioMixer
    Implements IDisposable

    <DllImport("user32.dll", CharSet:=CharSet.Auto, ExactSpelling:=True)>
    Private Shared Function GetDesktopWindow() As IntPtr
    End Function

    Public Const SampleRate As Integer = 44100

    Private audioDev As DirectSound
    Private bufPlayDesc As SoundBufferDescription
    Private playBuf As SecondarySoundBuffer
    Private notifySize As Integer
    Private numberPlaybackNotifications As Integer = 4
    Private nextPlaybackOffset As Integer

    Private waiter As AutoResetEvent = New AutoResetEvent(False)

    Private mAudioBuffer() As Integer
    Private mainBuffer() As Byte

    Private audioWriteBufferPosition As Integer

    Private playbackThread As Thread
    Private cancelAllThreads As Boolean

    Private mVolume As Double = 1.0
    Private mBufferProviders As New List(Of IBufferProvider)

    Public Shared ReadOnly Property SyncObject As New Object()

    Public Sub New()
        Initialize()
    End Sub

    ''' <summary>
    ''' 16bit normalized (<see cref="Short.MinValue"/> to <see cref="Short.MaxValue"/>) audio buffer
    ''' </summary>
    ''' <returns>Array of <see cref="Integer"/>s</returns>
    Public ReadOnly Property AudioBuffer As Integer()
        Get
            Return mAudioBuffer
        End Get
    End Property

    ''' <summary>
    ''' List of <see cref="BufferProvider"/>s 
    ''' </summary>
    ''' <returns>A list of <see cref="IBufferProvider"/>s</returns>
    Public ReadOnly Property BufferProviders As List(Of IBufferProvider)
        Get
            Return mBufferProviders
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the attenuation value applied to the combined <see cref="BufferProviders"/> 
    ''' </summary>
    ''' <returns><see cref="Double"/></returns>
    <RangeAttribute(0.0, 1.0)>
    Public Property Volume As Double
        Get
            Return mVolume
        End Get
        Set(value As Double)
            mVolume = Math.Min(1, Math.Max(0, value))
        End Set
    End Property

    Private Sub MainLoop()
        Dim i As Integer

        Do
            waiter.WaitOne()

            SyncLock SyncObject
                For i = 0 To mBufferProviders.Count - 1
                    mBufferProviders(i).FillAudioBuffer(mAudioBuffer, i = 0)
                Next
            End SyncLock

            ' Hard clipping
            For i = 0 To mAudioBuffer.Length - 1
                Array.Copy(BitConverter.GetBytes(CShort(
                                                 Math.Min(
                                                    Short.MaxValue,
                                                    Math.Max(
                                                        Short.MinValue,
                                                        mAudioBuffer(i) * mVolume)))),
                           0,
                           mainBuffer,
                           i * 2, 2)
            Next

            WriteAudioBuffer()
        Loop Until cancelAllThreads
    End Sub

    ''' <summary>
    ''' Use to gracefully dispose all used resources.
    ''' This member is called by the <see cref="Dispose()"/> method. 
    ''' </summary>
    Public Sub Close()
        cancelAllThreads = True

        mBufferProviders.ForEach(Sub(bp) bp.Close())

        Do
            Thread.Sleep(10)
        Loop While playbackThread.ThreadState <> ThreadState.Stopped

        playBuf.Stop()
        waiter.Set()

        playBuf.Dispose()
        audioDev.Dispose()
    End Sub

    Private Sub Initialize()
        ReDim mAudioBuffer(SampleRate * 2 / 88 - 1)
        ReDim mainBuffer(mAudioBuffer.Length * 2 - 1)

        ' Define the capture format
        Dim format As WaveFormat = New WaveFormat()
        With format
            .BitsPerSample = 16
            .Channels = 2
            .FormatTag = WaveFormatTag.Pcm
            .SamplesPerSecond = SampleRate
            .BlockAlignment = CShort(.Channels * .BitsPerSample / 8)
            .AverageBytesPerSecond = .SamplesPerSecond * .BlockAlignment
        End With

        ' Define the size of the notification chunks
        notifySize = mainBuffer.Length
        notifySize -= notifySize Mod format.BlockAlignment

        ' Create a buffer description object
        bufPlayDesc = New SoundBufferDescription()
        With bufPlayDesc
            .Format = format
            .Flags = BufferFlags.ControlPositionNotify Or
                     BufferFlags.GetCurrentPosition2 Or
                     BufferFlags.GlobalFocus Or
                     BufferFlags.Static
            .SizeInBytes = notifySize * numberPlaybackNotifications
        End With

        audioDev = New DirectSound()
        audioDev.SetCooperativeLevel(GetDesktopWindow(), CooperativeLevel.Normal)
        playBuf = New SecondarySoundBuffer(audioDev, bufPlayDesc)

        ' Define the notification events
        Dim np(numberPlaybackNotifications - 1) As NotificationPosition

        For i As Integer = 0 To numberPlaybackNotifications - 1
            np(i) = New NotificationPosition()
            np(i).Offset = (notifySize * i) + notifySize - 1
            np(i).Event = waiter
        Next
        playBuf.SetNotificationPositions(np)

        nextPlaybackOffset = 0
        playBuf.Play(0, PlayFlags.Looping)

        playbackThread = New Thread(AddressOf MainLoop)
        playbackThread.Start()
    End Sub

    Private Sub WriteAudioBuffer()
        Dim lockSize As Integer

        lockSize = playBuf.CurrentWritePosition - nextPlaybackOffset
        If lockSize < 0 Then lockSize += bufPlayDesc.SizeInBytes

        ' Block align lock size so that we always read on a boundary
        lockSize -= lockSize Mod notifySize
        If lockSize = 0 Then Exit Sub

        playBuf.Write(Of Byte)(mainBuffer, nextPlaybackOffset, LockFlags.None)

        nextPlaybackOffset += mainBuffer.Length
        nextPlaybackOffset = nextPlaybackOffset Mod bufPlayDesc.SizeInBytes ' Circular buffer
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