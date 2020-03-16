
Public Class CleanSet(Of T)
    Implements IEnumerable(Of T)

    Public list As New HashSet(Of T)
    Public cleanList As New HashSet(Of T)

    Public Sub Add(e As T)
        list.Remove(e)
        cleanList.Remove(e)
        list.Add(e)
    End Sub

    Public Sub AddRange(es As IEnumerable(Of T))
        For Each e As T In es
            Add(e)
        Next
    End Sub
    Public Sub Mark(e As T)
        cleanList.Add(e)
    End Sub
    Public Sub Clean()
        For Each u As T In cleanList
            list.Remove(u)
        Next
        cleanList.Clear()
    End Sub
    Public Sub Remove(e As T)
        list.Remove(e)
        cleanList.Remove(e)
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return list.GetEnumerator
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return list.GetEnumerator
    End Function
End Class