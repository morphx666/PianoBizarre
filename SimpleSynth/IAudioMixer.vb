Public Interface IAudioMixer
    ReadOnly Property AudioBuffer As Integer()
    ReadOnly Property BufferProviders As List(Of IBufferProvider)
    Property Volume As Double
    Sub Close()
End Interface
