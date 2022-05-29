Option Explicit

Const msiOpenDatabaseModeTransact = 1

Dim msiPath : msiPath = Wscript.Arguments(0)

Dim installer
Set installer = Wscript.CreateObject("WindowsInstaller.Installer")
Dim database
Set database = installer.OpenDatabase(msiPath, msiOpenDatabaseModeTransact)

Dim query
    query = "INSERT INTO Property(Property, Value) VALUES('DISABLEADVTSHORTCUTS', '1')"
Dim view
Set view = database.OpenView(query)
view.Execute
database.Commit