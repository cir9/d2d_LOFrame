
Imports LayoutFrame
Public Class Ease
    Public Class Linear
        Public Shared Ease As New EaseFunction(Function(t As Single) t)
    End Class
    Public Class Quad
        Public Shared EaseIn As New EaseFunction(Function(t As Single) As Single
                                                     Return t * t
                                                 End Function)
        Public Shared EaseOut As New EaseFunction(Function(t As Single) As Single
                                                      t = 1 - t
                                                      Return 1 - t * t
                                                  End Function)
        Public Shared EaseInOut As New EaseFunction(Function(t As Single) As Single
                                                        t *= 2
                                                        If t < 1 Then Return t * t * 0.5
                                                        t = 2 - t
                                                        Return 1 - t * t * 0.5
                                                    End Function)
    End Class
    Public Class Quart
        Public Shared EaseIn As New EaseFunction(Function(t As Single) As Single
                                                     Return t * t * t * t
                                                 End Function)
        Public Shared EaseOut As New EaseFunction(Function(t As Single) As Single
                                                      t = 1 - t
                                                      Return 1 - t * t * t * t
                                                  End Function)
        Public Shared EaseInOut As New EaseFunction(Function(t As Single) As Single
                                                        t *= 2
                                                        If t < 1 Then Return t * t * t * t * 0.5
                                                        t = 2 - t
                                                        Return 1 - t * t * t * t * 0.5
                                                    End Function)
    End Class

    Public Class Sept
        Public Shared EaseIn As New EaseFunction(Function(t As Single) As Single
                                                     Return t * t * t * t * t * t * t
                                                 End Function)
        Public Shared EaseOut As New EaseFunction(Function(t As Single) As Single
                                                      t = 1 - t
                                                      Return 1 - t * t * t * t * t * t * t
                                                  End Function)
    End Class

    Public Class Blend
        Public Shared Function ByWeight(e1 As EaseFunction, w1 As Single, e2 As EaseFunction, w2 As Single) As EaseFunction
            Return New EaseFunction(Function(t As Single) As Single
                                        Return (e1.Ease(t) * w1 + e2.Ease(t) * w2) / (w1 + w2)
                                    End Function)
        End Function
        Public Shared Function ByWeight(e() As EaseFunction, w() As Single) As EaseFunction
            Return New EaseFunction(Function(t As Single) As Single
                                        Dim totalWeight As Single
                                        Dim res As Single
                                        For i = 0 To e.Count - 1
                                            res += e(i).Ease(t) * w(i)
                                            totalWeight += w(i)
                                        Next
                                        Return res / totalWeight
                                    End Function)
        End Function
        Public Shared Function FromOneToOne(e1 As EaseFunction, e2 As EaseFunction) As EaseFunction
            Return New EaseFunction(Function(t As Single) As Single
                                        Return e1.Ease(t) * (1.0F - t) + e2.Ease(t) * t
                                    End Function)
        End Function
        Public Shared Function FromOneToOne(e1 As EaseFunction, e2 As EaseFunction, compEase As EaseFunction) As EaseFunction
            Return New EaseFunction(Function(t As Single) As Single
                                        Dim rt As Single = compEase.Ease(t)
                                        Return e1.Ease(t) * (1.0F - rt) + e2.Ease(t) * rt
                                    End Function)
        End Function
        Public Shared Function Compound(e1 As EaseFunction, e2 As EaseFunction) As EaseFunction
            Return New EaseFunction(Function(t As Single) As Single
                                        Return e1.Ease(e2.Ease(t))
                                    End Function)
        End Function
    End Class
End Class
Public Class EaseFunction
    Public Ease As Func(Of Single, Single)
    Public Sub New(tEase As Func(Of Single, Single))
        Ease = tEase
    End Sub
End Class

