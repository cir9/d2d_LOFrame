
Imports LayoutFrame

Public Class Element
    Implements IComparable(Of Element)
#Region "<Properties>"
    Public Property Box As New Box
    Public Property Visible As Boolean = True
    Public Property Z_Index As Integer
    Public Property Name As String
    Public Property Transform As New Transform
    Public Property Level As Integer

    Public Property Row As Integer

#End Region
#Region "<INIT>"
    Public Property Inited As Boolean = False
    ''' <summary>
    ''' 初始化设备相关的对象。
    ''' </summary>
    ''' <param name="father">该元素的父容器</param>
    Public Overridable Sub Init(father As Div)
        Level = father.Level + 1
        Inited = True
        r = father.r
        BackgroundBrush.Init(r)
        BorderBrush.Init(r)
    End Sub
#End Region
#Region "<Colrors, Border, Opacity>"
    Public Property BorderWidth As Single = 0F
    Public Property BorderRadius As Single = 0F
    Public Property Opacity As Single = 1.0F

    Public BackgroundBrush As New Brush
    Public BorderBrush As New Brush
    Public Property BackgroundColor As Color4F
        Get
            Return BackgroundBrush.Color
        End Get
        Set(value As Color4F)
            BackgroundBrush.Color = value
            If Inited Then BackgroundBrush.Update()
        End Set
    End Property
    Public Property BorderColor As Color4F
        Get
            Return BorderBrush.Color
        End Get
        Set(value As Color4F)
            BorderBrush.Color = value
            If Inited Then BorderBrush.Update()
        End Set
    End Property
    Public Sub SetBackgroundColor(r As Single, g As Single, b As Single, a As Single)
        BackgroundBrush.SetColor(r, g, b, a)
    End Sub
    Public Sub SetBorderColor(r As Single, g As Single, b As Single, a As Single)
        BorderBrush.SetColor(r, g, b, a)
    End Sub
    Public ReadOnly Property WillDrawBorder As Boolean
        Get
            Return BorderWidth > 0F
        End Get
    End Property
#End Region
#Region "<Father>"
    Public WriteOnly Property Father As Div
        Set(value As Div)
            value.AddContent(Me)
        End Set
    End Property
#End Region
#Region "<Events>"
    Public Event OnUpdate(sender As Element)
    Public Event OnUpdateMe(sender As Element)
    Public Event OnClipUpdate(clip As Rect2F)
    Public Event OnTransformedClipUpdate(p() As Point2F)
#End Region
#Region "<Paint>"
    Protected Friend r As Renderer
    Protected RedrawClip As New Rect2F
    Public Sub BeginPaint()
        Static lastMatrix As Matrix3x2
        If Transform.Transformed Then
            lastMatrix = r.Matrix
            r.AddVector(Box.RenderArea.Left, Box.RenderArea.Top)
            r.Matrix = Transform.GetMatrix(Box.CenterX, Box.CenterY) * lastMatrix
            Paint_PrepareOpacity()
            r.Matrix = lastMatrix
        Else
            Paint_PrepareOpacity()
        End If
    End Sub
    Public Overridable Sub Paint_PrepareOpacity()
        If Opacity <> 1.0F Then
            r.PushLayer(Opacity)
            PaintBackground()
            Paint()
            PaintBorder()
            r.PopLayer()
        Else
            PaintBackground()
            Paint()
            PaintBorder()
        End If
    End Sub
    Public Sub PaintBackground()
        If BorderRadius = 0F Then
            BackgroundBrush.FillRectangle(Box.RenderArea)
        Else
            BackgroundBrush.FillRoundedRectangle(Box.RenderArea, BorderRadius)
        End If
    End Sub
    Public Overridable Sub Paint()
    End Sub
    Public Sub PaintBorder()
        If WillDrawBorder Then
            If BorderRadius = 0F Then
                BorderBrush.DrawRectangle(Box.RenderArea, BorderWidth)
            Else
                BorderBrush.DrawRoundedRectangle(Box.RenderArea, BorderRadius, BorderWidth)
            End If
        End If

        If BoundUsed Then
            BoundUsed = True
            InitClip()
        End If
    End Sub

