ConsoleApp.exe -repo "https://github.com/MicrosoftDocs/Dynamics-365-unified-Operations-public" -json articles/ -replaceUrl "https://docs.microsoft.com/en-us/dynamics365/unified-operations/" -out "Dynamics-365-unified-Operations-public" -conceptualLog "logs\conceptualLog.csv" -tocLog "logs\tocLog.csv"

pause