Public Class AnimationUtil
    Public Shared Running As Boolean = False
    Public Shared Animations As New CleanSet(Of Animation)

    Private Shared lastCleanTime As Single
    Private Shared cleanInterval As Single = 1000.0F
    Public Shared Sub Start(a As Animation)
        Animations.Add(a)
        a.StartRun()
    End Sub
    Public Shared Sub Start(anis As IEnumerable(Of Animation))
        Animations.AddRange(anis)
        For Each a As Animation In anis
            a.StartRun()
        Next
    End Sub
    Public Shared Sub Start(e As Element, anis As IEnumerable(Of Animation))
        Animations.AddRange(anis)
        For Each a As Animation In anis
            a.Target = e
            a.StartRun()
        Next
    End Sub
    Public Shared Sub Run()
        Running = True
        For Each a As Animation In Animations
            If Not a.Run Then
                Animations.Mark(a)
            End If
        Next
        If RenderUtil.Now - lastCleanTime > cleanInterval Then
            Animations.Clean()
            lastCleanTime = RenderUtil.Now
        End If

        'End SyncLock
        Running = False
    End Sub

End Class

Public MustInherit Class Animation
    Public Property Delay As Single
    Public Property EndTiming As Single
    Public Property EaseFunction As EaseFunction
    Public Property Running As Boolean = False
    Public Property Looping As Boolean = False
    Public Property FirstMove As Boolean = True

    Private startTimer As Double

    Protected _target As Element

    Public Property Target As Element
        Get
            Return _target
        End Get
        Set(value As Element)
            _target = value
            OnSetTarget()
        End Set
    End Property

    Protected MustOverride Sub Animate(t As Single)
    Protected MustOverride Sub OnEnd()
    Protected MustOverride Sub OnDelay()


    Public Sub New(et As Single, ease As EaseFunction)
        EndTiming = et
        EaseFunction = ease
    End Sub


    Public MustOverride Sub OnSetTarget()
    Public Sub StartRun()
        startTimer = RenderUtil.Now
        Running = True
        OnStart()
    End Sub

    Public MustOverride Sub OnStart()


    Public Function Run() As Boolean
        If Not Running Then Return False
        If RenderUtil.Now - startTimer - Delay > EndTiming Then
            If Looping Then
                startTimer += EndTiming
                Return Run()
            End If
            OnEnd()
            _target = Nothing
            Running = False
            Return False
        ElseIf RenderUtil.Now - startTimer > Delay Then
            Animate(EaseFunction.Ease((RenderUtil.Now - startTimer - Delay) / EndTiming))
            Return True
        Else
            OnDelay()
            Return True
        End If
    End Function
End Class
Public Class BoxAnimation
    Inherits Animation
    Private lastBox As Box, targetBox As Box

    Public Sub New(et As Single, ease As EaseFunction, targetbox As Box)
        MyBase.New(et, ease)
        Me.targetBox = targetbox
    End Sub
    Public Overrides Sub OnSetTarget()
        lastBox = _target.Box
        _target.Box = targetBox
    End Sub

    Public Overrides Sub OnStart()
        targetBox.BeginAnimate(lastBox)
        _target.Update()
    End Sub

    Protected Overrides Sub Animate(t As Single)
        If targetBox.LastAnimation Then
            ' Debug.Print(_target.Box.RenderArea.ToString)
            _target.UpdateBoundClip()
        End If
        targetBox.DoAnimatetion(lastBox, t)
        If targetBox.LastAnimation Then _target.Update()
    End Sub
    Protected Overrides Sub OnEnd()
        targetBox.RenderEdge = targetBox.fixedRenderEdge
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
        targetBox.EndAnimate()
        If targetBox.LastAnimation Then _target.Update()
        lastBox = Nothing
    End Sub

    Protected Overrides Sub OnDelay()
        targetBox.DoAnimatetion()
    End Sub
