Imports System.Text.RegularExpressions

Public Class Measures
    Public Class MeasureTypeInfo
        Public Property UnitString As String
        Public Property NewFunction As Func(Of Single, Measure)
        Public Sub New(s As String, f As Func(Of Single, Measure))
            UnitString = s
            NewFunction = f
        End Sub
    End Class

    Private Shared measureTypes() As MeasureTypeInfo = {
        New MeasureTypeInfo("px",   Function(v As Single) New AbsoluteMeasure(v)),
        New MeasureTypeInfo("*",    Function(v As Single) New WeightMeasure(v)),
        New MeasureTypeInfo("%",    Function(v As Single) New PercentageRelativeMeasure(v * 0.01F)),
        New MeasureTypeInfo("w",    Function(v As Single) New WidthRelativeMeasure(v)),
        New MeasureTypeInfo("h",    Function(v As Single) New HeightRelativeMeasure(v)),
        New MeasureTypeInfo("auto", Function(v As Single) New WeightMeasure(1.0F)),
        New MeasureTypeInfo("null", Function(v As Single) New NullMeasure()),
        New MeasureTypeInfo("%w",   Function(v As Single) New WidthRelativeMeasure(v * 0.01F)),
        New MeasureTypeInfo("%h",   Function(v As Single) New HeightRelativeMeasure(v * 0.01F)),
        New MeasureTypeInfo("vw",   Function(v As Single) New ViewWidthRelativeMeasure(v)),
        New MeasureTypeInfo("vh",   Function(v As Single) New ViewHeightRelativeMeasure(v)),
        New MeasureTypeInfo("%v",   Function(v As Single) New PercentageViewRelativeMeasure(v * 0.01F)),
        New MeasureTypeInfo("%vw",  Function(v As Single) New ViewWidthRelativeMeasure(v * 0.01F)),
        New MeasureTypeInfo("%vh",  Function(v As Single) New ViewHeightRelativeMeasure(v * 0.01F))
    }
    Private Shared reg As New Regex("(-?\d*(?:\.\d+)?)(\D+)")

    Public Shared Property ViewHeight As Single
    Public Shared Property ViewWidth As Single
    Public Shared Function Parse(str As String) As Measure
        If String.Equals(str, "0") Then str = "null"
        Dim m As Match = reg.Match(str)
        Dim v As Single, u As String
        If m.Success Then
            v = If(m.Groups(1).Value.Length > 0, Val(m.Groups(1).Value), 1.0F)
            u = m.Groups(2).Value
            For Each t As MeasureTypeInfo In measureTypes
                If String.Equals(u, t.UnitString, StringComparison.OrdinalIgnoreCase) Then
                    Return t.NewFunction(v)
                End If
            Next
        End If
        Return New NullMeasure()
    End Function
    Public Shared Function ParseMany(str As String) As Measure()
        Dim exps() As String = str.Split(" "c)
        Dim res(exps.Length - 1) As Measure
        For i As Integer = 0 To exps.Length - 1
            res(i) = Parse(exps(i))
        Next
        Return res
    End Function
    Public Shared Function Auto() As Measure
        Return New WeightMeasure(1.0F)
    End Function
    Public Shared Sub UpdateDimension(rel As Size2F, o As Orientation, measures() As Measure)
        Dim totalLength As Single = rel.Length(o)
        Dim totalWeight As Single
        For Each m As Measure In measures
            If TypeOf m Is WeightMeasure Then
                totalWeight += m.Value
            Else
                m.RenderLength = m.GetPixels(rel, o)
                totalLength -= m.RenderLength
            End If
        Next
        For Each m As Measure In measures
            If TypeOf m Is WeightMeasure Then
                m.RenderLength = totalLength / totalWeight * m.Value
            End If
        Next
    End Sub
    Public Shared Sub UpdateMeasures(rel As Size2F, o As Orientation, measures() As Measure)
        For Each m As Measure In measures
            If TypeOf m Is WeightMeasure Then
                m.RenderLength = rel.Length(o)
            Else
                m.RenderLength = m.GetPixels(rel, o)
            End If
        Next
    End Sub