#End Region
#Region "<TempSubs>"
    Protected Friend lastRel As Size2F
    Friend Sub TempUpdateMeasure(m As Measure, o As Orientation)
        m.RenderLength = m.GetPixels(lastRel, o)
    End Sub
    Friend Sub TempUpdateMeasure(mX As Measure, mY As Measure)
        Dim relSize As Size2F = lastRel
        mX.RenderLength = mX.GetPixels(relSize, Orientation.Horizontal)
        mY.RenderLength = mY.GetPixels(relSize, Orientation.Vertical)
    End Sub

#End Region
#Region "<Update>"
    Public Overridable Sub Update()
        RaiseEvent OnUpdate(Me)
    End Sub
    Public Overridable Sub UpdateMe()
        RaiseEvent OnUpdateMe(Me)
    End Sub

    Public Overridable Sub UpdateMe(rel As Size2F)
        UpdateBoundClip()
        Box.Update(rel, BorderWidth)
        lastRel = rel
        UpdateBoundClip()
    End Sub
    Public Overridable Sub Update(rel As Size2F)
        UpdateMe(rel)
    End Sub

#End Region
#Region "<BoundClip>"
    Protected boundBox As New Rect2F
    Public Property BoundUsed As Boolean = True
    Public Overridable Sub InitClip()
        boundBox.SetClipInit(Rect2F.Infinity)
    End Sub
    Public Sub UpdateBoundClipOfRect(rect As Rect2F)
        If Transform.Transformed Then
            Dim m As Matrix3x2 = Transform.GetMatrix(Box.CenterX, Box.CenterY)
            Dim p() As Point2F = rect.Points
            For i As Integer = 0 To 3
                p(i) = m * p(i)
            Next
            SendTransformedClipUpdate(p)
        Else
            SendClipUpdate(rect)
        End If
    End Sub
    Public Overridable Sub RecieveBoundClip(rX As Single, rY As Single, clip As Rect2F)
        BoundUsed = True
        If boundBox.SetClipBound(Box.ReltX + rX, Box.ReltY + rY, clip) Then
            UpdateBoundClipOfRect(boundBox)
        End If
    End Sub
    Public Overridable Sub RecieveBoundClip(clip As Rect2F)
        BoundUsed = True

        If boundBox.SetClipBound(Box.ReltX, Box.ReltY, clip) Then
            UpdateBoundClipOfRect(boundBox)
        End If
    End Sub
    Public Overridable Sub UpdateBoundClip()
        If BackgroundBrush.WillDraw OrElse BorderBrush.WillDraw Then
            UpdateBoundClipOfRect(Box.RenderArea)
        End If
    End Sub
    Public Sub SendClipUpdate(rect As Rect2F)
        RaiseEvent OnClipUpdate(rect)
    End Sub
    Public Sub SendTransformedClipUpdate(p() As Point2F)
        RaiseEvent OnTransformedClipUpdate(p)
    End Sub

#End Region
#Region "<Object>"
    Public Overrides Function ToString() As String
        Return String.Format("{0} {1}", Me.GetType, Name)
    End Function
    Public Function CompareTo(other As Element) As Integer Implements IComparable(Of Element).CompareTo
        Return Z_Index.CompareTo(other.Z_Index)
    End Function
#End Region
End Class
Public Class Div
    Inherits Element
#Region "<Properties>"
    Public Property Overflow As Boolean = True
    Public Property Contents As ICollection(Of Element)
    Protected sliderBrush As New Brush With {.Color = New Color4F(0F, 0F, 0F, 0.6F)}
#End Region
#Region "<INIT>"
    Public Overrides Sub Init(father As Div)
        MyBase.Init(father)
        sliderBrush.Init(r)
    End Sub
    Public Overridable Sub NewContents()
        Contents = New List(Of Element)
    End Sub
    Public Sub New()
        NewContents()
    End Sub