End Class
Public Class BoxInAnimation
    Inherits Animation
    Private targetBox As Box
    Public Property VectorX As Measure
    Public Property VectorY As Measure

    Public Sub New(et As Single, ease As EaseFunction, vX As Measure, vY As Measure)
        MyBase.New(et, ease)
        VectorX = vX
        VectorY = vY
    End Sub

    Public Overrides Sub OnSetTarget()
        targetBox = _target.Box
    End Sub
    Public Overrides Sub OnStart()
        targetBox.BeginAnimate()
        _target.Update()
    End Sub
    Protected Overrides Sub Animate(t As Single)
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
        targetBox.DoAnimatetion()
        _target.TempUpdateMeasure(VectorX, VectorY)
        Dim dX As Single, dY As Single
        Dim nt As Single = t - 1.0F
        dX = VectorX.RenderLength * nt
        dY = VectorY.RenderLength * nt
        targetBox.animatedRenderArea.SetDelta(dX, dY)
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
    End Sub
    Protected Overrides Sub OnEnd()
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
        targetBox.animatedRenderArea.SetDelta(0F, 0F)
        targetBox.fixedRenderArea.SetDelta(0F, 0F)
        targetBox.EndAnimate()
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
    End Sub
    Protected Overrides Sub OnDelay()
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
        targetBox.DoAnimatetion()
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
    End Sub
End Class
Public Class BoxOutAnimation
    Inherits Animation
    Private targetBox As Box
    Public Property VectorX As Measure
    Public Property VectorY As Measure

    Public Sub New(et As Single, ease As EaseFunction, vX As Measure, vY As Measure)
        MyBase.New(et, ease)
        VectorX = vX
        VectorY = vY
    End Sub

    Public Overrides Sub OnSetTarget()
        targetBox = _target.Box
    End Sub
    Public Overrides Sub OnStart()
        targetBox.BeginAnimate()
        _target.Update()
    End Sub
    Protected Overrides Sub Animate(t As Single)
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
        targetBox.DoAnimatetion()
        _target.TempUpdateMeasure(VectorX, VectorY)
        Dim dX As Single, dY As Single
        dX = VectorX.RenderLength * t
        dY = VectorY.RenderLength * t
        targetBox.animatedRenderArea.SetDelta(dX, dY)
        If targetBox.LastAnimation Then _target.Update()
    End Sub
    Protected Overrides Sub OnEnd()
        If targetBox.LastAnimation Then _target.UpdateBoundClip()
        targetBox.animatedRenderArea.SetDelta(VectorX.RenderLength, VectorY.RenderLength)
        targetBox.fixedRenderArea.SetDelta(VectorX.RenderLength, VectorY.RenderLength)
        targetBox.EndAnimate()
        If targetBox.LastAnimation Then _target.Update()
    End Sub
    Protected Overrides Sub OnDelay()
        targetBox.DoAnimatetion()
    End Sub
End Class
Public MustInherit Class TransformAnimation
    Inherits Animation
    Protected transform As Transform
    Public Sub New(et As Single, ease As EaseFunction)
        MyBase.New(et, ease)
    End Sub
    Public Overrides Sub OnSetTarget()
        transform = Target.Transform
    End Sub
    Protected MustOverride Sub WhenAnimating(t As Single)
    Protected MustOverride Sub WhenEnd()
    Public Overrides Sub OnStart()
        _target.UpdateBoundClip()
        transform.BeginAnimation()
    End Sub
    Protected Overrides Sub Animate(t As Single)

        transform.DoAnimation()
        If transform.FirstAnimation Then _target.UpdateBoundClip()
        WhenAnimating(t)
        If transform.LastAnimation Then _target.UpdateBoundClip()
    End Sub
    Protected Overrides Sub OnEnd()
        If transform.LastAnimation Then _target.UpdateBoundClip()
        WhenEnd()
        transform.EndAnimation()
        If transform.LastAnimation Then _target.UpdateBoundClip()

        transform = Nothing
    End Sub
    Protected Overrides Sub OnDelay()
        transform.DoAnimation()
        If transform.FirstAnimation Then _target.UpdateBoundClip()
        If transform.LastAnimation Then _target.UpdateBoundClip()
    End Sub
