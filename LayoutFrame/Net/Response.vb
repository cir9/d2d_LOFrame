
Imports System.Net
Imports System.IO
Imports System.Dynamic
Imports System.Threading

Public Class DynamicDictionary
    Inherits DynamicObject
    Public Dictionary As New Dictionary(Of String, String)
    ReadOnly Property Count As Integer
        Get
            Return Dictionary.Count
        End Get
    End Property
    Public Function Remove(key As String) As Boolean
        Return Dictionary.Remove(key)
    End Function
    Public Overrides Function TryGetMember(
        ByVal binder As GetMemberBinder,
        ByRef result As Object) As Boolean

        Return Dictionary.TryGetValue(binder.Name, result)
    End Function
    Public Overrides Function TrySetMember(
        ByVal binder As SetMemberBinder,
        ByVal value As Object) As Boolean

        Dictionary(binder.Name) = value
        Return True
    End Function
End Class
Public Class ParamCollection
    Inherits DynamicDictionary
    Public Const CharList As String = ",="
    Public Sub Add(key As String, value As String)
        Dictionary(key) = value
    End Sub
    Public Sub Clear()
        Dictionary.Clear()
    End Sub
    Public Function ToParam() As String
        If Dictionary IsNot Nothing Then
            Dim strList As New List(Of String)
            For Each key As String In Dictionary.Keys
                strList.Add(WebUtility.UrlEncode(key) & "=" & WebUtility.UrlEncode(Dictionary(key)))
            Next
            Return "?" & String.Join("&", strList)
        End If
        Return vbNullString
    End Function
    Public Overrides Function ToString() As String
        If Dictionary IsNot Nothing Then
            Dim strList As New List(Of String)
            For Each key As String In Dictionary.Keys
                strList.Add(key & "=" & Dictionary(key))
            Next
            Return String.Join("&", strList)
        End If
        Return vbNullString
    End Function
    Default Public Property Item(ByVal key As String) As String
        Get
            Return Dictionary(key)
        End Get
        Set(ByVal Value As String)
            Dictionary(key) = Value
        End Set
    End Property
    Public Sub New(ParamArray keyArray() As String)
        Dictionary = New Dictionary(Of String, String)
        For i As Integer = 0 To keyArray.LongLength - 1 Step 2
            Dictionary.Add(keyArray(i), keyArray(i + 1))
        Next
    End Sub
    Public Sub New(str As String)
        Dictionary = New Dictionary(Of String, String)
        Dim keyArray() As String = str.Split(CharList.ToCharArray)
        For i As Integer = 0 To keyArray.LongLength - 1 Step 2
            Dictionary.Add(keyArray(i), keyArray(i + 1))
        Next
    End Sub
End Class
Public Class HeaderCollection
    Inherits ParamCollection
    Public Function ToHeaders() As WebHeaderCollection
        Dim headerC As New WebHeaderCollection
        For Each Key As String In Dictionary.Keys
            headerC.Add(Key, Dictionary(Key))
        Next
        Return headerC
    End Function
    Public Sub SetRequestHeaders(req As HttpWebRequest)
        For Each key As String In Dictionary.Keys
            Select Case key
                Case "User-Agent"
                    req.UserAgent = Dictionary(key)
                Case "Referer"
                    req.Referer = Dictionary(key)
                Case Else
                    req.Headers.Add(key, Dictionary(key))
            End Select
        Next
    End Sub
    Public Sub New(ParamArray keyArray() As String)
        Dictionary = New Dictionary(Of String, String)
        For i As Integer = 0 To keyArray.LongLength - 1 Step 2
            Dictionary.Add(keyArray(i), keyArray(i + 1))
        Next
    End Sub
    Public Sub New(str As String)
        Dictionary = New Dictionary(Of String, String)
        Dim keyArray() As String = str.Split(CharList.ToCharArray)
        For i As Integer = 0 To keyArray.LongLength - 1 Step 2
            Dictionary.Add(keyArray(i), keyArray(i + 1))
        Next
    End Sub
End Class
Public Class Response

    Friend P As HttpWebResponse
    Friend Q As HttpWebRequest
    Public Property Method As String = "GET"
    Public Property Headers As HeaderCollection
    Public Property Params As ParamCollection
    Public Property URL As String
    Public Property Data As String = Nothing
    Public Property TimeOut As Integer = 20000
    Public Property StatusCode As Integer
    Public Sub Close()
        If P IsNot Nothing Then P.Close() : P = Nothing
        If Q IsNot Nothing Then Q.Abort() : Q = Nothing
    End Sub
    Private Sub doPost()
        Dim iParam As String
        If Params IsNot Nothing Then
            iParam = Params.ToParam
        Else
            iParam = ""
        End If
        Dim iURL = URL & iParam
        Select Case Method
            Case "GET"
                Q = WebRequest.Create(iURL)
                Q.ServicePoint.Expect100Continue = False
                Q.Timeout = TimeOut
                If Headers IsNot Nothing Then Headers.SetRequestHeaders(Q)

                P = Q.GetResponse()
                StatusCode = P.StatusCode

                    'Deb.Print(P.StatusCode)
            Case "POST"
                Q = WebRequest.Create(iURL)

                Q.Method = "POST"
                Q.ContentType = "application/x-www-form-urlencoded"
                Q.ServicePoint.Expect100Continue = False
                Q.Timeout = TimeOut
                If Headers IsNot Nothing Then Headers.SetRequestHeaders(Q)

                Dim encoding As New Text.UTF8Encoding()
                Dim bys As Byte() = Text.Encoding.UTF8.GetBytes(Data)
                Q.ContentLength = bys.Length

                Using newStream As Stream = Q.GetRequestStream()
                    newStream.Write(bys, 0, bys.Length)
                    newStream.Close()
                End Using

                P = Q.GetResponse()
                StatusCode = P.StatusCode
        End Select
    End Sub
    Public Function GetResponseData() As String
        doPost()
        Dim responseData As String
        Using responseReader As StreamReader = New StreamReader(P.GetResponseStream(), Text.Encoding.UTF8)
            responseData = responseReader.ReadToEnd()
            responseReader.Close()
        End Using
        Close()
        Return responseData
    End Function
    Public Function GetDownloadStream() As Stream
        doPost()
        Return P.GetResponseStream
    End Function

    Public Sub New(tMethod As String,
                   tURL As String,
                   Optional tHeaders As HeaderCollection = Nothing,
                   Optional tParams As ParamCollection = Nothing,
                   Optional tData As String = Nothing)
        Method = tMethod
        URL = tURL
        Headers = tHeaders
        Params = tParams
        Data = tData
    End Sub

End Class