Module WINMAIN

    Public Function GetHttpContent(url As String) As String '这里是加载URL的函数
        Try
            Dim req As HttpWebRequest = WebRequest.CreateHttp(url), resp As HttpWebResponse, sol$
            With req
                .UserAgent = "Mozilla/5.0 (Windows NT 8.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.4044.138 Safari/537.36"
                .Accept = "*/*"
                .Method = "GET"
                .Timeout = 15000
                .Headers.Add("accept-encoding", "gzip, deflate")
            End With
            resp = req.GetResponse
            Select Case resp.ContentEncoding.ToLower'区分GZIP压缩和普通编码流
                Case "gzip"
                    Using z As New GZipStream(resp.GetResponseStream, CompressionMode.Decompress)
                        Using sr As New StreamReader(z, Encoding.UTF8)
                            sol = sr.ReadToEnd
                        End Using
                    End Using
                    Exit Select
                Case "deflate"
                    Using z As New DeflateStream(resp.GetResponseStream, CompressionMode.Decompress)
                        Using sr As New StreamReader(z, Encoding.UTF8)
                            sol = sr.ReadToEnd
                        End Using
                    End Using
                    Exit Select
                Case Else
                    Using sr As New StreamReader(resp.GetResponseStream, Encoding.UTF8)
                        sol = sr.ReadToEnd
                    End Using
                    Exit Select
            End Select
            Return sol
        Catch EX As Exception
            Console.BackgroundColor = ConsoleColor.Red
            Console.ForegroundColor = ConsoleColor.White
            Console.WriteLine("-------------ERROR------------")
            Console.WriteLine(EX.ToString)
            Console.WriteLine("-------------ERROR------------")
            Console.ResetColor()
            Return ""
            Exit Function
        End Try
    End Function




    Sub Main()
        Try
            Console.Title = " BiliBili Packs "
            Application.VisualStyleState = VisualStyles.VisualStyleState.ClientAndNonClientAreasEnabled
            Application.EnableVisualStyles()

            Dim KEYWORDS As String = "MIAIONE"
            If Command() = Nothing Then
                Console.Write("输入关键词：")
                KEYWORDS = Console.ReadLine()
            Else
                Dim exs As String() = Command.ToLower.Split(" ")
                Select Case exs(0)
                    Case "/u"
                        KEYWORDS = exs(1)
                    Case "-u"
                        KEYWORDS = exs(1)
                End Select
            End If
            Dim MAX_CON As Integer = 0
            Dim AKR As Document = Nothing
            Try '这里先访问一次，判断总页数，不过我发现好像都是50，最多50
                AKR = NSoupClient.Parse(GetHttpContent(String.Format("https://search.bilibili.com/video?keyword={0}&page=1", KEYWORDS)))
            Catch EX As Exception
                Console.BackgroundColor = ConsoleColor.Red
                Console.ForegroundColor = ConsoleColor.White
                Console.WriteLine("-------------ERROR------------")
                Console.WriteLine(EX.ToString)
                Console.WriteLine("-------------ERROR------------")
                Console.ResetColor()
            End Try
            Dim EKR As Elements = AKR.GetElementsByTag("li")
            'Console.WriteLine(EKR)
            For Each gr In EKR
                'Console.WriteLine(gr)
                If gr.Attr("class") = ("page-item last") Then
                    Dim NSP = NSoupClient.Parse(gr.ToString)
                    'Console.WriteLine(NSP)
                    MAX_CON = Integer.Parse(NSP.GetElementsByTag("button").Text)

                End If
            Next
            Dim ID As Integer = 0
            Dim Consti As Integer = 0
            Dim dir As String = ""
            For I As Integer = 1 To MAX_CON
                Dim rnds As New Random(Guid.NewGuid.GetHashCode)
                Dim timestamp As Integer = rnds.Next(1, 3000) '随机延时，要不然。。。。
                Thread.Sleep(timestamp)
                Dim doc As Document = Nothing
                Try
                    doc = NSoupClient.Parse(GetHttpContent(String.Format("https://search.bilibili.com/video?keyword={0}&page={1}", KEYWORDS, I)))
                Catch EX As Exception
                    Console.BackgroundColor = ConsoleColor.Red
                    Console.ForegroundColor = ConsoleColor.White
                    Console.WriteLine("-------------ERROR------------")
                    Console.WriteLine(EX.ToString)
                    Console.WriteLine("-------------ERROR------------")
                    Console.ResetColor()
                End Try
                Dim ele As Elements = doc.GetElementsByTag("a") '获取a标签
                For Each Lis In ele '开始循环
                    GC.Collect()
                    If (Lis.Attr("href") <> "") And (Lis.Attr("href").Contains("//www.bilibili.com/video/BV") = True) Then
                        Consti += 1

                        If Consti > 1 Then '这里莫名有重复
                            If dir <> "https:" + Lis.Attr("href").Replace("?from=search", "") Then
                                ID += 1
                                Console.BackgroundColor = ConsoleColor.Green
                                Console.ForegroundColor = ConsoleColor.White
                                Console.Write("|" + ID.ToString.PadLeft(8, "0") + "|")
                                Console.ResetColor()
                                Using SW As StreamWriter = New StreamWriter("BV链接.txt", True, Encoding.UTF8)
                                    SW.WriteLine(Lis.Attr("href").Replace("?from=search", "").Replace("//www.bilibili.com/video/", ""))
                                    SW.Flush()
                                    SW.Close()
                                End Using '我们改下，改为只抓取BV号的
                                Console.Write("https:" + Lis.Attr("href").Replace("?from=search", "")) '这里可以写成写入txt的，用来直接保存BV号或链接
                                dir = "https:" + Lis.Attr("href").Replace("?from=search", "") '重复舍弃
                                Console.WriteLine()
                            End If
                        Else
                            ID += 1
                            Console.BackgroundColor = ConsoleColor.Green
                            Console.ForegroundColor = ConsoleColor.White
                            Console.Write("|" + ID.ToString.PadLeft(8, "0") + "|")
                            Console.ResetColor()
                            Using SW As StreamWriter = New StreamWriter("BV链接.txt", True, Encoding.UTF8)
                                SW.WriteLine(Lis.Attr("href").Replace("?from=search", "").Replace("//www.bilibili.com/video/", ""))
                                SW.Flush()
                                SW.Close()
                            End Using
                            Console.Write("https:" + Lis.Attr("href").Replace("?from=search", ""))
                            dir = "https:" + Lis.Attr("href").Replace("?from=search", "")
                            Console.WriteLine()
                        End If
                        Console.Title = " BiliBili Packs | 共" + ID.ToString + "个视频/第" + ID.ToString + "个视频|第" + I.ToString + "页/共" + MAX_CON.ToString + "页"

                    End If
                Next
            Next
        Catch EX As Exception
            Console.BackgroundColor = ConsoleColor.Red
            Console.ForegroundColor = ConsoleColor.White
            Console.WriteLine("-------------ERROR------------")
            Console.WriteLine(EX.ToString)
            Console.WriteLine("-------------ERROR------------")
            Console.ResetColor()
        End Try
        Console.WriteLine("按任意键退出")
        Console.ReadKey()
    End Sub


End Module