#End Region
#Region "<Scroll>"
    'Public Property ScrollWidth As Single
    Public Property ScrollTop As Single
    Public Property ScrollBottom As Single

    Protected sliderRect As New Rect2F
    Public Property ScrollX As Single
        Get
            Return -Box.ContentArea.DeltaX
        End Get
        Set(value As Single)
            Box.ContentArea.DeltaX = -value
        End Set
    End Property
    Public Property ScrollY As Single
        Get
            Return -Box.ContentArea.DeltaY
        End Get
        Set(value As Single)
            If ScrollHeight > Box.RenderArea.Height Then
                UpdateAllChildrenClip()
            End If
            Box.ContentArea.DeltaY = -value
            RecalculateScroll()
        End Set
    End Property
    Public ReadOnly Property ScrollHeight As Single
        Get
            Return ScrollBottom - ScrollTop
        End Get
    End Property
    Public ReadOnly Property ScrollAreaHeight As Single
        Get
            Return ScrollBottom - ScrollTop - Box.RenderArea.Height
        End Get
    End Property
    Public Sub RecalculateScroll()
        If Not Overflow Then
            If ScrollHeight > Box.RenderArea.Height Then
                UpdateBoundClipOfRect(sliderRect)
            End If
            ScrollTop = 0F
            ScrollBottom = Box.ContentArea.Height
            For Each c As Element In Contents
                MathF.LetMin(ScrollTop, c.Box.RenderArea.Top)
                MathF.LetMax(ScrollBottom, c.Box.RenderArea.Bottom)
                c.UpdateBoundClip()
            Next
            ScrollBottom += Box.RenderArea.Height - Box.ContentArea.Height
            MathF.LetBetween(Box.ContentArea.DeltaY, Box.RenderArea.Height - ScrollBottom, -ScrollTop)
            If ScrollHeight > Box.RenderArea.Height Then
                sliderRect.SetPointSizeRect(Box.RenderArea.Right - 10,
                                        Box.RenderArea.Top + (ScrollY - ScrollTop) / ScrollHeight * Box.RenderArea.Height,
                                        10.0F, Box.RenderArea.Height / ScrollHeight * Box.RenderArea.Height)

                UpdateBoundClipOfRect(sliderRect)
            End If
        End If
    End Sub
#End Region
#Region "<Contents>"
    Public Property Contains As Element()
        Get
            Return Contents.ToArray
        End Get
        Set(value As Element())
            For Each e As Element In value
                AddContent(e)
            Next
        End Set
    End Property
    Public Sub AddContent(c As Element)
        Contents.Add(c)
        c.Init(Me)
        AddChildHandler(c)
    End Sub
    Public Sub RemoveContent(c As Element)
        Contents.Remove(c)
        RemoveChildHandler(c)
    End Sub
    Protected Sub AddChildHandler(c As Element)
        AddHandler c.OnUpdate, AddressOf ReceiveChildUpdates
        AddHandler c.OnUpdateMe, AddressOf ReceiveChildUpdatesIt
        AddHandler c.OnClipUpdate, AddressOf RecieveBoundClip
        AddHandler c.OnTransformedClipUpdate, AddressOf RecieveTransformedClip
    End Sub
    Protected Sub RemoveChildHandler(c As Element)
        RemoveHandler c.OnUpdate, AddressOf ReceiveChildUpdates
        RemoveHandler c.OnUpdateMe, AddressOf ReceiveChildUpdatesIt
        RemoveHandler c.OnClipUpdate, AddressOf RecieveBoundClip
        RemoveHandler c.OnTransformedClipUpdate, AddressOf RecieveTransformedClip
    End Sub