End Class
Public MustInherit Class Measure
    Public MustOverride Property Value As Single
    Public Property RenderLength As Single
    Public MustOverride Function GetPixels(rel As Size2F, ori As Orientation) As Single
    Public MustOverride Function Clone() As Measure
End Class

Public Class NullMeasure
    Inherits Measure
    Public Overrides Property Value As Single

    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        Return 0F
    End Function
    Public Overrides Function Clone() As Measure
        Return New NullMeasure()
    End Function
    Public Overrides Function ToString() As String
        Return "0"
    End Function
End Class
Public Class AbsoluteMeasure
    Inherits Measure
    Public Overrides Property Value As Single
    Public Sub New(tValue As Single)
        Value = tValue
    End Sub
    Public Overrides Function Clone() As Measure
        Return New AbsoluteMeasure(Value)
    End Function
    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        Return Value
    End Function
    Public Overrides Function ToString() As String
        Return String.Format("{0}px", Value)
    End Function
End Class
Public Class PercentageRelativeMeasure
    Inherits Measure
    Public Overrides Property Value As Single

    Public Sub New(Optional tValue As Single = 1.0F)
        Value = tValue
    End Sub
    Public Overrides Function Clone() As Measure
        Return New PercentageRelativeMeasure(Value)
    End Function
    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        Return rel.Length(ori) * Value
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("{0}%", Value * 100.0F)
    End Function
End Class
Public Class WidthRelativeMeasure
    Inherits Measure
    Public Overrides Property Value As Single

    Public Sub New(Optional tValue As Single = 1.0F)
        Value = tValue
    End Sub
    Public Overrides Function Clone() As Measure
        Return New WidthRelativeMeasure(Value)
    End Function
    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        Return rel.Width * Value
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("{0}w", Value)
    End Function
End Class
Public Class HeightRelativeMeasure
    Inherits Measure
    Public Overrides Property Value As Single

    Public Sub New(Optional tValue As Single = 1.0F)
        Value = tValue
    End Sub
    Public Overrides Function Clone() As Measure
        Return New HeightRelativeMeasure(Value)
    End Function
    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        Return rel.Height * Value
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("{0}h", Value)
    End Function
End Class
Public Class ViewWidthRelativeMeasure
    Inherits Measure
    Public Overrides Property Value As Single

    Public Sub New(Optional tValue As Single = 1.0F)
        Value = tValue
    End Sub
    Public Overrides Function Clone() As Measure
        Return New ViewWidthRelativeMeasure(Value)
    End Function
    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        Return Measures.ViewWidth * Value
    End Function
    Public Overrides Function ToString() As String
        Return String.Format("{0}vw", Value)
    End Function
End Class
Public Class ViewHeightRelativeMeasure
    Inherits Measure
    Public Overrides Property Value As Single
    Public Sub New(Optional tValue As Single = 1.0F)
        Value = tValue
    End Sub
    Public Overrides Function Clone() As Measure
        Return New ViewHeightRelativeMeasure(Value)
    End Function
    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        Return Measures.ViewHeight * Value
    End Function
    Public Overrides Function ToString() As String
        Return String.Format("{0}vh", Value)
    End Function
End Class
Public Class PercentageViewRelativeMeasure
    Inherits Measure
    Public Overrides Property Value As Single

    Public Sub New(Optional tValue As Single = 1.0F)
        Value = tValue
    End Sub
    Public Overrides Function Clone() As Measure
        Return New PercentageViewRelativeMeasure(Value)
    End Function
    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        If ori = Orientation.Horizontal Then
            Return Measures.ViewWidth * Value
        Else
            Return Measures.ViewHeight * Value
        End If
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("{0}%v", Value * 100.0F)
    End Function
End Class
Public Class WeightMeasure
    Inherits Measure
    Public Overrides Property Value As Single
    Public Sub New(tValue As Single)
        Value = tValue
    End Sub
    Public Overrides Function Clone() As Measure
        Return New WeightMeasure(Value)
    End Function
    Public Overrides Function GetPixels(rel As Size2F, ori As Orientation) As Single
        Return 0F
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("{0}*", Value)
    End Function
End Class
