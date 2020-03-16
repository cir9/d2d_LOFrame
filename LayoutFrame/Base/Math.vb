Imports SharpDX.Mathematics.Interop

Public Structure Angle
    Private Shared a2d As Double = 180.0F / Math.PI
    Private Shared d2a As Double = Math.PI / 180.0F
    Public Shared Function ArcToDegree(a As Double) As Double
        Return a * a2d
    End Function
    Public Shared Function Deg(d As Double) As Double
        Return d * d2a
    End Function
    Public Shared Function TrueDeg(d As Double) As Double
        Return d - Int(d / 360.0) * 360.0
    End Function
End Structure

Public Class MathF
    Public Shared Function Max(ParamArray fs() As Single) As Single
        Dim m As Single = fs(0)
        For i As Integer = 1 To fs.GetUpperBound(0)
            If fs(i) > m Then m = fs(i)
        Next
        Return m
    End Function

    Public Shared Function LetBetween(ByRef n As Single, a As Single, b As Single) As Boolean
        If n < a Then
            n = a
            Return True
        ElseIf n > b Then
            n = b
            Return True
        End If
        Return False
    End Function
    Public Shared Function Between(n As Single, a As Single, b As Single) As Boolean
        Return n >= a AndAlso n <= b
    End Function
    Public Shared Function Min(ParamArray fs() As Single) As Single
        Dim m As Single = fs(0)
        For i As Integer = 1 To fs.GetUpperBound(0)
            If fs(i) < m Then m = fs(i)
        Next
        Return m
    End Function
    Public Shared Function LetMax(ByRef a As Single, b As Single) As Boolean
        If b > a Then
            a = b
            Return True
        Else
            Return False
        End If
    End Function
    Public Shared Function LetMin(ByRef a As Single, b As Single) As Boolean
        If b < a Then
            a = b
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function LetMaxE(ByRef a As Single, b As Single) As Boolean
        If b >= a Then
            a = b
            Return True
        Else
            Return False
        End If
    End Function
    Public Shared Function LetMinE(ByRef a As Single, b As Single) As Boolean
        If b <= a Then
            a = b
            Return True
        Else
            Return False
        End If
    End Function
End Class

Public Structure Point2F
    Public X As Single
    Public Y As Single

    Public Sub New(nx As Single, ny As Single)
        X = nx
        Y = ny
    End Sub

    Public Shared Operator *(m As Matrix3x2, p As Point2F) As Point2F
        Return New Point2F(m.M11 * p.X + m.M21 * p.Y + m.M31, m.M12 * p.X + m.M22 * p.Y + m.M32)
    End Operator
End Structure

Public Structure Matrix3x2
    Public M11 As Single, M12 As Single, M21 As Single, M22 As Single, M31 As Single, M32 As Single
    Public Sub New(a11 As Single, a12 As Single, a21 As Single, a22 As Single, a31 As Single, a32 As Single)
        M11 = a11
        M12 = a12
        M21 = a21
        M22 = a22
        M31 = a31
        M32 = a32
    End Sub
    Public Function AddVector(X As Single, Y As Single) As Matrix3x2
        Return New Matrix3x2(M11, M12,
                             M21, M22,
                             M31 + M11 * X + M21 * Y, M32 + M12 * X + M22 * Y)
    End Function
    Public Function SubVector(X As Single, Y As Single) As Matrix3x2
        Return New Matrix3x2(M11, M12,
                             M21, M22,
                             M31 - M11 * X - M21 * Y, M32 - M12 * X - M22 * Y)
    End Function
    Public Shared Function Identity() As Matrix3x2
        Return New Matrix3x2(1.0F, 0F, 0F, 1.0F, 0F, 0F)
    End Function
    Public Shared Operator *(t As Single, m As Matrix3x2) As Matrix3x2
        Return New Matrix3x2(m.M11 * t, m.M12 * t, m.M21 * t, m.M22 * t, m.M31 * t, m.M32 * t)
    End Operator
    Public Shared Operator *(m As Matrix3x2, t As Single) As Matrix3x2
        Return New Matrix3x2(m.M11 * t, m.M12 * t, m.M21 * t, m.M22 * t, m.M31 * t, m.M32 * t)
    End Operator
    Public Shared Operator +(a As Matrix3x2, b As Matrix3x2) As Matrix3x2
        Return New Matrix3x2(a.M11 + b.M11, a.M12 + b.M12,
                             a.M21 + b.M21, a.M22 + b.M22,
                             a.M31 + b.M31, a.M32 + b.M32)
    End Operator
    Public Shared Operator *(b As Matrix3x2, a As Matrix3x2) As Matrix3x2
        Return New Matrix3x2(a.M11 * b.M11 + a.M21 * b.M12, a.M12 * b.M11 + a.M22 * b.M12,
                             a.M11 * b.M21 + a.M21 * b.M22, a.M12 * b.M21 + a.M22 * b.M22,
                             a.M11 * b.M31 + a.M21 * b.M32 + a.M31, a.M12 * b.M31 + a.M22 * b.M32 + a.M32)
    End Operator

    Public Shared Widening Operator CType(m As Matrix3x2) As RawMatrix3x2
        Return New RawMatrix3x2(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32)
    End Operator

    Public Shared Widening Operator CType(m As RawMatrix3x2) As Matrix3x2
        Return New Matrix3x2(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32)
    End Operator
End Structure