#End Region
#Region "<Bound>"
    Public Overrides Sub InitClip()
        If Overflow Then
            boundBox.SetClipInit(Rect2F.Infinity)
        Else
            'boundBox.SetClipInit(Rect2F.Infinity)
            boundBox.SetClippedClipInit(Box.RenderArea)
        End If
    End Sub
    Public Sub UpdateAllChildrenClip()
        For Each c As Element In Contents
            c.UpdateBoundClip()
        Next
    End Sub

    Public Sub UpdateChildrenClipFrom(index As Integer)
        For i As Integer = index To Contents.Count - 1
            Contents(i).UpdateBoundClip()
        Next
    End Sub
    Public Overridable Sub UpdateChildBoundClip()
        If Overflow Then
            UpdateBoundClipOfRect(boundBox)
        Else
            If boundBox.SetClipMax(0F, 0F, Box.RenderArea) Then

                UpdateBoundClipOfRect(boundBox)
            End If
        End If
    End Sub
    Public Overrides Sub RecieveBoundClip(clip As Rect2F)
        If Not Overflow Then
            If Not Box.RenderArea.CheckIn(Box.ContentArea.Left, Box.ContentArea.Top, clip) Then Exit Sub
        End If
        BoundUsed = True
        If boundBox.SetClipBound(Box.ReltX, Box.ReltY, clip) Then
            UpdateChildBoundClip()
        End If
        'Debug.Print("--" & boundBox.ToString)
    End Sub
    Public Overridable Sub RecieveTransformedClip(p() As Point2F)
        If Not Overflow Then
            If Not Box.RenderArea.CheckIn(Box.ReltX, Box.ReltY, p) Then Exit Sub
        End If
        BoundUsed = True
        If boundBox.SetClipBound(Box.ReltX, Box.ReltY, p) Then
            UpdateChildBoundClip()
        End If
    End Sub
#End Region
#Region "<Update>"
    Public Overridable Sub ReceiveChildUpdatesIt(sender As Element)
        sender.UpdateMe(Box.ContentArea.Size)
        RecalculateScroll()
    End Sub
    Public Overridable Sub ReceiveChildUpdates(sender As Element)
        sender.Update(Box.ContentArea.Size)
        RecalculateScroll()
    End Sub
    Public Sub UpdateAllChildren()
        Dim rel As Size2F = Box.ContentArea.Size
        For Each c As Element In Contents
            c.Update(rel)
        Next
    End Sub
    Public Sub UpdateAllChildren_Scroll()
        UpdateAllChildren()
        RecalculateScroll()
    End Sub
    Public Overrides Sub Update(rel As Size2F)
        UpdateMe(rel)
        UpdateAllChildren_Scroll()
    End Sub
#End Region
#Region "<Paint>"
    Public Overrides Sub Paint_PrepareOpacity()
        If Overflow Then
            If Opacity <> 1.0F Then
                r.PushLayer(Opacity)
                PaintBackground()
                Paint()
                PaintBorder()
                r.PopLayer()
            Else
                PaintBackground()
                Paint()
                PaintBorder()
            End If
        Else
            PaintBackground()
            r.PushLayer(Box.RenderArea, Opacity)
            Paint()
            PaintBorder()
            r.PopLayer()
        End If
    End Sub
    Public Overrides Sub Paint()
        r.AddVector(Box.ReltX, Box.ReltY)
        For Each e As Element In Contents
            e.BeginPaint()
        Next
        r.SubVector(Box.ReltX, Box.ReltY)

        If Not Overflow Then
            If ScrollHeight > Box.RenderArea.Height Then
                sliderBrush.FillRectangle(sliderRect)
            End If
        End If
    End Sub

