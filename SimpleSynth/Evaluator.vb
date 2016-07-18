Imports NCalc

''' <summary>
''' This class provides the means to programmatically evaluate formulas using the <see cref="NCalc"/> library.
''' Several functions, variables and constants, not natively available in NCalc, are provided to facilitate the
''' use of formulas for synthesis purposes:
''' Functions
''' <list type="bullet">
''' <item>
''' <term>Iif(boolean_condition, resultIfTrue, resultIfFalse)</term>
''' <description><see cref="IIf(Boolean, Object, Object)"/></description>
''' </item>
''' <item>
''' <term>ToRad(value)</term>
''' <description>Converts value to radians</description>
''' </item>
''' <item>
''' <term>Abs(value)</term>
''' <description>Returns the absolute value of value <see cref="Math.Abs(Double)"/> </description>
''' </item>
''' <item>
''' <term>Osc(frequency)</term>
''' <description></description>
''' </item>
''' <item>
''' <term>Rnd</term>
''' <description>Returns a random value between 0 and 1</description>
''' </item>
''' </list>
''' Constants
''' <list type="bullet">
''' <item>
''' <term>Pi</term>
''' <description><see cref="Math.PI"/> </description>
''' </item>
''' <item>
''' <term>e</term>
''' <description><see cref="Math.E"/> </description>
''' </item>
''' <item>
''' <term>t</term>
''' <description>Returns a value between -250 and 250 which represents the current 100th of a second?</description>
''' </item>
''' </list> 
''' </summary>
Public Class Evaluator
    Private Const min As Double = -1.0
    Private Const max As Double = 1.0

    Private mFormula As String

    Private customParameters As Dictionary(Of String, Double)

    Private e1 As Expression

    ''' <summary>
    ''' Gets or sets the formula to be evaluated
    ''' </summary>
    ''' <returns><see cref="String"/></returns>
    Public Property Formula As String
        Get
            Return mFormula
        End Get
        Set(value As String)
            mFormula = value
            If mFormula = "" Then mFormula = "0"
            e1 = New Expression(mFormula)

            AddHandler e1.EvaluateFunction, Sub(name As String, args As FunctionArgs)
                                                Select Case name
                                                    Case "IIf"
                                                        If args.Parameters(0).Evaluate() Then
                                                            args.Result = args.Parameters(1).Evaluate()
                                                        Else
                                                            args.Result = args.Parameters(2).Evaluate()
                                                        End If
                                                    Case "ToRad"
                                                        args.Result = args.Parameters(0).Evaluate() * Math.PI / 180
                                                    Case "Abs"
                                                        args.Result = Math.Abs(args.Parameters(0).Evaluate())
                                                    Case "Osc"
                                                        Dim f As Double = args.Parameters(0).Evaluate()
                                                        Dim f2 As Double = f / 2
                                                        Dim f4 As Double = f / 4
                                                        Dim t As Double = Now.Ticks Mod f
                                                        args.Result = If(t < f2, t - f4, f2 - t + f4) / f4
                                                    Case "Rnd"
                                                        args.Result = (New Random()).NextDouble()
                                                End Select
                                            End Sub

            AddHandler e1.EvaluateParameter, Sub(name As String, args As ParameterArgs)
                                                 Select Case name
                                                     Case "Pi"
                                                         args.Result = Math.PI
                                                     Case "e"
                                                         args.Result = Math.E
                                                     Case "t"
                                                         Dim t = Now.Ticks Mod 1000
                                                         args.Result = If(t < 500, t - 250, 500 - t + 250)
                                                     Case Else
                                                         If customParameters.ContainsKey(name) Then
                                                             args.Result = customParameters(name)
                                                         End If
                                                 End Select
                                             End Sub
        End Set
    End Property

    Public ReadOnly Property Variables As Dictionary(Of String, Object)
        Get
            If e1 Is Nothing Then
                Return Nothing
            Else
                Return e1.Parameters
            End If
        End Get
    End Property

    Public Function Evaluate(customParameters As Dictionary(Of String, Double)) As Double
        If e1 Is Nothing Then
            Return 0
        Else
            Me.customParameters = customParameters
            Return e1.Evaluate()
        End If
    End Function
End Class
