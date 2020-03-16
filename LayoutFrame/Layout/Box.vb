Imports LayoutFrame

Public Class BoxDim
    Public Property Left As Measure
    Public Property Top As Measure
    Public Property Right As Measure
    Public Property Bottom As Measure
    Public Sub New()
        Left = New NullMeasure
        Top = New NullMeasure
        Right = New NullMeasure
        Bottom = New NullMeasure
    End Sub
    Public Sub New(m As Measure)
        Top = m
        Left = m.Clone
        Bottom = m.Clone
        Right = m.Clone
    End Sub
    Public Sub New(h As Measure, w As Measure)
        Top = h
        Left = w
        Bottom = h.Clone
        Right = w.Clone
    End Sub
    Public Sub New(t As Measure, w As Measure, b As Measure)
        Top = t
        Left = w
        Bottom = b
        Right = w.Clone
    End Sub
    Public Sub New(t As Measure, l As Measure, b As Measure, r As Measure)
        Top = t
        Left = l
        Bottom = b
        Right = r
    End Sub
    '上左下右顺序
    Public Sub New(exp As String)
        Dim exps() As String = exp.Split(" "c)
        Dim indexes() As Integer
        Select Case exps.Count
            Case 1
                indexes = {0, 0, 0, 0}
            Case 2
                indexes = {0, 1, 0, 1}
            Case 3
                indexes = {0, 1, 2, 1}
            Case 4
                indexes = {0, 1, 2, 3}
            Case Else
                exps = {"0"}
                indexes = {0, 0, 0, 0}
        End Select
        Top = Measures.Parse(exps(indexes(0)))
        Left = Measures.Parse(exps(indexes(1)))
        Bottom = Measures.Parse(exps(indexes(2)))
        Right = Measures.Parse(exps(indexes(3)))
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("{0} {1} {2} {3}", Top, Left, Bottom, Right)
    End Function
End Class
Public Class Box
    Public Property Padding As New BoxDim
    Public Property Margin As New BoxDim

    '默认width,height为WeightMeasure(1.0F)
    Public Property Width As Measure = New WeightMeasure(1.0F)
    Public Property Height As Measure = New WeightMeasure(1.0F)


    Public ReadOnly Property CenterX As Single
        Get
            Return (RenderArea.Left + RenderArea.Right) * 0.5F
        End Get
    End Property
    Public ReadOnly Property CenterY As Single
        Get
            Return (RenderArea.Top + RenderArea.Bottom) * 0.5F
        End Get
    End Property

    Public ReadOnly Property ReltX As Single
        Get
            Return RenderArea.Left + ContentArea.Left
        End Get
    End Property
    Public ReadOnly Property ReltY As Single
        Get
            Return RenderArea.Top + ContentArea.Top
        End Get
    End Property



    Public ReadOnly Property TotalWidth As Single
        Get
            Return RenderArea.OriginRight + RenderEdge.Right
        End Get
    End Property
    Public ReadOnly Property TotalHeight As Single
        Get
            Return RenderArea.OriginBottom + RenderEdge.Bottom
        End Get
    End Property


    Friend fixedRenderEdge As New Edge2F
    Friend animatedRenderEdge As New Edge2F

    Friend fixedRenderArea As New Rect2F
    Friend fixedContentArea As New Rect2F
    Public Property RenderEdge As Edge2F = fixedRenderEdge
    Public Property RenderArea As Rect2F = fixedRenderArea
    Public Property ContentArea As Rect2F = fixedContentArea

    Public Sub New()

    End Sub
    Public Sub New(marginExp As String, paddingExp As String, sizeExp As String)
        Margin = New BoxDim(marginExp)
        Padding = New BoxDim(paddingExp)
        Dim m() As Measure = Measures.ParseMany(sizeExp)
        Width = m(0)
        Height = m(1)
    End Sub

#Region "<Animation>"
    Friend animatedRenderArea As New Rect2F
    Friend animatedContentArea As New Rect2F
    Private animationCount As Integer
    Private animationIndex As Integer
    Public Sub BeginAnimate(lastBox As Box)
        animationCount += 1
        If animationCount = 1 Then
            animatedRenderArea.CopyRect(lastBox.RenderArea)
            animatedContentArea.CopyRect(lastBox.ContentArea)
            RenderArea = animatedRenderArea
            ContentArea = animatedContentArea
        End If
        RenderEdge = animatedRenderEdge
    End Sub
    Public Sub BeginAnimate()
        animationCount += 1
        If animationCount = 1 Then
            animatedRenderArea.CopyRect(fixedRenderArea)
            animatedContentArea.CopyRect(fixedContentArea)
            RenderArea = animatedRenderArea
            ContentArea = animatedContentArea
        End If
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
    Public Sub DoAnimatetion()
        animationIndex += 1
        If animationCount >= animationIndex Then animationIndex = 0
    End Sub
    Public Sub DoAnimatetion(lastBox As Box, t As Single)
        DoAnimatetion()
        animatedRenderArea.SetTransitionRect(lastBox.RenderArea, fixedRenderArea, t)
        animatedContentArea.SetTransitionRect(lastBox.ContentArea, fixedContentArea, t)
        Dim nt As Single = 1.0F - t
        animatedRenderEdge.SetTransistionEdge(lastBox.RenderEdge, fixedRenderEdge, t)
    End Sub
    Public Sub EndAnimate()
        animationCount -= 1
        If animationCount >= animationIndex Then animationIndex = 0
        If animationCount = 0 Then
            RenderArea = fixedRenderArea
            ContentArea = fixedContentArea
        End If
    End Sub

#End Region


    Public Sub Update(rel As Size2F, border As Single)
        Measures.UpdateDimension(rel, Orientation.Horizontal, {Margin.Left, Width, Margin.Right})
        Measures.UpdateDimension(rel, Orientation.Vertical, {Margin.Top, Height, Margin.Bottom})
        fixedRenderArea.SetPointSizeRect(Margin.Left.RenderLength, Margin.Top.RenderLength,
                                         Width.RenderLength, Height.RenderLength)
        fixedRenderEdge.SetEdge(Margin)
        Dim conSize As Size2F = fixedRenderArea.Size
        Measures.UpdateDimension(conSize, Orientation.Horizontal, {Padding.Left, Padding.Right})
        Measures.UpdateDimension(conSize, Orientation.Vertical, {Padding.Top, Padding.Bottom})
        fixedContentArea.SetMarginRect(conSize,
                                       Padding.Left.RenderLength + border, Padding.Top.RenderLength + border,
                                       Padding.Right.RenderLength + border, Padding.Bottom.RenderLength + border)
    End Sub
    Public Function SetLength(ori As Orientation, m As Measure) As Box
        If ori = Orientation.Horizontal Then
            Width = m
        Else
            Height = m
        End If
        Return Me
    End Function
End Class