#End Region
End Class
Public Class Bar
    Inherits Div
    Public Property MinWidth As Measure = New NullMeasure()
    Public Property MinHeight As Measure = New NullMeasure()
    Public Property VerticalExpand As Boolean = True
    Public Property HorizontalExpand As Boolean = False

    Private width As New AbsoluteMeasure(0F)
    Private height As New AbsoluteMeasure(0F)
    Public Overrides Sub NewContents()
        Contents = New List(Of Element)
    End Sub
    Public Sub RecalculateExpand_End()
        MinWidth.RenderLength = MinWidth.GetPixels(lastRel, Orientation.Horizontal)
        MinHeight.RenderLength = MinHeight.GetPixels(lastRel, Orientation.Vertical)
        If HorizontalExpand Then Box.Width = width
        If VerticalExpand Then Box.Height = height
        If HorizontalExpand Then
            Dim right As Single = Box.RenderArea.Left
            For Each c As Element In Contents
                MathF.LetMax(right, c.Box.RenderArea.Right)
            Next
            width.Value = Math.Max(
                right + Box.RenderArea.Width - Box.ContentArea.Width,
                MinWidth.RenderLength)
        End If
        If VerticalExpand Then
            Dim bottom As Single = 0F
            For Each c As Element In Contents
                MathF.LetMax(bottom, c.Box.RenderArea.Bottom)
            Next
            height.Value = Math.Max(
                bottom + Box.RenderArea.Height - Box.ContentArea.Height,
                MinHeight.RenderLength)
        End If
        UpdateMe()
    End Sub
    Public Overrides Sub Update(rel As Size2F)
        UpdateMe(rel)
        UpdateAllChildren()
        RecalculateExpand_End()
    End Sub
    Public Overrides Sub ReceiveChildUpdates(sender As Element)
        BoundUsed = True
        sender.Update(Box.ContentArea.Size)
        RecalculateExpand_End()
    End Sub
    Public Overrides Sub ReceiveChildUpdatesIt(sender As Element)
        BoundUsed = True
        sender.UpdateMe(Box.ContentArea.Size)
        RecalculateExpand_End()
    End Sub
End Class
Public Class Flow
    Inherits Div
    Dim cons As List(Of Element)
    Public Overrides Sub NewContents()
        cons = New List(Of Element)
        Contents = cons 'New List(Of Element)
    End Sub
    Protected Function GetLineHead(index As Integer) As Integer
        Return index - Contents(index).Row
    End Function
    Protected Function GetPrevLineHead(headIndex As Integer) As Integer
        If headIndex > 0 Then
            Return GetLineHead(headIndex - 1)
        Else
            Return 0
        End If
    End Function
    Protected Function IsLineHead(index As Integer)
        Return Contents(index).Row = 0
    End Function
    Public Sub RecalculateFlow(index As Integer)
        Dim relSize As Size2F = Box.ContentArea.Size
        Dim lineTop As Single, lineRight As Single
        Dim lineHeight As Single
        Dim r As Rect2F
        Dim width As Single, height As Single
        Dim e As Element

        Dim lineRow As Integer
        Dim beginIndex As Integer
        If IsLineHead(index) Then
            beginIndex = GetPrevLineHead(index)
            lineTop = Contents(beginIndex).Box.RenderArea.DeltaY
        Else
            beginIndex = GetLineHead(index)
            lineTop = Contents(beginIndex).Box.RenderArea.DeltaY
        End If
        For i As Integer = beginIndex To Contents.Count - 1

            e = Contents(i)
            e.UpdateBoundClip()
            r = e.Box.RenderArea
            width = e.Box.TotalWidth
            height = e.Box.TotalHeight
            If lineRight + width <= relSize.Width Then
                r.SetDelta(lineRight, lineTop)
                e.UpdateBoundClip()
                lineRight += width
                MathF.LetMax(lineHeight, height)
                e.Row = lineRow
                lineRow += 1
                Continue For
            End If
            lineTop += lineHeight
            r.SetDelta(0F, lineTop)
            e.UpdateBoundClip()
            lineRight = width
            lineHeight = height
            e.Row = lineRow
            lineRow = 0
        Next

        RecalculateScroll()
    End Sub
    Public Overrides Sub Update(rel As Size2F)
        UpdateMe(rel)
        UpdateAllChildren()
        RecalculateFlow(0)
    End Sub
    Public Overrides Sub ReceiveChildUpdatesIt(sender As Element)
        'UpdateBoundClip()
        sender.UpdateMe(Box.ContentArea.Size)
        RecalculateFlow(cons.IndexOf(sender))
    End Sub
    Public Overrides Sub ReceiveChildUpdates(sender As Element)
        'UpdateBoundClip()
        sender.Update(Box.ContentArea.Size)
        RecalculateFlow(cons.IndexOf(sender))
        'UpdateAllChildren()
    End Sub
