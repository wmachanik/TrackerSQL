<%@ Page Language="C#" %>
<!DOCTYPE html>
<html>
<head>
    <title>Bare timing test</title>
</head>
<body>
    <h1>TEST PAGE</h1>
    <p>No Site.Master, no ScriptManager, no database.</p>
    <p>Served at: <%= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") %></p>
    <p>If this is fast but System Tools is slow, look at master/page code or auth — not IIS itself.</p>
    <p>Timings also go to App_Data\timing.log when EnableRequestTiming=true.</p>
</body>
</html>