End Class
Public Class ZoomAnimation
    Inherits TransformAnimation
    Public Property BeginZoom As Single
    Public Property EndZoom As Single

    Public Sub New(et As Single, ease As EaseFunction, zoomTo As Single)
        MyBase.New(et, ease)
        EndZoom = zoomTo
    End Sub
    Public Overrides Sub OnSetTarget()
        MyBase.OnSetTarget()
        BeginZoom = transform.Zoom
    End Sub
    Protected Overrides Sub WhenAnimating(t As Single)
        transform.Zoom = BeginZoom * (1 - t) + EndZoom * t
    End Sub
    Protected Overrides Sub WhenEnd()
        transform.Zoom = EndZoom
        If EndZoom <> 1.0F Then transform.Enabled = True
    End Sub
End Class
Public Class RotateAnimation
    Inherits TransformAnimation
    Public Property BeginAngle As Double
    Public Property EndAngle As Double
    Public Property Round As Integer

    Public Sub New(et As Single, ease As EaseFunction, rotateTo As Double, Optional round As Integer = 0)
        MyBase.New(et, ease)
        Me.Round = round
        EndAngle = Angle.TrueDeg(rotateTo)
    End Sub

    Public Overrides Sub OnSetTarget()
        MyBase.OnSetTarget()
        BeginAngle = Angle.TrueDeg(transform.Rotate)
        If EndAngle - BeginAngle > 180.0 Then BeginAngle += 180.0
        EndAngle += round * 360.0
    End Sub
    Protected Overrides Sub WhenAnimating(t As Single)
        transform.Rotate = BeginAngle * (1 - t) + EndAngle * t
    End Sub
    Protected Overrides Sub WhenEnd()
        transform.Rotate = Angle.TrueDeg(EndAngle)
        If transform.Rotate <> 0F Then transform.Enabled = True
    End Sub
End Class
Public Class OpacityAnimation
    Inherits Animation
    Public Property BeginOpacity As Single
    Public Property EndOpacity As Single
    Public Sub New(et As Single, ease As EaseFunction, toOpa As Single)
        MyBase.New(et, ease)
        EndOpacity = toOpa
    End Sub

    Public Overrides Sub OnSetTarget()
        BeginOpacity = Target.Opacity
    End Sub
    Public Overrides Sub OnStart()
    End Sub
    Protected Overrides Sub Animate(t As Single)
        _target.Opacity = BeginOpacity * (1 - t) + EndOpacity * t
        _target.UpdateBoundClip()
    End Sub
    Protected Overrides Sub OnEnd()
        _target.Opacity = EndOpacity
        _target.UpdateBoundClip()
    End Sub
    Protected Overrides Sub OnDelay()
    End Sub
End Class
Public Class ColorAnimation
    Inherits Animation
    Public Property BeginColor As Color4F
    Public Property EndColor As Color4F

    Private _brush As Brush

    Public Property Brush As Brush
        Get
            Return _brush
        End Get
        Set(value As Brush)
            _brush = value
            BeginColor = _brush.Color
        End Set
    End Property
    Public Sub New(et As Single, ease As EaseFunction, toColor As Color4F)
        MyBase.New(et, ease)
        EndColor = toColor
    End Sub
    Public Overrides Sub OnSetTarget()
    End Sub
    Public Overrides Sub OnStart()
    End Sub

    Protected Overrides Sub Animate(t As Single)
        Brush.SetColor(BeginColor, EndColor, t)
        _target.UpdateBoundClip()
    End Sub
    Protected Overrides Sub OnEnd()
        brush.SetColor(EndColor)
        _target.UpdateBoundClip()

        _brush = Nothing
    End Sub
    Protected Overrides Sub OnDelay()
    End Sub
End Class