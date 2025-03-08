<%@ Page Language="C#" %>
<!DOCTYPE html>
<html>
<body>
    <h1>Información del servidor</h1>
    <p>Runtime: <%= System.Environment.Version %></p>
    <p>Path: <%= Request.PhysicalApplicationPath %></p>
</body>
</html>