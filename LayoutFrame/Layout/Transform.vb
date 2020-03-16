

Imports SharpDX.Mathematics.Interop


Public Class Transforms
    Public Shared Identity As New Matrix3x2(1.0F, 0F, 0F, 1.0F, 0F, 0F)
    Public Shared Function Rotate(theta As Double, X As Single, Y As Single) As Matrix3x2
        Dim sin As Single, cos As Single
        sin = Math.Sin(theta)
        cos = Math.Cos(theta)
        Return New Matrix3x2(cos, -sin, sin, cos, X - X * cos - Y * sin, Y + X * sin - Y * cos)
    End Function
    Public Shared Function Skew(k As Single, q As Single, X As Single, Y As Single) As Matrix3x2
        Return New Matrix3x2(1.0F + k * q, q, k, 1.0F, -k * q * X - k * Y, -q * X)
    End Function
    Public Shared Function Zoom(t As Single, X As Single, Y As Single) As Matrix3x2
        Return New Matrix3x2(t, 0F, 0F, t, X - t * X, Y - t * Y)
    End Function
    Public Shared Function Rotate(theta As Double) As Matrix3x2
        Dim sin As Single, cos As Single
        sin = Math.Sin(theta)
        cos = Math.Cos(theta)
        Return New Matrix3x2(cos, -sin, sin, cos, 0F, 0F)
    End Function
    Public Shared Function Skew(k As Single, q As Single) As Matrix3x2
        Return New Matrix3x2(1.0F + k * q, q, k, 1.0F, 0F, 0F)
    End Function
    Public Shared Function Zoom(t As Single) As Matrix3x2
        Return New Matrix3x2(t, 0F, 0F, t, 0F, 0F)
    End Function

    Public Shared Function Move(X As Single, Y As Single) As Matrix3x2
        Return New Matrix3x2(1.0F, 0F, 0F, 1.0F, X, Y)
    End Function
End Class

Public Class Transform
    Public Property Enabled As Boolean
    Public Property Rotate As Double
    Public Property Zoom As Single = 1.0F
    Public Property MovementX As Single
    Public Property MovementY As Single

    Public animationCount As Integer
    Public animationIndex As Integer
    Public Sub BeginAnimation()
        animationCount += 1
    End Sub
    Public Sub DoAnimation()
        animationIndex += 1
        If animationIndex >= animationCount Then animationIndex = 0
    End Sub
    Public Sub EndAnimation()
        animationCount -= 1
        If animationIndex >= animationCount Then animationIndex = 0
    End Sub
    Public ReadOnly Property FirstAnimation As Boolean
        Get
            Return animationIndex = 1 OrElse animationCount = 1
        End Get
    End Property
    Public ReadOnly Property LastAnimation As Boolean
        Get
            Return animationIndex = 0
        End Get
    End Property

    Public ReadOnly Property Transformed As Boolean
        Get
            Return Enabled OrElse animationCount > 0
        End Get
    End Property

    Public Function GetMatrix(centerX As Single, centerY As Single) As Matrix3x2
        If Transformed Then
            Return Transforms.Move(-centerX, -centerY) * Transforms.Zoom(Zoom) *
                   Transforms.Rotate(Angle.Deg(Rotate)) * Transforms.Move(MovementX + centerX, MovementY + centerY)
        Else
            Return Transforms.Identity
        End If
    End Function
End Class