End Class

Public Class StackBoxes
    Public Property Orientation As Orientation
    Public Property Boxes As New List(Of Box)
    Public Sub Update(rel As Size2F)
        Dim ws(Boxes.Count - 1) As Measure
        Dim hs(Boxes.Count - 1) As Measure
        For i As Integer = 0 To Boxes.Count - 1
            ws(i) = Boxes(i).Width
            hs(i) = Boxes(i).Height
        Next
        If Orientation = Orientation.Horizontal Then
            Measures.UpdateDimension(rel, Orientation.Horizontal, ws)
            Measures.UpdateMeasures(rel, Orientation.Vertical, hs)
            Dim left As Single
            For Each b As Box In Boxes
                b.RenderArea.SetDoublePointRect(left, 0F,
                                                 b.Width.RenderLength + left,
                                                 b.Height.RenderLength)
                b.ContentArea.SetPointSizeRect(0F, 0F, b.RenderArea.Width, b.RenderArea.Height)
                left += b.Width.RenderLength
            Next
        Else
            Measures.UpdateMeasures(rel, Orientation.Horizontal, ws)
            Measures.UpdateDimension(rel, Orientation.Vertical, hs)
            Dim top As Single
            For Each b As Box In Boxes
                b.RenderArea.SetDoublePointRect(0F, top,
                                                 b.Width.RenderLength,
                                                 b.Height.RenderLength + top)
                b.ContentArea.SetPointSizeRect(0F, 0F, b.RenderArea.Width, b.RenderArea.Height)
                top += b.Height.RenderLength
            Next
        End If
    End Sub
    Default Public Property Item(index As Integer) As Box
        Get
            Return Boxes(index)
        End Get
        Set(value As Box)
            Boxes(index) = value
        End Set
    End Property
    Public Sub New(ori As Orientation, ParamArray ms() As Measure)
        Orientation = ori
        Dim b As Box
        For Each m As Measure In ms
            b = New Box().SetLength(1 - ori, Measures.Auto).SetLength(ori, m)
            Boxes.Add(b)
        Next
    End Sub
End Class
Public Class Stack
    Inherits Div
    Public Property StackBoxes As StackBoxes
    Protected cons As New List(Of Div)

    Default Public ReadOnly Property Slot(index As Integer) As Div
        Get
            Return cons(index)
        End Get
    End Property

    Public Sub AddAt(index As Integer, e As Element)
        cons(index).AddContent(e)
        e.Init(cons(index))
    End Sub
    Public Sub RemoveAt(index As Integer, e As Element)
        cons(index).RemoveContent(e)
    End Sub

    Public Overrides Sub Init(father As Div)
        MyBase.Init(father)

        Dim d As Div
        Dim i As Integer = -1
        For Each b As Box In StackBoxes.Boxes
            i += 1
            d = New Div() With {.Box = b,
                .Name = Name & "-innerDiv(" & i & ")"}
            AddContent(d)
            cons.Add(d)
        Next
    End Sub
    Public Overrides Sub NewContents()
        Contents = New List(Of Element)
    End Sub
    Public Sub New(sb As StackBoxes)
        StackBoxes = sb
    End Sub

    Public Overrides Sub Update(rel As Size2F)
        Box.Update(rel, BorderWidth)
        StackBoxes.Update(Box.ContentArea.Size)
        For i As Integer = 0 To Contents.Count - 1
            For Each e As Element In cons(i).Contents
                e.Update(cons(i).Box.ContentArea.Size)
            Next
        Next
    End Sub
End Class

